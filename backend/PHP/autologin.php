<?php
// 自動ログイン処理を行うPHP

require_once '../dbconnect.php';
require_once '../DBPHP/DBuser.php';

$DBuser = new DBUser($pdo);

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$localtoken = $data['localtoken'] ?? "";

$result = $DBuser->AutoLoginUser($localtoken);
echo json_encode($result, JSON_UNESCAPED_UNICODE);
?>