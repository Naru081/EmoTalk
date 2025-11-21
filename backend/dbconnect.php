<?php
require 'vendor/autoload.php';

// .envファイルの読み込み
$dotenv = Dotenv\Dotenv::createImmutable(__DIR__);
$dotenv->load();

// 環境変数から接続情報を取得
$dbHost = $_ENV['DB_HOST'];
$dbPort = $_ENV['DB_PORT'];
$dbName = $_ENV['DB_NAME'];
$dbUsername = $_ENV['DB_USERNAME'];
$dbPassword = $_ENV['DB_PASSWORD'];

// データベース接続設定
try {
    $dsn = "mysql:host=$dbHost;port=$dbPort;dbname=$dbName;charset=utf8mb4";
    $pdo = new PDO($dsn, $dbUsername, $dbPassword);
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    // ↓成功メッセージを表示しないようにコメントアウト(接続成功確認済み)
    // echo "データベース接続に成功しました。";
} catch (PDOException $e) {
    // ↓エラーメッセージを表示しないようにコメントアウト
    // echo "データベース接続に失敗しました: " . $e->getMessage();
}

?>