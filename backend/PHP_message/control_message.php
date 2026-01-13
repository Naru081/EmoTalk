<?PHP
// 会話(メイン機能)に関してのコントロールを行うPHP
require_once __DIR__ . '/../common_function.php';

// prof_idを取得し、中身がなければエラーを返す
$prof_id = $_POST['prof_id'] ?? '';
$message_content = $_POST['message_content'] ?? '';

if (empty($prof_id)) {
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
        echo json_encode([
            "success" => false,
            "message" => "音声データのテキスト変換に失敗しました"
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    // 変換されたテキストをメッセージ内容として使用
    $message_content = $wisper_res["text"];
} else {
    // テキスト形式の場合、メッセージ内容を取得
    $message_content = $_POST["message_content"] ?? '';

    // message_contentが空の場合はエラーを返す
    if (empty($message_content)) {
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
if (!$result) {
    echo json_encode($result, JSON_UNESCAPED_UNICODE);
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

// CoeiroInk APIのウォーミングアップ-connect_api.php
WarmUpCoeiroInkAPI($model_voice);

// CoeiroInk APIに接続してAIの応答メッセージを音声データに変換-connect_api.php
$voice_wav = ConnectCoeiroInkAPI($model_voice, $response_text_hiragana);

// CoeiroInk APIから音声データの取得に失敗した場合
if (empty($voice_wav)) {
    echo json_encode([
        "success" => false,
        "message" => "CoeiroInk APIから音声データの取得に失敗しました"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// 音声データをBase64エンコード形式に変換
$wav_base64 = base64_encode($voice_wav );

// Unity(C#)に返送するデータを作成
echo json_encode(
    [
        "success" => true,
        "message" => "メインプロセスの処理が完了しました",
        "response_text" => $message_content,
        // "response_text_hiragana" => $response_text_hiragana, // リップシンク用に必要ならば有効化
        "emotion" => $emotion,
        "voice_wav_base64" => $wav_base64
    ],
    JSON_UNESCAPED_UNICODE
);
?>