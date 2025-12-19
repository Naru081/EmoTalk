using UnityEngine;
using UnityEngine.UI;

public class PropertyWindow : MonoBehaviour
{
    [Header("Root")]
    public GameObject root; // このパネル

    [Header("Inputs")]
    public InputField personalityInput;
    public InputField toneInput;
    public InputField pronounInput;

    [Header("Buttons")]
    public Button saveButton;
    public Button cancelButton;

    private ProfileData target;

    // ==============================
    // プロパティ画面を開く
    // ==============================
    public void Open(ProfileData profile)
    {
        target = profile;

        // 現在値をUIへ反映
        if (personalityInput != null) personalityInput.text = target.personality ?? "";
        if (toneInput != null) toneInput.text = target.tone ?? "";
        if (pronounInput != null) pronounInput.text = target.pronoun ?? "";

        if (root != null) root.SetActive(true);
        else gameObject.SetActive(true);

        // ボタンイベントはOpen時に付ける（Awake禁止方針）
        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(OnClickSave);
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(Close);
        }
    }

    // ==============================
    // プロパティ画面を閉じる
    // ==============================
    public void Close()
    {
        if (root != null) root.SetActive(false);
        else gameObject.SetActive(false);

        target = null;
    }

    // ==============================
    // 保存ボタン押したとき
    // ==============================
    private void OnClickSave()
    {
        if (target == null) return;

        target.personality = personalityInput != null ? personalityInput.text : "";
        target.tone = toneInput != null ? toneInput.text : "";
        target.pronoun = pronounInput != null ? pronounInput.text : "";

        // 保存＆通知（あなたのUpdateProfileはNotifyChangedも入っている）:contentReference[oaicite:3]{index=3}
        ProfileManager.Instance.UpdateProfile(target);

        Close();
    }
}