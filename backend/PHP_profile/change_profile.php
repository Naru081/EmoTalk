<?php
// プロファイルの切り替えを行うPHP

require_once __DIR__ . '/../common_function.php';

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$user_id = $data['user_id'] ?? "";
$prof_id = $data['prof_id'] ?? "";
$current_prof_id = $data['current_prof_id'] ?? "";

// 入力値チェック
// もし現在選択中のプロファイル(current_prof_id)と切り替えようとしているプロファイル(prof_id)が同じ場合はエラーを返し終了
if ($current_prof_id == $prof_id) {
    echo json_encode([
        "success" => false,
        "message" => "このプロファイルは現在使用中です"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// プロファイル切り替え処理-DBuser.php
$result = $DBuser->ChangeProfile($user_id,$prof_id);

// もしプロファイル切り替えが失敗なら
if (!$result['success'])
{
    echo json_encode($result, JSON_UNESCAPED_UNICODE);
    exit;
}

// user_idに対応した全profileデータを取得する-DBprofile.php(ハンバーガーメニューに戻った際に更新されているようにするため)
$profile_data = $DBprofile->GetProfileData($user_id);

// もしプロファイルデータ取得が失敗なら
if (!$profile_data['success'])
{
    echo json_encode($result, JSON_UNESCAPED_UNICODE);
    exit;
}

$result['profiles'] = $profile_data['profiles'];

// 全処理が成功時、successとmessageとprofile_dataを返す
echo json_encode($result, JSON_UNESCAPED_UNICODE);

?>