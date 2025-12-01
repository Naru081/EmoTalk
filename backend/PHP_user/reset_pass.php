<?php
// パスワード再設定を行うPHP

require_once '../dbconnect.php';
require_once '../DBPHP/DBuser.php';

$DBuser = new DBUser($pdo);

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$email = $data['email'] ?? ""; 
$newpassword = $data['newpassword'] ?? "";  // unity側の変数名合わせて後日修正

// パスワードが8文字以上16文字内かチェック
if ($newpassword === "" || strlen($newpassword) < 8 || strlen($newpassword) > 16) {
    echo json_encode(["success" => false, "message" => "パスワードは8文字以上16文字以内で入力してください"], JSON_UNESCAPED_UNICODE);
    exit;
}

// パスワード再設定処理-DBuser.php
$result = $DBuser->ResetPassword($email, $newpassword);

// successとmessageを返す
echo json_encode($result, JSON_UNESCAPED_UNICODE);
?>