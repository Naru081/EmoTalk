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
        // ① 新規作成
        ProfileData data = ProfileManager.Instance.CreateProfile(selectedModelIndex);

        // ② 入力反映
        data.displayName = nameInput.text;
        data.personality = personalityInput.text;
        data.tone        = toneInput.text;
        data.pronoun     = pronounInput.text;

        // ③ 更新保存
        ProfileManager.Instance.UpdateProfile(data);

        // ④ ハンバーガーメニュー更新
        if (ProfileListWindow.Instance != null)
        {
            ProfileListWindow.Instance.RefreshList();
        }
        else
        {
            Debug.LogWarning("ProfileListWindow.Instance が null です！");
        }

        // ⑤ 閉じる
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