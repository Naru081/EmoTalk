<?php
declare(strict_types=1);

ini_set('display_errors', '0');
error_reporting(0);
ob_start();

require_once __DIR__ . '/../common_function.php';
require_once __DIR__ . '/connect_api.php';

header('Content-Type: application/json; charset=utf-8');

$row = file_get_contents('php://input');
$data = json_decode($row, true);

$model_voice = $data['model_voice'] ?? ($_POST['model_voice'] ?? '');
$text = $data['responseText_hiragana'] ?? ($_POST['responseText_hiragana'] ?? '');

if ($model_voice === '' || $text === '') {
    ob_clean();
    echo json_encode([
        "success" => false,
        "message" => "model_voice または text が不足しています"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// CoeiroInk APIのウォーミングアップ
WarmUpCoeiroInkAPI($model_voice);

// 音声生成
$voice_wav = ConnectCoeiroInkAPI($model_voice, $text);

if ($voice_wav === null || $voice_wav === '') {
    $response = [
        "success" => false,
        "message" => "音声生成に失敗しました",
        "voice_wav_base64" => ""
    ];
} else {
    $response = [
        "success" => true,
        "message" => "音声生成に成功しました",
        "voice_wav_base64" => base64_encode($voice_wav)
    ];
}

ob_clean();
header('Content-Type: application/json; charset=utf-8');
echo json_encode($response, JSON_UNESCAPED_UNICODE);
exit;

?>