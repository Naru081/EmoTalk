<?php
// データベースmodelテーブルとのやり取りを行う処理をまとめたPHP

/*
==================================================
                modelテーブル構造
==================================================
model_id       INT(8)          PRIMARY KEY AUTO_INCREMENT NOT NULL
model_name     VARCHAR(50)    NOT NULL
model_info     VARCHAR(255)    NOT NULL
model_voice    VARCHAR(255)    NOT NULL
model_path     VARCHAR(255)    NOT NULL
==================================================
*/

class DBmodel
{
    private $pdo;

    public function __construct($pdo)
    {
        $this->pdo = $pdo;
    }

    // -------------------- get_models.php --------------------

    // modelテーブルから全データを取得
    public function ModelData(){
        $stmt = $this->pdo->prepare("SELECT * FROM model");
        $stmt->execute();   // SQL実行
        $model = $stmt->fetchAll(PDO::FETCH_ASSOC); // 実行した結果(モデル情報)を取得 PDO::FETCH_ASSOCで連想配列として取得(余計な情報を省く)

        return $model;
    }

    // -------------------- その他の処理 --------------------

}
?>