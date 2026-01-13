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

// DBから会話履歴を取得
$chat_history = $DBmessage->GetMessageAll($prof_id);

if (!$chat_history['success']) {
    echo json_encode([
        "success" => false,
        "message" => "会話履歴の取得に失敗しました"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}
// Unity(C#)に返送するデータを作成
echo json_encode(
    [
        "success" => true,
        "message" => "チャット履歴の取得に成功しました",
        "message_history" => $chat_history["message_alldata"]
    ],
    JSON_UNESCAPED_UNICODE
);
?>