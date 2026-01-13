<?PHP
// メッセージ送信(ユーザとAI)を行うPHP(関数)
// message_senderが 0 の場合はユーザ、1 の場合はAIとする


function insert_message($pdo, $prof_id, $message_sender, $message_content)
{
    require_once __DIR__ . '/../common_function.php';

    // $message_senderが0か1以外の場合はエラー
    if (!in_array($message_sender,[0, 1])) {
        return [
            "success" => false,
            "message" => "DBへのメッセージ保存に失敗しました"
        ];
    }

    // メッセージをDBに保存-DBmessage.php
    $result = $DBmessage->InsertMessage($prof_id, $message_sender, $message_content);
    $result['message'] = "DBへのメッセージ保存に成功しました";

    // successとmessageを格納した$resultを返す
    return $result;
}

?>