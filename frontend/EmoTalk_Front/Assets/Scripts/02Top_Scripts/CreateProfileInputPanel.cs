using UnityEngine;
using UnityEngine.UI;

public class CreateProfileInputPanel : MonoBehaviour
{
    [Header("Input UI")]
    public InputField nameInput;
    public InputField personalityInput;
    public InputField toneInput;
    public InputField pronounInput;

    [Header("Windows")]
    public CreateProfileModelSelect modelSelectPanel;  // CreatePanel_M

    private int selectedModelIndex = 0;

    // ===============================
    //  初期化
    // ===============================
    public void Open(int modelIndex)
    {
        selectedModelIndex = modelIndex;

        // 初期値設定（名前はモデル番号によって変える）
        nameInput.text = $"Model #{modelIndex + 1}";
        personalityInput.text = "";
        toneInput.text = "";
        pronounInput.text = "";

        gameObject.SetActive(true);
    }

    // ===============================
    //  閉じる
    // ===============================
    public void Close()
    {
        gameObject.SetActive(false);
    }

    // ==============================
    // 保存処理
    // ==============================
    public void OnSave()
    {
        string title = nameInput.text.Trim();
        string chara = personalityInput.text.Trim();
        string tone = toneInput.text.Trim();
        string firstPerson = pronounInput.text.Trim();

        if (string.IsNullOrEmpty(title))
        {
            Debug.LogWarning("タイトルを入力してください。");
            return;
        }

        ProfileManager.Instance.CreateProfile(
            selectedModelIndex,
            title,
            chara,
            tone,
            firstPerson
            );

        Close();
    }

    // ==============================
    // 戻る
    // ==============================
    public void OnBack()
    {
        Close();
        modelSelectPanel.Open();
    }
}