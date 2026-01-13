using UnityEngine;

public class ModelChangeWindow : MonoBehaviour
{
    public GameObject root;                 // このパネル（自分自身でもOK）
    public ProfileEditWindow editWindow;    // 戻り先（編集画面）

    private ProfileData targetProfile;

    // ==============================
    // モデル変更ウィンドウの表示
    // ==============================
    public void Open(ProfileData profile)
    {
        targetProfile = profile;
        if (root != null) root.SetActive(true);
        else gameObject.SetActive(true);
    }

    // ==============================
    // モデル変更ウィンドウの非表示
    // ==============================
    public void Close()
    {
        if (root != null) root.SetActive(false);
        else gameObject.SetActive(false);
    }
    
    // ==============================
    // モデル選択ボタンから呼ばれる
    // ==============================
    public void SelectModel(int modelIndex)
    {
        if (targetProfile == null) return;

        // ① データ更新
        targetProfile.modelIndex = modelIndex;

        // ② DB保存
        //ProfileManager.Instance.UpdateProfile(targetProfile);  // SaveProfilesしてくれる :contentReference[oaicite:3]{index=3}
        ProfileManager.Instance.UpdateProfileModel(targetProfile);

        // ④ 編集画面の表示更新（画像・名前など）
        if (editWindow != null)
            editWindow.RefreshCurrentView();   // ↓次で追加する

        // ⑤ ハンバーガーリスト即更新（イベント通知） :contentReference[oaicite:5]{index=5}
        ProfileManager.Instance.NotifyChanged();

        Close();
    }
}