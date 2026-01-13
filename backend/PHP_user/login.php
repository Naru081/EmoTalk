<?php
// ログイン処理とトークン発行リクエストを行うPHP

require_once __DIR__ . '/../common_function.php';

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$email = $data['user_mail'] ?? "";
$password = $data['user_pass'] ?? "";

// メールアドレスの形式チェック
if ($email === "" || !filter_var($email, FILTER_VALIDATE_EMAIL)) {
    echo json_encode(["success" => false, "message" => "メールアドレスまたはパスワードが不正です"], JSON_UNESCAPED_UNICODE);
    exit;
}

// パスワードが8文字以上16文字内で英数字のみかチェック また、英字と数字は必ず使用することとする
if (
    $password === "" || 
    strlen($password) < 8 ||
    strlen($password) > 16 ||
    !preg_match('/[a-zA-Z]/', $password) || // 英字を含むか
    !preg_match('/[0-9]/', $password) || // 数字を含むか
    !preg_match('/^[a-zA-Z0-9]+$/', $password) // 英数字のみか
) {
    echo json_encode(["success" => false, "message" => "メールアドレスまたはパスワードが不正です"], JSON_UNESCAPED_UNICODE);
    exit;
}

// ログイン処理-DBuser.php
$result = $DBuser->LoginUser($email, $password);

// ログイン処理に失敗した場合
if (!$result['success']) {
    echo json_encode($result, JSON_UNESCAPED_UNICODE);
    exit;
}
// successとmessageとuser_idとuser_mailとuser_currentprofとtokenを返す
echo json_encode($result, JSON_UNESCAPED_UNICODE);

?>