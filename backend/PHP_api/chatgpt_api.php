<?php
require_once __DIR__ . '/../common_function.php';
require_once __DIR__ . '/connect_api.php';

$prof_id = $_POST['prof_id'] ?? '';
$message_content = $_POST['message_content'] ?? '';

if (empty($prof_id) || empty($message_content)) {
    echo json_encode([
        "success" => false,
        "message" => "prof_id または message_content が不足しています"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// プロファイル取得
$profile_config = $DBprofile->GetProfileConfig($prof_id);

$model_id = $profile_config['model_id'];
$prof_chara = $profile_config['prof_chara'];
$prof_tone = $profile_config['prof_tone'];
$prof_fp = $profile_config['prof_fp'];

// モデル名とVOICE UUID
$model_data = $DBmodel->GetModelNameVoice($model_id);
$model_name = $model_data['model_name'];
$model_voice = $model_data['model_voice'];

// 過去メッセージ
$recent_messages = $DBmessage->GetRecentMessages($prof_id, 6);

$message_sender = 0; // ユーザが送信したメッセージなので0を指定
$user_result = $DBmessage->InsertMessage($prof_id, $message_sender, $message_content);

// ChatGPT 呼び出し
$ChatGPT_res = ConnectChatGPTAPI(
    $model_name,
    $message_content,
    $prof_chara,
    $prof_tone,
    $prof_fp,
    $recent_messages
);

$message_sender = 1; // AIが送信したメッセージなので0を指定
$ai_result = $DBmessage->InsertMessage($prof_id, $message_sender, $ChatGPT_res['response_text']);

// ChatGPT の返答をそのまま返す
echo json_encode([
    "response_text" => $ChatGPT_res['response_text'],
    "response_text_hiragana" => $ChatGPT_res['response_text_hiragana'],
    "model_voice" => $model_data['model_voice']   
], JSON_UNESCAPED_UNICODE);
exit;
