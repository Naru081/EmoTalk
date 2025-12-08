<?PHP
// 会話(メイン機能)に関してのコントロールを行うPHP

require_once '../dbconnect.php';
require_once '../DBPHP/DBmessage.php';
require_once '../DBPHP/DBprofile.php';
require_once 'send_message.php';

$DBmessage = new DBmessage($pdo);
$DBprofile = new DBprofile($pdo);

// Unity(C#)から送られてきたデータがjson形式か音声データ形式かで処理を分岐させる
$contentType = $_SERVER["CONTENT_TYPE"] ?? '';

if (strpos($contentType, 'application/json') !== false) {
    // JSON形式の場合

    // unityからデータを取得(JSON形式)
    $row = file_get_contents('php://input');
    // JSONデータを連想配列に変換
    $data = json_decode($row, true);

    // 受け取ったデータを変数に格納 (空の場合は空白を代入)
    $prof_id = $data['prof_id'] ?? "";
    $message_content = $data['message_content'] ?? "";
}
else if (isset($_FILES['voice_file'])) {
    // 音声データの場合

    $prof_id = $_POST['prof_id'];
    $message_sender = $_POST['message_sender'];

    $tmpFile = $_FILES['voice_file']['tmp_name'];   // PHPが受け取ったファイルを一時的に保存している場所(xampp/tmp/xxxxxx)
    $mime = $_FILES['voice_file']['type'];       // ファイルのMIMEタイプ(audio/wavなど) ファイル形式のこと
    $name = $_FILES['voice_file']['name'];       // Unity側で指定したファイル名(voice.wavなど)

    // Wisper APIに接続して音声データをテキストに変換-connect_api.php
    $wisper_res = ConnectWisperAPI($tmpFile, $mime, $name);

    // もしWisper APIのレスポンスに"text"キーが存在しない場合はエラーを返す
    if (!isset($wisper_res['text'])) {
        echo json_encode([
            "success" => false,
            "message" => "音声データのテキスト変換に失敗しました"
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    $message_content = $wisper_res["text"]; // 変換されたテキストをメッセージ内容として使用
} else {
    // son形式でも音声データ形式でもない場合

    echo json_encode([
        "success" => false,
        "message" => "音声データのテキスト変換処理に失敗しました:不正なデータ形式"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// DBにユーザが送信したテキストメッセージを保存-send_mesage.php
$message_sender = 0; // ユーザが送信したメッセージなので0を指定
$result = insert_message($pdo, $prof_id, $message_sender, $message_content);

// DBにユーザが送信したテキストメッセージを保存失敗した場合
if (!$result['success']) {
    echo json_encode($result, JSON_UNESCAPED_UNICODE);
    exit;
}

// ChatGpt APIに渡すための現時点で選択されているプロファイルのモデル設定を取得-DBprofile.php
$profile_config = $DBprofile->GetProfileConfig($prof_id);
$profile_config = $profile_config['profiles'];  // successとmessageを破棄し、profilesデータ(prof_chara, prof_tone, prof_fp)だけを取得

// ChatGPT APIに接続してAIの応答メッセージを取得する
?>