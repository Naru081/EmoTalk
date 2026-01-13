<?PHP
header('Content-Type: application/json; charset=utf-8');
ini_set('display_errors', '0');
error_reporting(E_ALL);
ob_start();

// 会話(メイン機能)に関してのコントロールを行うPHP
require_once __DIR__ . '/../common_function.php';

// prof_idを取得し、中身がなければエラーを返す
$row = file_get_contents('php://input');
$data = json_decode($row, true);
$prof_id = $data['prof_id'] ?? ($_POST['prof_id'] ?? '');
$message_content = $data['message_content'] ?? ($_POST['message_content'] ?? '');


if (empty($prof_id)) {
    ob_clean();
    echo json_encode([
        "success" => false,
        "message" => "prof_idが設定されていません"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// 音声ファイルかテキストかを判定
if (!empty($_FILES['audio']['tmp_name'])) {
    // 音声ファイルがアップロードされている場合
    $tmpFile = $_FILES['audio']['tmp_name'];

    // Wisper APIに接続して音声データをテキストに変換-connect_api.php
    $wisper_res = ConnectWisperAPI($tmpFile);

    // もしWisper APIのレスポンスに"text"キーが存在しない場合はエラーを返す
    if (!$wisper_res['text']) {
        ob_clean();
        echo json_encode([
            "success" => false,
            "message" => "音声データのテキスト変換に失敗しました"
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    // 変換されたテキストをメッセージ内容として使用
    $message_content = $wisper_res["text"];
} else {
    // message_contentが空の場合はエラーを返す
    if (empty($message_content)) {
        ob_clean();
        echo json_encode([
            "success" => false,
            "message" => "メッセージ内容が空です"
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }
}

// ユーザのメッセージをDBに保存-send_mesage.php
$message_sender = 0; // ユーザが送信したメッセージなので0を指定
$result = $DBmessage->InsertMessage($prof_id, $message_sender, $message_content);

// DBにユーザが送信したテキストメッセージを保存失敗した場合
if (!$result || (is_array($result) && isset($result['success']) && !$result['success'])) {
    ob_clean();
    echo json_encode([
        "success" => false,
        "message" => "ユーザメッセージの保存に失敗しました"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// DBからプロファイル情報を取得-get_profile.php
$profile_config = $DBprofile->GetProfileConfig($prof_id);

$model_id = $profile_config['model_id'];
$prof_chara = $profile_config['prof_chara'];
$prof_tone = $profile_config['prof_tone'];
$prof_fp = $profile_config['prof_fp'];

// DBからモデルの名前とVOICEのUUIDを取得-get_model.php
$model_data = $DBmodel->GetModelNameVoice($model_id);
$model_name = $model_data['model_name'];
$model_voice = $model_data['model_voice'];

// 過去の会話履歴を取得-DBmessage.php
$recent_messages = $DBmessage->GetRecentMessages($prof_id, 6);

// ChatGPT APIに接続してAIの応答メッセージを取得する
$ChatGPT_res = ConnectChatGPTAPI(
    $model_name,
    $message_content,
    $prof_chara,
    $prof_tone,
    $prof_fp,
    $recent_messages
);

// ChatGPT APIからの応答に失敗した場合
if (!$ChatGPT_res['success']) {
    echo json_encode($ChatGPT_res, JSON_UNESCAPED_UNICODE);
    exit;
}

// AIの応答メッセージとそのひらがなのみverとAIの感情データを変数に格納
$message_content = $ChatGPT_res['response_text'];
$response_text_hiragana = $ChatGPT_res['response_text_hiragana'];
$emotion = $ChatGPT_res['emotion'];

// AIの応答メッセージをDBに保存-send_message.php
$message_sender = 1; // AIが送信したメッセージなので1を指定
$result = $DBmessage->InsertMessage($prof_id, $message_sender, $message_content);

// DBにユーザが送信したテキストメッセージを保存失敗した場合
if (!$result['success']) {
    echo json_encode($result, JSON_UNESCAPED_UNICODE);
    exit;
}

// CoeiroInk APIのウォーミングアップ
WarmUpCoeiroInkAPI($model_voice);

// 音声生成
$voice_wav = ConnectCoeiroInkAPI($model_voice, $response_text_hiragana);

// 音声データをBase64化（失敗時は空）
$wav_base64 = "";
if (!empty($voice_wav)) {
    $wav_base64 = base64_encode($voice_wav);
}

// Unity(C#)に返送するデータを作成
$response = [
    "success" => true,
    "message" => empty($voice_wav)
        ? "音声生成に失敗しました（テキストのみ表示します）"
        : "OK",
    "response_text" => $message_content,
    "emotion" => $emotion,
    "voice_wav_base64" => $wav_base64
];

ob_clean();
echo json_encode($response, JSON_UNESCAPED_UNICODE);
exit;
?>