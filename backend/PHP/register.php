<?php
// 新規登録処理を行うPHP

require_once '../dbconnect.php';
require_once '../DBPHP/DBuser.php';
require_once '../DBPHP/DBprofile.php';

$DBuser = new DBUser($pdo);
$DBprofile = new DBprofile($pdo);

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$email = $data['email'] ?? "";
$password = $data['password'] ?? "";

// メールアドレスの形式チェック
if ($email === "" || !filter_var($email, FILTER_VALIDATE_EMAIL)) {
    echo json_encode(["success" => false, "message" => "メールアドレスが不正です"], JSON_UNESCAPED_UNICODE);
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
    echo json_encode(["success" => false, "message" => "パスワードは半角英字と数字を含め、8文字以上16文字以内で入力してください"], JSON_UNESCAPED_UNICODE);
    exit;
}

// メールアドレスのDB重複チェック-DBuser.php
if ($DBuser->isEmailDuplicate($email)) {
    echo json_encode([
        "success" => false,
        "message" => "このメールアドレスは既に登録されています"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// ユーザ情報登録-DBuser.php
$result = $DBuser->registerUser($email, $password);

if (!$result) {
    // 登録失敗
    echo json_encode(["success" => false, "message" => "ユーザ登録に失敗しました"], JSON_UNESCAPED_UNICODE);
    exit;
}

//　ユーザ情報登録の結果からuser_idを取得
$user_id = $result['user_id'];

// デフォルトプロファイル作成-DBprofile.php
$createdefaultprofile = $DBprofile->createDefaultProfile($user_id);

if (!$createdefaultprofile) {
    // プロファイル作成失敗
    echo json_encode(["success" => false, "message" => "プロファイルの作成に失敗しました"], JSON_UNESCAPED_UNICODE);
    exit;
}

// successとmessageを返す
echo json_encode($result,JSON_UNESCAPED_UNICODE);

?>