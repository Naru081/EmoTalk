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

require_once __DIR__ . '/DBcommon.php';

class DBmodel extends DBcommon
{
    private $pdo;

    public function __construct($pdo)
    {
        parent::__construct($pdo);
        $this->pdo = $pdo;
    }

    // -------------------- get_models.php --------------------

    // modelテーブルから全データを取得
    public function ModelData(){
        $model_datalist = $this->ExecuteSelectNoParam("SELECT * FROM model");

        if ($model_datalist !== false) {    // 空配列でも成功とみなす
            return [
                "success" => true,
                "message" => "モデルデータの取得に成功しました",
                "model_datalist" => $model_datalist
            ];
        } else {
            return [
                "success" => false,
                "message" => "モデルデータの取得に失敗しました",
            ];
        }
    }

    // -------------------- chatgpt_api.php --------------------
    
    // model_idからmodel_nameを取得
    public function GetModelNameVoice($model_id){
        $result = $this->ExecuteSelect("SELECT model_name, model_voice FROM model WHERE model_id = ?", [$model_id]);
        
        if (!empty($result)) {
            $model_name = $result[0];   // 1行のみ取り出す

            return [
                "success" => true,
                "message" => "model_nameとmodel_voiceの取得に成功しました",
                "model_name"    => $model_name['model_name'],
                "model_voice"   => $model_name['model_voice']
            ];
        } else {
            return [
                "success" => false,
                "message" => "model_nameとmodel_voiceの取得に失敗しました",
            ];
        }
    }
}
?>