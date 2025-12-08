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
        $profile_data = $stmt->fetchAll(PDO::FETCH_ASSOC); // 連想配列で取得 

        if ($profile_data) { // modelprofileデータは必ず1件以上あるはずなので、取得できたら成功とする
            return [
                "success" => true,
                "message" => "プロファイルデータの取得に成功しました",
                "profiles" => $profile_data
            ];
        }
        else {
            return [    // 取得できなかった場合は失敗とする
                "success" => false,
                "message" => "プロファイルデータの取得に失敗しました"
            ];
        }
    }

        // prof_idから現在選択されているプロファイルのモデル設定情報を取得
        public function GetProfileConfig($prof_id)
        {
            $stmt = $this->pdo->prepare("SELECT * FROM model_profile WHERE prof_id = ?");
            $stmt->execute([$prof_id]);   // SQL実行
            $profile_config = $stmt->fetch(PDO::FETCH_ASSOC); // 連想配列で取得 
    
            if ($profile_config) { // modelprofileデータは必ず1件以上あるはずなので、取得できたら成功とする
                return [
                    "success" => true,
                    "message" => "プロファイルのモデル設定情報の取得に成功しました",
                    "profiles" => $profile_config
                ];
            }
            else {
                return [    // 取得できなかった場合は失敗とする
                    "success" => false,
                    "message" => "プロファイルのモデル設定情報の取得に失敗しました"
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
                "success" => true,
                "message" => "デフォルトプロファイルの作成に成功しました"
            ];
        } else {
            return [
                "success" => false,
                "message" => "デフォルトプロファイルの作成に失敗しました"
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

    // -------------------- update_profile_title.php --------------------

    // プロファイルタイトル変更
    public function UpdateProfileTitle($prof_id, $prof_title)
    {
        $stmt = $this->pdo->prepare("UPDATE model_profile SET prof_title = ? WHERE prof_id = ?");
        $result = $stmt->execute([$prof_title, $prof_id]); // SQL実行 成功ならtrue、失敗ならfalseを返す
        $row = $stmt->rowCount(); // データを更新した行数を取得(1なら成功、0なら失敗判定にするため)

        if ($result && $row > 0) {
            return [
                "success" => true,
                "message" => "プロファイルタイトルの更新に成功しました"
            ];
        } else {
            return [
                "success" => false,
                "message" => "プロファイルタイトルの更新に失敗しました"
            ];
        }
    }

    // -------------------- update_profile_model.php --------------------

    // プロファイルキャラクターモデル変更
    public function UpdateProfileModel($prof_id, $model_id)
    {
        $stmt = $this->pdo->prepare("UPDATE model_profile SET model_id = ? WHERE prof_id = ?");
        $result = $stmt->execute([$model_id, $prof_id]); // SQL実行 成功ならtrue、失敗ならfalseを返す
        $row = $stmt->rowCount(); // データを更新した行数を取得(1なら成功、0なら失敗判定にするため)

        if ($result && $row > 0) {
            return [
                "success" => true,
                "message" => "キャラクターモデルの変更に成功しました"
            ];
        } else {
            return [
                "success" => false,
                "message" => "キャラクターモデルの変更に失敗しました"
            ];
        }
    }

    // -------------------- update_profile_custom.php --------------------

    // プロファイルカスタム(性格、口調、一人称)変更
    public function UpdateProfileCustom($prof_id, $prof_chara, $prof_tone, $prof_fp)
    {
        $stmt = $this->pdo->prepare("UPDATE model_profile SET prof_chara = ?, prof_tone = ?, prof_fp = ? WHERE prof_id = ?");
        $result = $stmt->execute([$prof_chara, $prof_tone, $prof_fp, $prof_id]); // SQL実行 成功ならtrue、失敗ならfalseを返す
        $row = $stmt->rowCount(); // データを更新した行数を取得(1なら成功、0なら失敗判定にするため)

        if ($result && $row > 0) {
            return [
                "success" => true,
                "message" => "プロファイルカスタムの変更に成功しました"
            ];
        } else {
            return [
                "success" => false,
                "message" => "プロファイルカスタムの変更に失敗しました"
            ];
        }
    }

    // -------------------- delete_profile.php --------------------

    // プロファイル削除
    public function DeleteProfile($prof_id)
    {
        $stmt = $this->pdo->prepare("DELETE FROM model_profile WHERE prof_id = ?");
        $result = $stmt->execute([$prof_id]); // SQL実行 成功ならtrue、失敗ならfalseを返す
        $row = $stmt->rowCount(); // データを更新した行数を取得(1なら成功、0なら失敗判定にするため)

        if ($result && $row > 0) {
            return [
                "success" => true,
                "message" => "プロファイルの削除に成功しました"
            ];
        } else {
            return [
                "success" => false,
                "message" => "プロファイルの削除に失敗しました"
            ];
        }
    }

    // -------------------- register.php --------------------

    // user_idに対応したprof_idが一番小さいデータを取得する-DBprofile.php(新規登録直後の選択プロファイルを設定するため)
    public function GetDefaultSetProfile($user_id)
    {
        $stmt = $this->pdo->prepare("SELECT MIN(prof_id) AS min_prof_id FROM model_profile WHERE user_id = ?");
        $stmt->execute([$user_id]); // SQL実行
        $defaultset_prof_id = $stmt->fetchColumn(); // 最小のprof_idを取得

        // データが存在する場合、そのprof_idを返す
        if ($defaultset_prof_id) {
            return $defaultset_prof_id;
        } else {
            return [
                "success"=> false,
                "message"=> "デフォルトセットプロファイルの取得に失敗しました(DB)"
            ];
        }
    }

}
?>