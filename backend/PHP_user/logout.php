<?php
// ログアウト処理を行うPHP

require_once __DIR__ . '/../common_function.php';

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$user_id = $data['user_id'] ?? "";

// ログアウト処理-DBuser.php
$result = $DBuser->LogoutUser($user_id);

// successとmessageを返す
echo json_encode($result, JSON_UNESCAPED_UNICODE);
?>