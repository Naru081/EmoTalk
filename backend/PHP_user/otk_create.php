<?php
// パスワード再設定認証用のワンタイムキーを発行するPHP(メールでユーザに送信しDBに登録する)
require_once __DIR__ . '/../common_function.php';

// .env 読み込み
$dotenv = Dotenv\Dotenv::createImmutable(__DIR__ . '/..');
$dotenv->load();

use PHPMailer\PHPMailer\PHPMailer;
use PHPMailer\PHPMailer\Exception;

// unityからデータを取得(JSON形式)
$row = file_get_contents('php://input');
// JSONデータを連想配列に変換
$data = json_decode($row, true);

// 受け取ったデータを変数に格納 (空の場合は空白を代入)
$email = $data['user_mail'] ?? "";

// メールアドレスの形式チェック
if ($email === "" || !filter_var($email, FILTER_VALIDATE_EMAIL)) {
    echo json_encode(["success" => false, "message" => "メールアドレスが不正です"], JSON_UNESCAPED_UNICODE);
    exit;
}

// メールアドレスが登録されているかチェック
if (!$DBuser->isEmailDuplicate($email)) {
    echo json_encode([
        "success" => false,
        "message" => "このメールアドレスは登録されていません"
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// 数字6桁ワンタイムキー生成
$otk = random_int(100000,999999);
// 生成した時間を取得
$otk_created = date('Y-m-d H:i:s');

// DBにワンタイムキーを保存-DBuser.php
$result = $DBuser->CreateOtk($otk, $otk_created, $email);

// ユーザメールアドレス宛にワンタイムキーの送信処理
try {
    $mail = new PHPMailer(true);

    // メール送信設定
    $mail->isSMTP();
    $mail->Host       = $_ENV['MAIL_HOST'];
    $mail->SMTPAuth   = true;
    $mail->Username   = $_ENV['MAIL_USERNAME'];  // Gmail のアドレス
    $mail->Password   = $_ENV['MAIL_PASSWORD'];  // Gmail アプリパスワード
    $mail->SMTPSecure = 'tls'; 
    $mail->Port       = $_ENV['MAIL_PORT'];
    $mail->setFrom($_ENV['MAIL_FROM']); // 送信者のメールアドレス
    $mail->addAddress($email);          // 送信先のメールアドレス

    // メール内容
    $mail->isHTML(true);
    $mail->Subject = '【EmoTalk】 パスワード再設定用ワンタイムキーのお知らせ';
    $mail->Body = "<p>EmoTalkをご利用いただきありがとうございます。</p></br></br>
                     <p>パスワード再設定用のワンタイムキーは以下の通りです。</p></br>
                        <h2>{$otk}</h2></br>
                        <p>ワンタイムキーの有効期限は発行から30分間です。</p></br>
                        <p>他人に知られないようご注意ください。</p></br></br>
                        <p>EmoTalk運営チーム</p>";

    // メール送信
    $mail->send();

    // 最終結果success(trueかfalse)とmessageを返す
    echo json_encode($result, JSON_UNESCAPED_UNICODE);

} catch (Exception $e) {
    echo json_encode([
        "success"=> false,
        "message"=> "メールの送信に失敗しました: {$mail->ErrorInfo}"
    ], JSON_UNESCAPED_UNICODE);
}




?>