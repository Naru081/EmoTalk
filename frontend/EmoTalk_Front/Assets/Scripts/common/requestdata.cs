using System;

// ===== PHP 通信用リクエストデータクラス群 =====

[System.Serializable]
public class UserRequest        // 新規登録・ログイン共通リクエスト
{
    public string user_mail;
    public string user_pass;
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


[System.Serializable]           // user_idのみリクエスト
public class UserIdWrapper
{
    public int user_id;
}

[System.Serializable]           // プロファイル取得リクエスト
public class  UserCurrentProfileRequest
{
    public int user_id;
    public int prof_id;
    public int current_prof_id;
}

[Serializable]                  // プロファイル作成リクエスト
public class CreateProfileRequest
{
    public int user_id;
    public int model_id;
    public string prof_title;
    public string prof_chara;
    public string prof_tone;
    public string prof_fp;
}

[System.Serializable]           // プロファイルタイトル更新リクエスト
public class UpdateProfileTitleRequest
{
    public int user_id;
    public int prof_id;
    public string prof_title;
}

[System.Serializable]           // プロファイルカスタム設定更新リクエスト
public class UpdateProfileCustomRequest
{
    public int user_id;
    public int prof_id;
    public string prof_chara;
    public string prof_tone;
    public string prof_fp;
}

[System.Serializable]           // プロファイルモデル更新リクエスト
public class UpdateProfileModelRequest
{
    public int user_id;
    public int prof_id;
    public int model_id;
}

[Serializable]                  // プロファイル削除リクエスト
public class DeleteProfileRequest
{
    public int user_id;
    public int prof_id;
}

[Serializable]                  // テキストメッセージ送信リクエスト
public class MessageRequest
{
    public int prof_id;
    public string message_content;
}

[Serializable]                  // 音声メッセージ送信リクエスト
public class SendVoiceRequest
{
    public int prof_id;
    public byte[] wavdata;      
}

// 1/14追加
[Serializable]                  // CoeiroInk送信リクエスト
public class CoeiroInkRequest
{
    public string model_voice;
    public string responseText_hiragana;
}