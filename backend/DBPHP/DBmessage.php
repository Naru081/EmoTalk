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

class DBmessage
{
    private $pdo;

    public function __construct($pdo)
    {
        $this->pdo = $pdo;
    }

    // -------------------- ああああ --------------------



}
?>