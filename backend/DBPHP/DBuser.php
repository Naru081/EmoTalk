<?PHP
// データベースuserテーブルとのやり取りを行う処理をまとめたPHP

class DBuser
{
    private $pdo;

    public function __construct($pdo)
    {
        $this->pdo = $pdo;
    }

    // -------------------- register.php --------------------

    // メールアドレスのDB重複チェック
    public function isEmailDuplicate($email)
    {
        // メールアドレスのDB重複チェック
        $stmt = $this->pdo->prepare("SELECT user_id FROM user WHERE user_mail = ?");
        $stmt->execute([$email]);   // SQL実行

        return $stmt->fetch() !== false;    // 重複している場合はtrueを返す
    }

    // ユーザ情報(メールアドレスとパスワード(ハッシュ化)をDBに保存
    public function registerUser($email, $password)
    {
        $hash = password_hash($password, PASSWORD_DEFAULT); // パスワードをハッシュ化
        $stmt = $this->pdo->prepare("INSERT INTO user (user_mail, user_pass) VALUES (?, ?)");
        $result = $stmt->execute([$email, $hash]);  // SQL実行 成功ならtrue、失敗ならfalseを返す

        if ($result == true) {
            return [
                "success" => true,
                "message" => "ユーザ登録が完了しました"
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
        // メールアドレスからユーザ情報を取得
        $stmt = $this->pdo->prepare("SELECT user_id, user_pass FROM user WHERE user_mail = ?");
        $stmt->execute([$email]);   // SQL実行
        $user = $stmt->fetch(); // 実行した結果(ユーザ情報)を取得

        // ユーザ情報が存在しない場合の処理
        if (!$user) {
            return [
                "success"=> false,
                "message"=> "メールアドレスまたはパスワードが不正です"
            ];
        }

        // パスワードの照合
        if (!password_verify($password, $user["user_pass"])) {      
            return [
                "success"=> true,
                "message"=> "メールアドレスまたはパスワードが不正です"
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
            ];
    }

    // -------------------- logout.php --------------------

    // ログアウト処理（トークンを削除)
    public function LogoutUser($email)
    {
        $stmt = $this->pdo->prepare("UPDATE user SET user_token = NULL WHERE user_mail = ?");
        $result = $stmt->execute([$email]); // SQL実行 成功ならtrue、失敗ならfalseを返す
        if ($result == true) {
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

    // -------------------- autologin.php --------------------

    // 自動ログイン認証
    public function AutoLoginUser($localtoken)
    {
        $stmt = $this->pdo->prepare("SELECT user_id, user_mail, user_token FROM user WHERE user_token = ?");
        $result = $stmt->execute([$localtoken]); // SQL実行 成功ならtrue、失敗ならfalseを返す
        $user = $stmt->fetch();

        if ($user) {
            return [
                "success" => true,
                "message" => "自動ログインに成功しました",
                "email" => $user["user_mail"]
            ];
        } else {
            return [
                "success" => false,
                "message" => "自動ログインに失敗しました"
            ];
        }
    }

    // -------------------- otkcreate.php --------------------

    // ワンタイムキーをDBに保存
    public function createOtk($otk, $otk_created, $email)
    {
        $stmt = $this->pdo->prepare("UPDATE user SET user_otk = ?, user_otk_created = ? WHERE user_mail = ?");
        $result = $stmt->execute([$otk, $otk_created, $email]);  // SQL実行 成功ならtrue、失敗ならfalseを返す

        if ($result == true) {
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

}
?>