<?php
// プロファイルのタイトルの変更を行うPHP

require_once __DIR__ . '/../common_function.php';

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$user_id = $data['user_id'] ?? "";
$prof_id = $data['prof_id'] ?? "";
$prof_title = $data['prof_title'] ?? "";    // Profile題名

// 入力値チェック
// prof_titleが空の場合、もしくは文字数が11文字を越える場合
if ($prof_title === "" || mb_strlen($prof_title) > 10) {
    echo json_encode(["success" => false, "message" => "タイトルは10文字以内で入力してください"], JSON_UNESCAPED_UNICODE);
    exit;
}

// プロファイルタイトル変更処理-DBprofile.php
$result = $DBprofile->UpdateProfileTitle($prof_id, $prof_title);

// もしプロファイルタイトル変更が失敗なら
if (!$result['success'])
{
    echo json_encode($result, JSON_UNESCAPED_UNICODE);
    exit;
}

// user_idに対応した全profileデータを取得する-DBprofile.php(ハンバーガーメニューに戻った際に更新されているようにするため)
$result = $DBprofile->GetProfileData($user_id);

// もしプロファイルデータ取得が失敗なら
if (!$result['success'])
{
    echo json_encode($result, JSON_UNESCAPED_UNICODE);
    exit;
}

// 全処理が成功時、successとmessageとprofile_dataを返す
echo json_encode($result, JSON_UNESCAPED_UNICODE);

?>