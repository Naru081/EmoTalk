using System;

// ===== PHP 通信用レスポンスデータクラス群 =====

[System.Serializable]
public class BaseResponseData  // successとmessageを返す共通レスポンス
{
    public bool success;
    public string message;
}

[System.Serializable]
public class LoginResponse      // ログインレスポンス
{
    public bool success;
    public string message;
    public int user_id;
    public string user_mail;
    public string token;
}

[System.Serializable]
public class TokenResponse // トークン認証レスポンス
{
    public bool success;
    public string message;
    public int user_id;
    public string user_mail;
    public int user_currentprof;
}

