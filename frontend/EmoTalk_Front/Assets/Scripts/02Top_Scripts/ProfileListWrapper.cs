using System;
using System.Collections.Generic;

// ==============================
// データリストのラッパークラス
// ==============================
[Serializable]
public class ProfileListWrapper
{
    // プロファイルデータのリスト
    public List<ProfileData> profiles;

    // リストを確実に初期化し、NullReferenceException を防ぐ
    public ProfileListWrapper()
    {
        profiles = new List<ProfileData>();
    }
}