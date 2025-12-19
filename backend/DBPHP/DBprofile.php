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
prof_fp        VARCHAR(20)     NOT NULL
==================================================
*/

require_once __DIR__ . '/DBcommon.php';

class DBprofile extends DBcommon
{
    private $pdo;

    public function __construct($pdo)
    {
        parent::__construct($pdo);
        $this->pdo = $pdo;
    }

    // -------------------- 共通処理 --------------------

    // ユーザIDから全プロファイル情報を取得
    public function GetProfileData($user_id)
    {
        $profile_data = $this->ExecuteSelect("SELECT * FROM model_profile WHERE user_id = ?", [$user_id]);

        if (!empty($profile_data)) {
            return [
                "success" => true,
                "message" => "プロファイルデータの取得に成功しました",
                "profile_data" => $profile_data
            ];
        } else {
            return [
                "success" => false,
                "message" => "プロファイルデータの取得に失敗しました",
                "profile_data" => []
            ];
        }
    }

    // prof_idから現在選択されているプロファイルのモデル設定情報を取得
    public function GetProfileConfig($prof_id)
    {
        $profile_config = $this->ExecuteSelect("SELECT * FROM model_profile WHERE prof_id = ?", [$prof_id]);

        if (!empty($profile_config)) {
            $profile_config = $profile_config[0];   // 1行のみ取り出す

            return [
                "success" => true,
                "message" => "プロファイルのモデル設定情報の取得に成功しました",
                "model_id"   => $profile_config["model_id"],
                "prof_chara" => $profile_config["prof_chara"],
                "prof_tone"  => $profile_config["prof_tone"],
                "prof_fp"    => $profile_config["prof_fp"]
            ];
        } else {
            return [
                "success" => false,
                "message" => "プロファイルのモデル設定情報の取得に失敗しました",
                "profile_config" => []
            ];
        }
    }

    // -------------------- register.php --------------------

    // 各ユーザ登録時に、デフォルトプロファイルを3つ作成
    public function CreateDefaultProfile($user_id)
    {
        $result = $this->ExecuteUpdate("INSERT INTO model_profile (user_id, model_id, prof_title, prof_chara, prof_tone, prof_fp) 
        VALUES
        (?, 1, 'デフォルト1', '明るい', 'カジュアル', '私'),
        (?, 2, 'デフォルト2', '真面目', '丁寧', '僕'),
        (?, 3, 'デフォルト3', 'クール', '無口', '俺')", [$user_id,$user_id,$user_id]);

        if ($result["success"]) {
            return [
                "success" => true,
                "message" => "デフォルトプロファイルの作成に成功しました",
            ];
        } else {
            return [
                "success" => false,
                "message" => "デフォルトプロファイルの作成に失敗しました",
            ];
        }
    }

    // -------------------- create_profile.php --------------------

    // 新規プロファイル作成
    public function CreateProfile($user_id, $model_id, $prof_title, $prof_chara, $prof_tone, $prof_fp)
    {
        $result = $this->ExecuteUpdate(
            "INSERT INTO model_profile (user_id, model_id, prof_title, prof_chara, prof_tone, prof_fp) VALUES (?, ?, ?, ?, ?, ?)",
            [$user_id, $model_id, $prof_title, $prof_chara, $prof_tone, $prof_fp]
        );

        if ($result["success"]) {
            return [
                "success" => true,
                "message" => "新規プロファイルの作成に成功しました",
            ];
        } else {
            return [
                "success" => false,
                "message" => "新規プロファイルの作成に失敗しました"
            ];
        }
    }

    // -------------------- update_profile_title.php --------------------

    // プロファイルタイトル変更
    public function UpdateProfileTitle($prof_id, $prof_title)
    {
        $result = $this->ExecuteUpdate("UPDATE model_profile SET prof_title = ? WHERE prof_id = ?", [$prof_title, $prof_id]);
        if ($result["success"]) {
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
        $result = $this->ExecuteUpdate("UPDATE model_profile SET model_id = ? WHERE prof_id = ?", [$model_id, $prof_id]);
        if ($result["success"]) {
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
        $result = $this->ExecuteUpdate("UPDATE model_profile SET prof_chara = ?, prof_tone = ?, prof_fp = ? WHERE prof_id = ?", 
        [$prof_chara, $prof_tone, $prof_fp, $prof_id]);
        if ($result["success"]) {
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
        $result = $this->ExecuteUpdate("DELETE FROM model_profile WHERE prof_id = ?", [$prof_id]);
        if ($result["success"]) {
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

    // user_idに対応し、その中で一番小さいprofile_idを取得する-DBprofile.php(新規登録直後の選択プロファイルを設定するため)
    public function GetDefaultSetProfile($user_id)
    {
        $result = $this->ExecuteSelect("SELECT MIN(prof_id) AS min_prof_id FROM model_profile WHERE user_id = ?", [$user_id]);
        
        if (!empty($result)) {
            $min_prof_id = $result[0]['min_prof_id'];   // 配列から値のみを取り出す
            return [
                "success" => true,
                "message" => "user_idに対応した一番小さい値のprofile_idの取得に成功しました",
                "min_prof_id" => $min_prof_id
            ];
        } else {
            return [
                "success" => false,
                "message" => "user_idに対応した一番小さい値のprofile_idの取得に失敗しました"
            ];
        }
    }

}
?>