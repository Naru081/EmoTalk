using System;
using System.Collections.Generic;

// ===== PHP 通信用レスポンスデータクラス群 =====

[System.Serializable]
public class BaseResponseData       // successとmessageを返す共通レスポンス
{
    public bool success;
    public string message;
}

[System.Serializable]
public class RegisterResponse   // 新規登録リクエスト
{
    public bool success;
    public string message;
    public int user_id;
    public string user_mail;
    public int user_currentprof;
}

[System.Serializable]
public class LoginResponse          // ログインレスポンス
{
    public bool success;
    public string message;
    public int user_id;
    public string user_mail;
    public int user_currentprof;
    public string token;
}

[System.Serializable]
public class TokenResponse          // トークン認証レスポンス
{
    public bool success;
    public string message;
    public int user_id;
    public string user_mail;
    public int user_currentprof;
}

[System.Serializable]          
public class GetProfileResponse     // プロファイル一覧取得レスポンス
{
    public bool success;
    public string message;
    public List<ProfileDataFromDB> profile_data;
}

[System.Serializable]               // DBから取得したプロフィールデータ格納用クラス
public class ProfileDataFromDB
{
    public int prof_id;
    public int user_id;
    public int model_id;
    public string prof_title;
    public string prof_chara;
    public string prof_tone;
    public string prof_fp;
}

[Serializable]                      // プロファイル作成レスポンス
public class CreateProfileResponse
{
    public bool success;
    public string message;
    public List<ProfileDataFromDB> profiles;
}

[Serializable]                      // メインチャットレスポンス
public class  MessageResponse
{
    public bool success;
    public string message;
    public string response_text;
    public string emotion;
    public string response_text_hiragana;
    public string model_voice;
}

[Serializable]                      // CoeiroInkレスポンス
public class CoeiroInkResponse
{
    public bool success;
    public string message;
    public string voice_wav_base64;
}