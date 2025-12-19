using System;

// ===== PHP 通信用リクエストデータクラス群 =====

[System.Serializable]
public class UserRequest        // 新規登録・ログイン共通リクエスト
{
    public string user_mail;
    public string user_pass;
}

[System.Serializable]
public class RegisterResponse   // 新規登録リクエスト
{
    public bool success;
    public string message;
    public int user_id;
}


[System.Serializable]           // メールアドレスのみリクエスト
public class User_mailRequest
{
    public string user_mail;
}


[System.Serializable]           // ワンタイムキー認証リクエスト
public class OtkAuthRequest
{
    public string user_mail;
    public string otk;
}

[System.Serializable]           // パスワード再設定リクエスト
public class ResetPassRequest
{
    public string user_mail;
    public string newpassword;
}

[System.Serializable]           // トークン認証リクエスト
public class TokenRequest
{
    public string token;
}

