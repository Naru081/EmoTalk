<?PHP
// データベースuserテーブルとのやり取りを行う処理をまとめたPHP

/*
==================================================
                userテーブル構造
==================================================
user_id             INT(8)          PRIMARY KEY AUTO_INCREMENT NOT NULL
user_mail           VARCHAR(255)    UNIQUE KEY NOT NULL
user_pass           VARCHAR(255)    NOT NULL
user_otk            INT(6)     
user_otk_created    DATETIME        
user_token          VARCHAR(64)     
user_currentprof    INT(8)
==================================================
*/

require_once __DIR__ . '/DBcommon.php';

class DBuser extends DBcommon
{
    private $pdo;

    public function __construct($pdo)
    {
        parent::__construct($pdo);
        $this->pdo = $pdo;
    }
    // -------------------- register.php --------------------

    // メールアドレスのDB重複チェック
    public function isEmailDuplicate($email)
    {
        $emaildup = $this->ExecuteSelect("SELECT user_id FROM user WHERE user_mail = ?", [$email]);
        return !empty($emaildup); // 重複している場合はtrueを返す
    }

    // ユーザ情報(メールアドレスとパスワード(ハッシュ化)をDBに保存
    public function registerUser($email, $password)
    {
        // パスワードをハッシュ化
        $hash = password_hash($password, PASSWORD_DEFAULT);

        // ユーザ情報をDBに保存
        $result = $this->ExecuteUpdate("INSERT INTO user (user_mail, user_pass) VALUES (?, ?)", [$email, $hash]);
  
        if (!$result || !$result['success']) { // ユーザ情報がDBに保存失敗した場合
            return [ 
                "success" => false,
                "message" => "ユーザ登録に失敗しました",
            ];
        }
  
        // ユーザデータ(user_id)の取得
        $user = $this->ExecuteSelect("SELECT user_id FROM user WHERE user_mail = ?", [$email]);

        if (!empty($user)) {    // ユーザデータの取得に成功した場合
            $user = $user[0];   // 1行のみ取り出す

            return [
                "success" => true,
                "message" => "ユーザデータの取得に成功しました",
                "user_id" => $user["user_id"]
            ];
        } else {
            return [
                "success" => false,
                "message" => "ユーザデータ登録に失敗しました"
            ];
        }
    }

    // -------------------- login.php --------------------

    // ログインの認証処理とトークン発行、DBに保存
    public function LoginUser($email, $password)
    {
        // メールアドレスからユーザ情報を取得
        $row = $this->ExecuteSelect("SELECT user_id, user_pass, user_mail, user_currentprof FROM user WHERE user_mail = ?", [$email]);

        // ユーザ情報が存在しない場合の処理
        if (empty($row)) {
            return [
                "success" => false,
                "message" => "メールアドレスまたはパスワードが不正です"
            ];
        }

        $user = $row[0];   // 1行のみ取り出す

        // パスワードの照合
        if (!password_verify($password, $user["user_pass"])) {
            return [
                "success" => false,
                "message" => "メールアドレスまたはパスワードが不正です"
            ];
        }

        // 認証成功時、トークンを発行してDBに保存(Unityにも返す)
        $token = bin2hex(random_bytes(32)); // 64文字のランダムなトークンを生成

        // トークンをDBに保存
        $update = $this->ExecuteUpdate("UPDATE user SET user_token = ? WHERE user_id = ?", [$token, $user["user_id"]]);

        if ($update["success"]) {
            return [
                "success" => true,
                "message" => "ログインに成功しました",
                "user_id" => $user["user_id"],
                "user_mail" => $user["user_mail"],
                "user_currentprof" => $user["user_currentprof"],
                "token" => $token
            ];
        } else {
            return [
                "success" => false,
                "message" => "DBにトークン保存失敗しました",
            ];
        }
    }

    // -------------------- logout.php --------------------

    // ログアウト処理（トークンを削除)
    public function LogoutUser($user_id)
    {
        // トークンを削除(NULLに)する
        $result = $this->ExecuteUpdate("UPDATE user SET user_token = NULL WHERE user_id = ?", [$user_id]);
        if ($result["success"]) {
            return [
                "success" => true,
                "message" => "ログアウトに成功しました",
            ];
        } else {
            return [
                "success" => false,
                "message" => "ログアウトに失敗しました",
            ];
        }
    }

    // -------------------- auto_login.php --------------------

    // 自動ログイン認証
    public function AutoLoginUser($localtoken)
    {
        // トークンからユーザ情報を取得
        $user = $this->ExecuteSelect("SELECT user_id, user_mail, user_currentprof FROM user WHERE user_token = ?", [$localtoken]);

        if (!empty($user)) {    // SQL実行した結果ユーザ情報が存在する場合
            $user = $user[0];   // 1行のみ取り出す
            return [
                "success" => true,
                "message" => "自動ログインに成功しました",
                "user_id" => $user["user_id"],
                "user_mail" => $user["user_mail"],
                "user_currentprof" => $user["user_currentprof"]
            ];
        } else {
            return [
                "success" => false,
                "message" => "自動ログインに失敗しました",
            ];
        }
    }

    // -------------------- otk_create.php --------------------

    // ワンタイムキーをDBに保存
    public function CreateOtk($otk, $otk_created, $email)
    {
        $result = $this->ExecuteUpdate("UPDATE user SET user_otk = ?, user_otk_created = ? WHERE user_mail = ?", [$otk, $otk_created, $email]);
        if ($result["success"]) {
            return [
                "success" => true,
                "message" => "ワンタイムキーの送信に成功しました",
            ];
        } else {
            return [
                "success" => false,
                "message" => "ワンタイムキーの送信に失敗しました",
            ];
        }
    }

    // -------------------- otk_auth.php --------------------

    // ユーザから送られたワンタイムキーとDBのワンタイムキーを照合し認証
    public function AuthOtk($otk, $email)
    {
        $user = $this->ExecuteSelect("SELECT user_id, user_otk, user_otk_created FROM user WHERE user_otk = ? AND user_mail = ?", [$otk, $email]);

        if (!empty($user)) {    // ワンタイムキーの照合が成功したとき(ユーザ情報が存在する場合)
            $user = $user[0];   // 1行のみ取り出す

            // ワンタイムキーの有効期限チェック (発行から30分以内)
            $now = new DateTime();  // 現在の日時を取得
            $otk_created = new DateTime($user["user_otk_created"]); // ワンタイムキー発行日時を取得
            $otk_created->modify("+30 minutes"); // ワンタイムキー発行日時に30分(ワンタイムキーの有効期限)を加算

            if ($now > $otk_created) {  // 現時刻がワンタイムキー発行日時+30分を超えている場合、有効期限切れ
                // ワンタイムキー認証失敗時(有効期限切れ)
                return [
                    "success" => false,
                    "message" => "ワンタイムキーの認証に失敗しました"
                ];
            }

            // ワンタイムキー認証成功時、DBのワンタイムキー情報をクリア
            $result = $this->ExecuteUpdate("UPDATE user SET user_otk = NULL, user_otk_created = NULL WHERE user_id = ?", [$user["user_id"]]);

            if ($result["success"]) {
                return [
                    "success" => true,
                    "message" => "ワンタイムキーの認証に成功しました",
                ];
            } else {
                return [
                    "success" => false,
                    "message" => "ワンタイムキーの認証に失敗しました",
                ];
            }    
        } else {  // ワンタイムキーの照合が失敗したとき(ユーザ情報が存在しない場合)
            return [
                "success" => false,
                "message" => "ワンタイムキーの認証に失敗しました"
            ];
        }
    }

    // -------------------- reset_pass.php --------------------

    // 再設定後パスワードをハッシュ化してDBに保存
    public function ResetPassword($email, $newpassword)
    {
        // パスワードをハッシュ化
        $hash = password_hash($newpassword, PASSWORD_DEFAULT);

        $result = $this->ExecuteUpdate("UPDATE user SET user_pass = ? WHERE user_mail = ?", [$hash, $email]);
        if ($result["success"]) {
            return [
                "success" => true,
                "message" => "パスワードの再設定に成功しました"
            ];
        } else {
            return [
                "success" => false,
                "message" => "パスワードの再設定に失敗しました",
            ];
        }
    }

    // -------------------- change_profile.php --------------------

    // ユーザの現在のプロファイルを変更
    public function ChangeProfile($user_id, $prof_id)
    {
        $result = $this->ExecuteUpdate("UPDATE user SET user_currentprof = ? WHERE user_id = ?", [$prof_id, $user_id,]);
        if ($result["success"]) {
            return [
                "success" => true,
                "message" => "プロファイルの切り替えに成功しました"
            ];
        } else {
            return [
                "success"=> false,
                "message"=> "プロファイルの切り替えに失敗しました"
            ];
        }
    }
}
?>