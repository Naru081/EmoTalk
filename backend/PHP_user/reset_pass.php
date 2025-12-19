<?php
// パスワード再設定を行うPHP

require_once __DIR__ . '/../common_function.php';

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$email = $data['user_mail'] ?? ""; 
$newpassword = $data['newpassword'] ?? "";  // unity側の変数名合わせて後日修正

// パスワードが8文字以上16文字内で英数字のみかチェック また、英字と数字は必ず使用することとする
if (
    $newpassword === "" || 
    strlen($newpassword) < 8 ||
    strlen($newpassword) > 16 ||
    !preg_match('/[a-zA-Z]/', $newpassword) || // 英字を含むか
    !preg_match('/[0-9]/', $newpassword) || // 数字を含むか
    !preg_match('/^[a-zA-Z0-9]+$/', $newpassword) // 英数字のみか
) {
    echo json_encode(["success" => false, "message" => "パスワードは半角英字と数字を含め、8文字以上16文字以内で入力してください"], JSON_UNESCAPED_UNICODE);
    exit;
}

// パスワード再設定処理-DBuser.php
$result = $DBuser->ResetPassword($email, $newpassword);

// successとmessageを返す
echo json_encode($result, JSON_UNESCAPED_UNICODE);
?>