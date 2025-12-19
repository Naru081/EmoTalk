<?php
// パスワード再設定認証用のワンタイムキーを認証するPHP

require_once __DIR__ . '/../common_function.php';

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$otk = $data['otk'] ?? "";
$email = $data['user_mail'] ?? ""; 

// ワンタイムキーの形式チェック (数字6桁)
$otklength = strlen((string)$otk);
if ($otk == "" || $otklength != 6 || !is_numeric($otk)) 
{
    // ワンタイムキーが不正の場合
    echo json_encode(["success" => false, "message" => "ワンタイムキーの認証に失敗しました"],JSON_UNESCAPED_UNICODE);
    exit;
} 

// ワンタイムキー認証処理-DBuser.php
$result = $DBuser->AuthOtk($otk, $email);

// successとmessageを返す
echo json_encode($result, JSON_UNESCAPED_UNICODE);
?>