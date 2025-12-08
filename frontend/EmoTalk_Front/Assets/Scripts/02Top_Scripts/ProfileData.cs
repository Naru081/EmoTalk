using System;

[Serializable]
public class ProfileData
{
    public int profileId;        // DBで保存されるID（未保存なら -1）
    public string displayName;   // プロフィール名
    public int modelIndex;       // 利用モデル（0,1,2）
    public string personality;   // 性格
    public string tone;          // 口調
    public string pronoun;       // 一人称

    // コンストラクタ（新規作成用）
    public ProfileData(int modelIndex, string name = "New Profile")
    {
        this.profileId   = -1;           // 未保存
        this.displayName = name;
        this.modelIndex  = modelIndex;
        this.personality = "";
        this.tone        = "";
        this.pronoun     = "";
    }
}
