<?php
require_once __DIR__ . '/../common_function.php';
require_once __DIR__ . '/connect_api.php';

if (empty($_FILES['audio']['tmp_name'])) {
    echo json_encode([
        "success" => false,
        "message" => "音声ファイルがありません"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$tmpFile = $_FILES['audio']['tmp_name'];

// Whisper API 呼び出し
$wisper_res = ConnectWisperAPI($tmpFile);

// Whisper の結果をそのまま返す
echo json_encode($wisper_res, JSON_UNESCAPED_UNICODE);
exit;
