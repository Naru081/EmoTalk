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

require_once __DIR__ . '/DBcommon.php';

class DBmessage extends DBcommon
{
    private $pdo;

    public function __construct($pdo)
    {
        parent::__construct($pdo);
        $this->pdo = $pdo;
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

        return $result;
    }

    // 最新の会話を取得（新しい順で取得 → 逆順に）
    public function GetRecentMessages($prof_id, $limit = 6)
    {
        $sql = "
        SELECT message_sender, message_content FROM (
            SELECT msg_id, 
                   CASE msg_sender WHEN '0' THEN 0 WHEN '1' THEN 1 END AS message_sender, 
                   msg_content AS message_content
            FROM message 
            WHERE prof_id = :prof_id
            ORDER BY msg_id DESC
            LIMIT :limit
        ) AS sub
        ORDER BY msg_id ASC;
        ";

        $stmt = $this->pdo->prepare($sql);
        $stmt->bindValue(':prof_id', $prof_id, PDO::PARAM_INT);
        $stmt->bindValue(':limit', $limit, PDO::PARAM_INT);
        $stmt->execute();

        return $stmt->fetchAll(PDO::FETCH_ASSOC);
    }
}


?>