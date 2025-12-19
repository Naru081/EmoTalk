<?php
// 新規プロファイル作成を行うPHP

require_once __DIR__ . '/../common_function.php';

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$user_id = $data['user_id'] ?? "";
$model_id = $data['model_id'] ?? "";
$prof_title = $data['prof_title'] ?? "";    // Profile題名
$prof_chara = $data["prof_chara"] ??"";     // Profile性格
$prof_tone = $data["prof_tone"] ??"";       // Profile口調
$prof_fp = $data["prof_fp"] ??"";           // Profile一人称

// -------------------- 入力値チェック --------------------

// prof_titleが空の場合、もしくは文字数が11文字を越える場合
if ($prof_title === "" || mb_strlen($prof_title) > 10) {
    echo json_encode(["success" => false, "message" => "タイトルは10文字以内で入力してください"], JSON_UNESCAPED_UNICODE);
    exit;
}

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

// 新規プロファイル作成-DBprofile.php 
$result = $DBprofile->CreateProfile($user_id, $model_id, $prof_title, $prof_chara, $prof_tone, $prof_fp);

// もしプロファイル作成が失敗なら
if (!$result['success']) {
    // 失敗した場合は失敗結果をJSONで返す
    echo json_encode($result, JSON_UNESCAPED_UNICODE);
    exit;
}

// user_idに対応した全profileデータを取得する-DBprofile.php(ハンバーガーメニューに戻った際に更新されているようにするため)
$profiles = $DBprofile->GetProfileData($user_id);

// もし全プロファイル取得が失敗なら
if (!$profiles['success']) {
    // 失敗した場合は失敗結果をJSONで返す
    echo json_encode($profiles, JSON_UNESCAPED_UNICODE);
    exit;
}

// プロファイル作成に成功し、全プロファイル取得に成功した場合
// successとmessageとprofilesを返す
echo json_encode([
    "success" => true,
    "message" => "プロファイルの作成に成功しました",
    "profiles" => $profiles['profile_data']
], JSON_UNESCAPED_UNICODE); 

?>