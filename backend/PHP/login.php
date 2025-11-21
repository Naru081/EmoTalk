<?php
// ログイン処理を行うPHP

require_once '../dbconnect.php';
require_once '../DBPHP/DBuser.php';

$DBuser = new DBUser($pdo);

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$email = $data['email'] ?? "";
$password = $data['password'] ?? "";

// メールアドレスの形式チェック
if ($email === "" || !filter_var($email, FILTER_VALIDATE_EMAIL)) {
    echo json_encode(["success" => false, "message" => "メールアドレスまたはパスワードが不正です"], JSON_UNESCAPED_UNICODE);
    exit;
}

// パスワードが8文字以上16文字内かチェック
if ($password === "" || strlen($password) < 8 || strlen($password) > 16) {
    echo json_encode(["success" => false, "message" => "メールアドレスまたはパスワードが不正です"], JSON_UNESCAPED_UNICODE);
    exit;
}

// ログイン処理-DBuser.php(ユーザデバイス登録用のトークン発行も含む)
$result = $DBuser->LoginUser($email, $password);
echo json_encode($result, JSON_UNESCAPED_UNICODE);

?>