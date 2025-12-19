<?PHP
// prof_idからユーザとAIのメッセージデータの取得を行うPHP

require_once __DIR__ . '/../common_function.php';

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$prof_id = $data['prof_id'] ?? "";

// prof_idからメッセージデータを取得-DBmessage.php
$result = $DBmessage->GetMessageAll($prof_id);

?>