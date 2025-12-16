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

    public void Close()
    {
        root.SetActive(false);

        if(listWindow != null)listWindow.RefreshList();
    }

    // ▼ボタン用のダミー実装（後で中身を作る）
    public void OnClickSelect()
    {
        Debug.Log($"Select profile: {currentProfile.displayName}");
        // ProfileManager.Instance.SetCurrentProfile(currentProfile); みたいな処理を後で書く
    }

    public void OnClickDelete()
    {
        Debug.Log($"Delete request: {currentProfile.displayName}");
        // ここで削除確認ポップアップを出す予定
    }

    public void OnClickDetail()
    {
        Debug.Log($"Detail: {currentProfile.displayName}");
        // カスタム画面(CreatePanel_P 相当)へ遷移する処理を後で書く
    }

    public void OnClickClose()
    {
        Close();
    }

    // 名前編集ボタンの処理
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
     // 名前入力確定時の処理
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
    public void OnValueChangedName(string value)
    {
        if (currentProfile == null) return;

        string newName = (nameInput != null) ? nameInput.text : value;

        currentProfile.displayName = newName;
        if (nameText != null) nameText.text = newName;
    }
}