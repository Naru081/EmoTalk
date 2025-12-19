using UnityEngine;
using UnityEngine.UI;

public class ProfileEditWindow : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;      // ウインドウの一番上のパネル(ないなら this.gameObject を使う)
    public Image modelImg;

    // ここは後で実装するボタン
    public Button selectButton;
    public Button deleteButton;
    public Button detailButton;
    public Button closeButton;

    [Header("Name Edit")]
    [SerializeField] private Text nameText;
    [SerializeField] private InputField nameInput;
    [SerializeField] private Button editNameButton;

    private ProfileData currentProfile;

    // リスト名更新用
    public ProfileListWindow listWindow;

    // モデル変更表示
    public ModelChangeWindow modelChangeWindow;

    // 削除確認ポップアップ・削除拒否ポップアップ
    [Header("Delete UI")]
    public DeleteConfirmPopup deleteConfirmPopup;
    public DeleteDeniedPopup deleteDeniedPopup;

    // プロファイル編集画面を開く
    [Header("Detail Window")]
    public PropertyWindow detailWindow;

    // ==============================
    // プロファイル画面を開く
    // ==============================
    public void Open(ProfileData profile)
    {
        currentProfile = profile;
        Debug.Log("Open called. currentProfile=" + (currentProfile != null ? currentProfile.displayName : "NULL"));

        // 表示用（Text）
        if (nameText != null) nameText.text = profile.displayName;

        if (modelImg != null) modelImg.sprite = ProfileController.GetImgSprite(profile.modelIndex);

        // 編集用（InputField）
        if (nameInput != null) nameInput.text = profile.displayName;

        if (nameText != null) nameText.gameObject.SetActive(true);
        if (nameInput != null) nameInput.gameObject.SetActive(false);

        root.SetActive(true);
    }

    // ==============================
    // プロファイル画面を閉じる
    // ==============================
    public void Close()
    {
        root.SetActive(false);

        if(listWindow != null)listWindow.RefreshList();
    }

    // ==============================
    // モデル選択ボタンの処理
    // ==============================
    public void OnClickSelect()
    {
        if (currentProfile == null) return;

        // 選択状態を保存 + Live2Dを切り替え
        ProfileManager.Instance.SelectProfile(currentProfile);

        // リストを即更新
        if (listWindow != null) listWindow.RefreshList();

        Close();
    }

    // ==============================
    // プロフィール削除ボタンの処理
    // ==============================
    public void OnClickDelete()
    {
        if (currentProfile == null) return;

        int selectedId = ProfileManager.Instance.GetSelectedProfileId();

        // 選択中は削除不可
        if (selectedId == currentProfile.profileId)
        {
            deleteDeniedPopup.Open(); // 3秒表示
            return;
        }

        // 削除確認
        deleteConfirmPopup.Open(() =>
        {
            ProfileManager.Instance.DeleteProfile(currentProfile.profileId);
            ProfileManager.Instance.NotifyChanged();
            Close();
        });
    }

    // ==============================
    // 詳細編集ボタンの処理
    // ==============================
    public void OnClickDetail()
    {
        if (currentProfile == null) return;
        if (detailWindow == null) return;

        detailWindow.Open(currentProfile);
    }

    public void OnClickClose()
    {
        Close();
    }

    // ==============================
    // 名前編集ボタンの処理
    // ==============================
    public void OnClickEditName()
    {
        Debug.Log("EditName clicked");

        if (nameText != null) nameText.gameObject.SetActive(false);
        
        if (nameInput != null)
        {
            nameInput.gameObject.SetActive(true);
            nameInput.ActivateInputField();
            nameInput.Select();
        }
    }
    // ==============================
    // 名前入力確定時の処理
    // ==============================
    public void OnEndEditName(string value)
    {
        // ★引数 value は信用せず、実体から読む
        string newName = (nameInput != null) ? nameInput.text : value;

        Debug.Log($"OnEndEditName fired: value='{value}' inputText='{newName}'");

        if (currentProfile == null) return;

        if (string.IsNullOrWhiteSpace(newName))newName = currentProfile.displayName;

        currentProfile.displayName = newName;

        if (nameText != null) nameText.text = currentProfile.displayName;

         // 編集UIを戻す（やっているなら）
        if (nameInput != null) nameInput.gameObject.SetActive(false);
        if (nameText != null) nameText.gameObject.SetActive(true);

        // リスト反映
        ProfileManager.Instance.SaveProfiles();
        //ProfileListWindow.Instance.RefreshList();
    }
    // ==============================
    // 名前入力変更時の処理
    // ==============================
    public void OnValueChangedName(string value)
    {
        if (currentProfile == null) return;

        string newName = (nameInput != null) ? nameInput.text : value;

        currentProfile.displayName = newName;
        if (nameText != null) nameText.text = newName;
    }

    // ==============================
    // モデル変更の選択画面表示
    // ==============================
    public void OnClickChangeModel()
    {
        if (currentProfile == null) return;
        if (modelChangeWindow == null) return;

        modelChangeWindow.Open(currentProfile);
    }
    // ==============================
    // 画面表示を更新する
    // ==============================
    public void RefreshCurrentView()
    {
        if (currentProfile == null) return;

        if (nameText != null) nameText.text = currentProfile.displayName;
        if (nameInput != null) nameInput.text = currentProfile.displayName;

        if (modelImg != null) modelImg.sprite = ProfileController.GetImgSprite(currentProfile.modelIndex);
    }
}