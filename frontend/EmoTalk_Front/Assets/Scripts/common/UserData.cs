using UnityEngine;

// ユーザーデータを保存・取得するクラス
public static class UserData
{
    private const string KEY_USER_ID = "user_id";
    private const string KEY_USER_MAIL = "user_mail";
    private const string KEY_USER_CURRENT_PROF = "user_currentprof";

    // user_idの保存
    public static void SaveUserId(int userId)
    {
        PlayerPrefs.SetInt(KEY_USER_ID, userId);
        PlayerPrefs.Save();
    }

    // user_idの取得
    public static int GetUserId()
    {
        return PlayerPrefs.GetInt(KEY_USER_ID, -1);
    }

    // user_mailの保存
    public static void SaveUserMail(string userMail)
    {
        PlayerPrefs.SetString(KEY_USER_MAIL, userMail);
        PlayerPrefs.Save();
    }

    // user_mailの取得
    public static string GetUserMail()
    {
        return PlayerPrefs.GetString(KEY_USER_MAIL, "");
    }

    // user_currentprofの保存
    public static void SaveUserCurrentProfId(int profileId)
    {
        PlayerPrefs.SetInt(KEY_USER_CURRENT_PROF, profileId);
        PlayerPrefs.Save();
    }

    // user_currentprofの取得
    public static int GetUserCurrentProfId()
    {
        return PlayerPrefs.GetInt(KEY_USER_CURRENT_PROF, -1);
    }

    // ログアウト時に全ユーザーデータをクリア
    public static void ClearUserData()
    {
        PlayerPrefs.DeleteKey(KEY_USER_ID);
        PlayerPrefs.DeleteKey(KEY_USER_MAIL);
        PlayerPrefs.DeleteKey(KEY_USER_CURRENT_PROF);
        PlayerPrefs.Save();
    }
}