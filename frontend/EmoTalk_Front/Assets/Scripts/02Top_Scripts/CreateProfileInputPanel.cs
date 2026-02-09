using UnityEngine;
using UnityEngine.UI;

public class CreateProfileInputPanel : MonoBehaviour
{
    [Header("Input UI")]
    // ユーザーの各入力フィールド
    public InputField nameInput;
    public InputField personalityInput;
    public InputField toneInput;
    public InputField pronounInput;

    [Header("Windows")]
    // 戻るボタンを押したときに表示するモデル選択パネル
    public CreateProfileModelSelect modelSelectPanel;

    // 選択したモデルのインデックス
    private int selectedModelIndex = 0;

    // ===============================
    //  初期化
    // ===============================
    public void Open(int modelIndex)
    {
        selectedModelIndex = modelIndex;

        // 初期値設定（名前はモデル番号によって自動入力する）
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
    // 入力された文字をバリデーションして、プロファイルを保存
    public void OnSave()
    {
        string title = nameInput.text.Trim();
        string chara = personalityInput.text.Trim();
        string tone = toneInput.text.Trim();
        string firstPerson = pronounInput.text.Trim();

        // ----- 各種チェック処理 -----

        // タイトルが空欄もしくは10文字を越える場合は警告ポップアップを出して保存処理を中止
        if (string.IsNullOrWhiteSpace(title) || title.Length > 10)
        {
            // 警告ポップアップ表示
            ProfileManager.Instance.ShowWarningTitlePopup();
            return;
        }

        // 性格が空欄もしくは20文字を越える場合は警告ポップアップを出して保存処理を中止
        if (string.IsNullOrWhiteSpace(chara) || chara.Length > 20)
        {
            // 警告ポップアップ表示
            ProfileManager.Instance.ShowWarningCharaPopup();
            return;
        }

        // 口調が空欄もしくは20文字を越える場合は警告ポップアップを出して保存処理を中止
        if (string.IsNullOrWhiteSpace(tone) || tone.Length > 20)
        {
            // 警告ポップアップ表示
            ProfileManager.Instance.ShowWarningTonePopup();
            return;
        }

        // 一人称が空欄もしくは20文字を越える場合は警告ポップアップを出して保存処理を中止
        if (string.IsNullOrWhiteSpace(firstPerson) || firstPerson.Length > 20)
        {
            // 警告ポップアップ表示
            ProfileManager.Instance.ShowWarningFpPopup();
            return;
        }

        // プロファイル作成
        ProfileManager.Instance.CreateProfile(
            selectedModelIndex,
            title,
            chara,
            tone,
            firstPerson
        );

        // 保存成功ポップアップを表示
        ProfileManager.Instance.ShowSuccessSavePopup();

        Close();
    }

    // ==============================
    // 戻る(入力途中の項目は破棄する)
    // ==============================
    public void OnBack()
    {
        Close();
        modelSelectPanel.Open();
    }
}