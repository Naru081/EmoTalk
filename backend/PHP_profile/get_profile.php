<?php
// プロファイルデータの取得を行うPHP

require_once __DIR__ . '/../common_function.php';

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$user_id = $data['user_id'] ?? "";

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