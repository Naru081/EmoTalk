<?php
// データベースmessageテーブルとのやり取りを行う処理をまとめたPHP

/*
==================================================
                messageテーブル構造
==================================================
msg_id          INT(8)          PRIMARY KEY AUTO_INCREMENT NOT NULL
prof_id         INT(8)          FOREIGN KEY NOT NULL
msg_sender      VARCHAR(1)      NOT NULL
msg_content     TEXT            NOT NULL
==================================================
*/

class DBmessage extends DBcommon
{
    private $pdo;

    public function __construct($pdo)
    {
        parent::__construct($pdo);
    }

    // -------------------- get_messages.php --------------------
    public function GetMessageAll($prof_id)
    {
        // prof_idから全メッセージデータを取得
        $message_alldata = $this->ExecuteSelect("SELECT * FROM message WHERE prof_id = ?", [$prof_id]);

        // 全メッセージデータの取得に成功した場合(返り値がfalseでない場合)
        if ($message_alldata !== false) {
            return [
                "success" => true,
                "message" => "メッセージデータの取得に成功しました",
                "message_alldata" => $message_alldata
            ];
        }
    }

    // -------------------- send_message.php --------------------

    // ユーザのメッセージをDBに保存
    public function InsertMessage($prof_id, $message_sender, $message_content)
    {
        $result = $this->ExecuteUpdate("INSERT INTO message (prof_id, msg_sender, msg_content) VALUES (?, ?, ?)", [$prof_id, $message_sender, $message_content]);

        if ($result['success']) {
            return [
                "success" => true,
                "message" => "DBへのメッセージ保存に成功しました",
            ];
        }
    }
}
?>