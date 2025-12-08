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

class DBuser
{
    private $pdo;

    public function __construct($pdo)
    {
        $this->pdo = $pdo;
    }

    // ==================== 共通SQL処理関数(SELECT) ==================== 
    public function ExecuteSelect($sql, $param)
    {
        try {
            $stmt = $this->pdo->prepare($sql);
            $stmt->execute($param);   // SQL実行 
            $alldata = $stmt->fetchAll(PDO::FETCH_ASSOC); // 実行した結果(メッセージ情報)を取得 PDO::FETCH_ASSOCで連想配列として取得(余計な情報を省く)

            return $alldata;    // 取得した全データを返す
        } catch (PDOException $e) {
            // 例外エラーが発生した場合はfalseを返す
            return false;
        }
    }

    // ==================== 共通SQL処理関数(INSERT・UPDATE・DELETE) ====================
    public function ExecuteUpdate($sql, $param)
    {
        try {
            $stmt = $this->pdo->prepare($sql);
            $result = $stmt->execute($param);   // SQL実行 
            $row = $stmt->rowCount();

            if ($result && $row > 0) {
                return [
                    "success" => true,
                    "message" => "成功しました"
                ];
            } else {
                return [
                    "success" => false,
                    "message" => "失敗しました"
                ];
            }
        } catch (PDOException $e) {
            // 例外エラーが発生した場合はfalseを返す
            return [
                "success" => false,
                "message" => $e->getMessage()
            ];
        }
    }
    // ==================================================================================

    // -------------------- register.php --------------------

    // メールアドレスのDB重複チェック
    public function isEmailDuplicate($email)
    {
        $emaildup = $this->ExecuteSelect("SELECT user_id FROM user WHERE user_mail = ?",[$email]); 
        if (!empty($emaildup)) {
            return [
                "success" => false,
                "message"=> "メールアドレスは既に登録されています"
            ];
        } 
    }

    // ユーザ情報(メールアドレスとパスワード(ハッシュ化)をDBに保存
    public function registerUser($email, $password)
    {
        // パスワードをハッシュ化
        $hash = password_hash($password, PASSWORD_DEFAULT);

        // DBに保存
        $stmt = $this->pdo->prepare("INSERT INTO user (user_mail, user_pass) VALUES (?, ?)");
        $result = $stmt->execute([$email, $hash]);  // SQL実行 成功ならtrue、失敗ならfalseを返す
        $row = $stmt->rowCount(); // データを更新した行数を取得(1なら成功、0なら失敗判定にするため)

        if ($result && $row > 0) {
            // 成功した場合、登録したユーザIDも取得して返す
            $stmt = $this->pdo->prepare("SELECT user_id FROM user WHERE user_mail = ?");
            $stmt->execute([$email]);  // SQL実行
            $user = $stmt->fetch(); // 実行した結果(ユーザ情報)を取得

            return [
                "success" => true,
                "message" => "ユーザ登録が完了しました",
                "user_id" => $user["user_id"]
            ];
        } else {
            return [
                "success" => false,
                "message" => "ユーザ登録に失敗しました"
            ];
        }
    }

        // -------------------- login.php --------------------

    // ログインの認証処理とトークン発行、DBに保存
    public function LoginUser($email, $password)
    {
        try {
            // メールアドレスからユーザ情報を取得
            $stmt = $this->pdo->prepare("SELECT user_id, user_pass FROM user WHERE user_mail = ?");
            $stmt->execute([$email]);   // SQL実行
            $user = $stmt->fetch(); // 実行した結果(ユーザ情報)を取得

            // ユーザ情報が存在しない場合の処理
            if (!$user) {
                return [
                    "success" => false,
                    "message" => "メールアドレスまたはパスワードが不正です"
                ];
            }

            // パスワードの照合
            if (!password_verify($password, $user["user_pass"])) {
                return [
                    "success" => false,
                    "message" => "メールアドレスまたはパスワードが不正です"
                ];
            }

            // 認証成功時、トークンを発行してDBに保存(Unityにも返す)
            while (true) {  // 重複防止チェックループ 
                $token = bin2hex(random_bytes(32)); // 64文字のランダムなトークンを生成

                // トークンのDB重複チェック
                $stmt = $this->pdo->prepare("SELECT user_id FROM user WHERE user_token = ?");
                $stmt->execute([$token]);  // SQL実行

                // 重複していない場合はループを抜ける
                if ($stmt->fetch() === false) {
                    break;
                }
            }

            // トークンをDBに保存
            $stmt = $this->pdo->prepare("UPDATE user SET user_token = ? WHERE user_id = ?");
            $stmt->execute([$token, $user["user_id"]]);  // SQL実行

            return [
                "success" => true,
                "message" => "ログインに成功しました",
                "token" => $token,
                "user_id" => $user["user_id"],
                "user_mail" => $email
            ];

        } catch (PDOException $e) {
            return [
                "success" => false,
                "message" => "サーバーエラーが発生しました"
            ];
        }
    }

    // -------------------- logout.php --------------------

    // ログアウト処理（トークンを削除)
    public function LogoutUser($email)
    {
        $stmt = $this->pdo->prepare("UPDATE user SET user_token = NULL WHERE user_mail = ?");
        $result = $stmt->execute([$email]); // SQL実行 成功ならtrue、失敗ならfalseを返す
        $row = $stmt->rowCount(); // データを更新した行数を取得(1なら成功、0なら失敗判定にするため)
        if ($result && $row > 0) {
            return [
                "success" => true,
                "message" => "ログアウトしました"
            ];
        } else {
            return [
                "success" => false,
                "message" => "ログアウトに失敗しました"
            ];
        }
    }

    // -------------------- auto_login.php --------------------

    // 自動ログイン認証
    public function AutoLoginUser($localtoken)
    {
        $stmt = $this->pdo->prepare("SELECT user_id, user_mail, user_currentprof FROM user WHERE user_token = ?");    // ユーザデバイスのトークンとDBのトークンを照合するSQL
        $stmt->execute([$localtoken]); // SQL実行
        $user = $stmt->fetch(); // 実行した結果(ユーザ情報)を取得

        if ($user) {    // SQL実行した結果ユーザ情報が存在する場合
            return [
                "success" => true,
                "message" => "自動ログインに成功しました",
                "email" => $user["user_mail"],
                "user_id" => $user["user_id"]
            ];
        } else {
            return [
                "success" => false,
                "message" => "自動ログインに失敗しました"
            ];
        }
    }

    // -------------------- otk_create.php --------------------

    // ワンタイムキーをDBに保存
    public function CreateOtk($otk, $otk_created, $email)
    {
        $stmt = $this->pdo->prepare("UPDATE user SET user_otk = ?, user_otk_created = ? WHERE user_mail = ?");
        $result = $stmt->execute([$otk, $otk_created, $email]);  // SQL実行 成功ならtrue、失敗ならfalseを返す
        $row = $stmt->rowCount(); // データを更新した行数を取得(1なら成功、0なら失敗判定にするため)

        if ($result && $row > 0) {
            return [
                "success" => true,
                "message" => "メールアドレスにワンタイムキーを送信しました"
            ];
        } else {
            return [
                "success" => false,
                "message" => "ワンタイムキーの送信に失敗しました"
            ];
        }
    }

    // -------------------- otk_auth.php --------------------

    // ユーザから送られたワンタイムキーとDBのワンタイムキーを照合し認証
    public function AuthOtk($otk, $email)
    {
        $stmt = $this->pdo->prepare("SELECT user_id, user_otk, user_otk_created FROM user WHERE user_otk = ? AND user_mail = ?"); // ワンタイムキーを照合するSQL
        $stmt->execute([$otk, $email]); // SQL実行
        $user = $stmt->fetch(); // 実行した結果(ユーザ情報)を取得

        if ($user) {
            // --- ワンタイムキーの照合が成功したとき(ユーザ情報が存在する場合) ---

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
            $stmt = $this->pdo->prepare("UPDATE user SET user_otk = NULL, user_otk_created = NULL WHERE user_id = ?");
            $stmt->execute([$user["user_id"]]); // SQL実行

            return [
                "success" => true,
                "message" => "ワンタイムキーの認証に成功しました"
            ];

        } else {
            // --- ワンタイムキーの照合が失敗したとき(ユーザ情報が存在しない場合) ---

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

        // DBに保存
        $stmt = $this->pdo->prepare("UPDATE user SET user_pass = ? WHERE user_mail = ?");
        $result = $stmt->execute([$hash, $email]); // SQL実行 成功ならtrue、失敗ならfalseを返す
        $row = $stmt->rowCount(); // データを更新した行数を取得(1なら成功、0なら失敗判定にするため)

        if ($result && $row > 0) {
            // パスワード再設定成功時
            return [
                "success" => true,
                "message" => "パスワードの再設定が完了しました"
            ];
        } else {
            return [
                "success" => false,
                "message" => "パスワードの再設定に失敗しました"
            ];
        }
    }

    // -------------------- change_profile.php --------------------

    // ユーザの現在のプロファイルを変更
    public function ChangeProfile($user_id, $prof_id)
    {
        $stmt = $this->pdo->prepare("UPDATE user SET user_currentprof = ? WHERE user_id = ?");
        $result = $stmt->execute([$prof_id, $user_id]); // SQL実行 成功ならtrue、失敗ならfalseを返す
        $row = $stmt->rowCount(); // データを更新した行数を取得(1なら成功、0なら失敗判定にするため)

        if ($result && $row > 0) {
            return [
                "success" => true,
                "message" => "プロファイルの切り替えに成功しました"
            ];
        } else {
            return [
                "success" => false,
                "message" => "プロファイルの切り替えに失敗しました"
            ];
        }
    }


}
?>