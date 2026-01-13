<?php
require_once __DIR__ . '/../common_function.php';
require_once __DIR__ . '/connect_api.php';

$model_voice = $_POST['model_voice'] ?? '';
$text = $_POST['text'] ?? '';

if (empty($model_voice) || empty($text)) {
    echo json_encode([
        "success" => false,
        "message" => "model_voice または text が不足しています"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// CoeiroInk で音声生成
$voice_wav = ConnectCoeiroInkAPI($model_voice, $text);

if (empty($voice_wav)) {
    echo json_encode([
        "success" => false,
        "message" => "音声生成に失敗しました"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// 保存先ディレクトリ
$save_dir = __DIR__ . '/../tmp_voice/';
if (!file_exists($save_dir)) {
    mkdir($save_dir, 0777, true);
}

// ファイル名
$filename = "voice_" . uniqid() . ".wav";
$filepath = $save_dir . $filename;

// 保存
file_put_contents($filepath, $voice_wav);

// 公開URL（あなたのサーバーに合わせて変更）
$public_url = "https://yourserver.com/tmp_voice/" . $filename;

echo json_encode([
    "success" => true,
    "voice_url" => $public_url
], JSON_UNESCAPED_UNICODE);
exit;
