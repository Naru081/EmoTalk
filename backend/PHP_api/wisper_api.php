<?php
ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);

require_once __DIR__ . '/../common_function.php';
require_once __DIR__ . '/connect_api.php';

// アップロードファイルの情報をデバッグログに保存
// file_put_contents(__DIR__ . "/upload_debug.log", print_r($_FILES, true));

if (empty($_FILES['audio']['tmp_name'])) {
    echo json_encode([
        "success" => false,
        "message" => "音声ファイルがありません"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// アップロードファイルの情報をデバッグログに保存
// file_put_contents(__DIR__ . "/upload_debug.log", print_r($_FILES, true));

$tmpFile = $_FILES['audio']['tmp_name'];

// debug保存
// $debugPath = __DIR__ . '/debug.wav';
// copy($tmpFile, $debugPath);

// ffmpegで形式を強制変換
$convertedPath = __DIR__ . '/converted.wav';

// FFmpegのパス
$ffmpeg = "C:\\ffmpeg\\bin\\ffmpeg.exe";

$cmd = "\"$ffmpeg\" -y -i " . escapeshellarg($tmpFile) .
       " -ar 16000 -ac 1 -acodec pcm_s16le " .
       escapeshellarg($convertedPath) . " 2>&1";exec($cmd, $output, $ret);

// デバッグログ保存
// file_put_contents(__DIR__ . "/ffmpeg_ret.log", $ret);
// file_put_contents(__DIR__ . "/ffmpeg_out.log", implode("\n", $output));

if ($ret !== 0) {
    echo json_encode([
        "success" => false,
        "message" => "ffmpeg変換に失敗",
        "log" => implode("\n", $output)
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// Whisper API 呼び出し
$wisper_res = ConnectWisperAPI($convertedPath);

// デバッグログ保存
// file_put_contents(__DIR__ . "/wisper_result.log", print_r($wisper_res, true));
// file_put_contents(__DIR__ . "/php_debug.log", "スクリプト終了直前\n", FILE_APPEND);

// Whisper APIの結果をそのまま返す
echo json_encode($wisper_res, JSON_UNESCAPED_UNICODE);
exit;
?>