<?php
// 新規登録処理を行うPHP

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
$result = $DBuser->isEmailDuplicate($email);
if ($result) {
    echo json_encode([
        "success" => false,
        "message" => "そのメールアドレスは既に登録されています"
    ]
    );
    exit;
} 

// ユーザ情報登録-DBuser.php
$result = $DBuser->registerUser($email, $password);

if (!$result['success']) {
    // 登録失敗
    echo json_encode($result, JSON_UNESCAPED_UNICODE);
    exit;
}

//　ユーザ情報登録の結果からuser_idを取得
$user_id = $result['user_id'];

// デフォルトプロファイル作成-DBprofile.php
$createdefaultprofile = $DBprofile->createDefaultProfile($user_id);

if (!$createdefaultprofile['success']) {
    // プロファイル作成失敗
    echo json_encode(["success" => false, "message" => "プロファイルの作成に失敗しました"], JSON_UNESCAPED_UNICODE);
    exit;
}

// profileテーブルから、user_idに対応したprof_idが一番小さいデータを取得する-DBprofile.php(新規登録直後の選択プロファイルを設定するため)
$defaultset_profile = $DBprofile->GetDefaultSetProfile($user_id);

// 取得に失敗した場合
if (!$defaultset_profile["success"]) {
    echo json_encode($defaultset_profile, JSON_UNESCAPED_UNICODE);
    exit;
}

// userテーブルのuser_currentprofにデフォルトセットプロファイル(defaultset_profile)をセット-DBuser.php
$prof_id = $defaultset_profile['min_prof_id'];  // 連想配列の中の値を取得
$defaultset_profile_result = $DBuser->ChangeProfile($user_id, $prof_id);

if (!$defaultset_profile_result['success']) {
    // プロファイル切り替え失敗
    echo json_encode(["success"=> false, "message"=> "デフォルトセットプロファイルの登録に失敗しました"], JSON_UNESCAPED_UNICODE);
    exit;
}

// 処理が完全に成功したら以下を返す
$finalResult = [
    "success" => true,
    "message" => "ユーザ登録が完了しました",
    "user_id" => $user_id,
];
echo json_encode($finalResult, JSON_UNESCAPED_UNICODE);

?>