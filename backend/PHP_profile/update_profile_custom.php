<?php
// プロファイルのカスタム(性格、口調、一人称)の変更を行うPHP

require_once __DIR__ . '/../common_function.php';

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$user_id = $data['user_id'] ?? "";
$prof_id = $data['prof_id'] ?? "";
$prof_chara = $data["prof_chara"] ??"";     // Profile性格
$prof_tone = $data["prof_tone"] ??"";       // Profile口調
$prof_fp = $data["prof_fp"] ??"";           // Profile一人称

// -------------------- 入力値チェック --------------------

// prof_charaが空の場合、もしくは文字数が20文字を越える場合
if ($prof_chara === "" || mb_strlen($prof_chara) > 20) {
    echo json_encode(["success" => false, "message" => "性格は20文字以内で入力してください"], JSON_UNESCAPED_UNICODE);
    exit;
}

// prof_toneが空の場合、もしくは文字数が20文字を越える場合
if ($prof_tone === "" || mb_strlen($prof_tone) > 20) {
    echo json_encode(["success" => false, "message" => "口調は20文字以内で入力してください"], JSON_UNESCAPED_UNICODE);
    exit;
}

// prof_fpが空の場合、もしくは文字数が20文字を越える場合
if ($prof_fp === "" || mb_strlen($prof_fp) > 20) {
    echo json_encode(["success" => false, "message" => "一人称は20文字以内で入力してください"], JSON_UNESCAPED_UNICODE);
    exit;
}

// -------------------------------------------------------

// プロファイルカスタム変更処理-DBprofile.php 
$result = $DBprofile->UpdateProfileCustom($prof_id, $prof_chara, $prof_tone, $prof_fp);

// もしプロファイルカスタム変更が失敗なら
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