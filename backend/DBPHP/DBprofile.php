<?php
// データベースmodelprofileテーブルとのやり取りを行う処理をまとめたPHP

/*
==================================================
                modelprofileテーブル構造
==================================================
prof_id        INT(8)          PRIMARY KEY AUTO_INCREMENT NOT NULL
user_id        INT(8)          FOREIGN KEY NOT NULL
model_id       INT(8)          FOREIGN KEY NOT NULL
prof_title     VARCHAR(10)     NOT NULL
prof_chara     VARCHAR(20)     NOT NULL
prof_tone      VARCHAR(20)     NOT NULL
prof_fps       INT(20)         NOT NULL
==================================================
*/

class DBprofile
{
    private $pdo;

    public function __construct($pdo)
    {
        $this->pdo = $pdo;
    }

    // -------------------- 共通処理 --------------------

    // ユーザIDから全プロファイル情報を取得
    public function GetProfileData($user_id)
    {
        $stmt = $this->pdo->prepare("SELECT * FROM model_profile WHERE user_id = ?");
        $stmt->execute([$user_id]);   // SQL実行
        $profiles = $stmt->fetchAll(PDO::FETCH_ASSOC); // 連想配列で取得 

        if ($profiles) { // modelprofileデータは必ず1件以上あるはずなので、取得できたら成功とする
            return [
                "success" => true,
                "message" => "プロファイルデータの取得に成功しました",
                "profiles" => $profiles
            ];
        }
        else {
            return [    // 取得できなかった場合は失敗とする
                "success" => false,
                "message" => "プロファイルデータの取得に失敗しました"
            ];
        }
    }

    // -------------------- register.php --------------------

    // 各ユーザ登録時に、デフォルトプロファイルを3つ作成
    public function CreateDefaultProfile($user_id)
    {
        // 仮データです。 モデルを決めた際に変更します。
        $stmt = $this->pdo->prepare("INSERT INTO model_profile (user_id, model_id, prof_title, prof_chara, prof_tone, prof_fp) 
        VALUES
        (?, 1, 'デフォルト1', '明るい', 'カジュアル', '私'),
        (?, 2, 'デフォルト2', '真面目', '丁寧', '僕'),
        (?, 3, 'デフォルト3', 'クール', '無口', '俺')");
        $result = $stmt->execute([$user_id, $user_id, $user_id]);   // SQL実行
        $row = $stmt->rowCount();   // データを更新した行数を取得(3なら成功、0なら失敗判定にするため)

        if ($result && $row === 3) {
            return [
                "success"=> true,
                "message"=> "デフォルトプロファイルの作成に成功しました"
            ];
        } else {
            return [
                "success"=> false,
                "message"=> "デフォルトプロファイルの作成に失敗しました"
            ];
        }
    }

    // -------------------- create_profile.php --------------------

    // 新規プロファイル作成
    public function CreateProfile($user_id, $model_id, $prof_title, $prof_chara, $prof_tone, $prof_fp)
    {
        // DBにProfile情報を保存
        $stmt = $this->pdo->prepare("INSERT INTO model_profile (user_id, model_id, prof_title, prof_chara, prof_tone, prof_fp) VALUES (?, ?, ?, ?, ?, ?)");
        $result = $stmt->execute([$user_id, $model_id, $prof_title, $prof_chara, $prof_tone, $prof_fp]); // SQL実行
        $row = $stmt->rowCount();   // データを更新した行数を取得(1なら成功、0なら失敗判定にするため)

        if ($result && $row > 0) {
            return [
                "success" => true
            ];
        } else {
            return [
                "success" => false,
                "message" => "プロファイル作成に失敗しました"
            ];
        }
    }

}
?>