using UnityEngine;
using UnityEngine.UI;

public class ProfileEditWindow : MonoBehaviour
{
    [Header("UI")]
    public Text titleText;
    public InputField nameInput;
    public Image iconImage;

    public Button deleteButton;
    public Button detailButton;
    public Button selectButton;
    public Button closeButton;

    [Header("Windows")]
    public ProfilePropertyWindow propertyWindow;   // 性格・口調のカスタム画面
    public DeleteConfirmDialog deleteDialog;       // 削除確認ダイアログ
    public ModelSelectWindow modelSelectWindow;    // モデル変更画面（Model #1, #2, #3）

    private ProfileData currentProfile;

    // -----------------------------
    // 開く（ProfileListWindow から呼ばれる）
    // -----------------------------
    public void Open(ProfileData data)
    {
        currentProfile = data;

        gameObject.SetActive(true);

        RefreshUI();
    }

    // -----------------------------
    // UI更新
    // -----------------------------
    private void RefreshUI()
    {
        nameInput.text = currentProfile.displayName;
        titleText.text = currentProfile.displayName;

        // モデルアイコン更新
        iconImage.color = GetColorByModelIndex(currentProfile.modelIndex);

        // 選択中プロフィールの場合は削除不可
        bool isSelected = (ProfileManager.Instance.GetSelectedProfile() == currentProfile);
        deleteButton.interactable = !isSelected;
    }

    private Color GetColorByModelIndex(int index)
    {
        switch (index)
        {
            case 0: return new Color(0.5f, 0.7f, 1f);
            case 1: return new Color(1f, 0.7f, 0.4f);
            case 2: return new Color(0.5f, 1f, 0.5f);
        }
        return Color.white;
    }

    // -----------------------------
    // 名前変更
    // -----------------------------
    public void OnNameChanged(string newName)
    {
        currentProfile.displayName = newName;
        ProfileManager.Instance.UpdateProfile(currentProfile);
        RefreshUI();
    }

    // -----------------------------
    // モデル変更（ModelSelectWindow を開く）
    // -----------------------------
    public void OnChangeModel()
    {
        modelSelectWindow.Open(currentProfile, OnModelSelected);
    }

    private void OnModelSelected(int newModelIndex)
    {
        currentProfile.modelIndex = newModelIndex;
        ProfileManager.Instance.UpdateProfile(currentProfile);
        RefreshUI();
    }

    // -----------------------------
    // 詳細（性格・口調編集）
    // -----------------------------
    public void OnDetail()
    {
        propertyWindow.Open(currentProfile, OnPropertySaved);
    }

    private void OnPropertySaved()
    {
        ProfileManager.Instance.UpdateProfile(currentProfile);
        RefreshUI();
    }

    // -----------------------------
    // 選択（Live2D切り替え）
    // -----------------------------
    public void OnSelect()
    {
        ProfileManager.Instance.SelectProfile(currentProfile);
        RefreshUI();
    }

    // -----------------------------
    // 削除確認 → ダイアログへ
    // -----------------------------
    public void OnDelete()
    {
        deleteDialog.Open(
            confirmAction: () =>
            {
                bool result = ProfileManager.Instance.DeleteProfile(currentProfile);
                if (result)
                    Close();
            }
        );
    }

    // -----------------------------
    // 閉じる
    // -----------------------------
    public void Close()
    {
        gameObject.SetActive(false);
    }
}