<?PHP
// prof_idからユーザとAIのメッセージデータの取得を行うPHP

require_once __DIR__ . '/../common_function.php';

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$prof_id = $data['prof_id'] ?? "";

// DBから会話履歴を取得
$chat_history = $DBmessage->GetMessageAll($prof_id);

if (!$chat_history['success']) {
    echo json_encode([
        "success" => false,
        "message" => "会話履歴の取得に失敗しました"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// unity(C#)に返送するデータを作成
echo json_encode(
    [
        "success" => true,
        "message" => "チャット履歴の取得に成功しました",
        "message_history" => $chat_history["message_alldata"]
    ],
    JSON_UNESCAPED_UNICODE
);
?>