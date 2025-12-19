<?php
// 自動ログイン処理を行うPHP

require_once __DIR__ . '/../common_function.php';

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$token = $data['token'] ?? "";

// 自動ログイン処理-DBuser.php
$result = $DBuser->AutoLoginUser($token);

// user_idとuser_mailとuser_currentprofとsuccessとmessageをJSONで返す
echo json_encode($result, JSON_UNESCAPED_UNICODE);  
?>