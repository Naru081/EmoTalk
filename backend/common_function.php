<?php

// DB用共通関数読み込み
require_once __DIR__ . '/DBPHP/DBcommon.php';

// DB接続
require_once __DIR__ . '/dbconnect.php';

// DB関数読み込み
require_once __DIR__ . '/DBPHP/DBuser.php';
require_once __DIR__ . '/DBPHP/DBmessage.php';
require_once __DIR__ . '/DBPHP/DBprofile.php';
require_once __DIR__ . '/DBPHP/DBmodel.php';

// 各機能用関数読み込み
require_once __DIR__ . '/PHP_message/send_message.php';
require_once __DIR__ . '/PHP_api/connect_api.php';

// DBクラスオブジェクト生成
$DBmessage = new DBmessage($pdo);
$DBprofile = new DBprofile($pdo);
$DBmodel = new DBmodel($pdo);
$DBuser = new DBuser($pdo);

# 呼び出す際は、require_once __DIR__ . '/../common_function.php'; を使用してください
?>