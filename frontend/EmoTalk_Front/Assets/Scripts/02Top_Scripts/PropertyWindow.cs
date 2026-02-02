using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    private string originalPersonality; // テキストボックスの元の性格を避難用
    private string originalTone;        // テキストボックスの元の口調を避難用
    private string originalPronoun;     // テキストボックスの元の一人称を避難用

    // 警告文表示用ポップアップ
    public PopupManager warningCharaPopup;    // 性格用
    public PopupManager warningTonePopup;     // 口調用
    public PopupManager warningFpPopup;       // 一人称用

    // ==============================
    // プロパティ画面を開く
    // ==============================
    public void Open(ProfileData profile)
    {
        target = profile;

        // 変更前の値を保存
        originalPersonality = target.personality;
        originalTone = target.tone;
        originalPronoun = target.pronoun;

        // 現在値をUIへ反映
        if (personalityInput != null) personalityInput.text = target.personality ?? "";
        if (toneInput != null) toneInput.text = target.tone ?? "";
        if (pronounInput != null) pronounInput.text = target.pronoun ?? "";

        if (root != null) root.SetActive(true);
        else gameObject.SetActive(true);

        // ボタンイベントはOpen時に付ける
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

        string c = personalityInput.text;
        string t = toneInput.text;
        string f = pronounInput.text;

        // 性格チェック 空の場合もしくは20文字を越える場合は警告ポップアップを出して元の値に戻し、保存処理を中止
        if (string.IsNullOrWhiteSpace(c) || c.Length > 20)
        {
            personalityInput.text = originalPersonality ?? "";
            warningCharaPopup.Open();
            StartCoroutine(OpenNextFrame(warningCharaPopup));
            return;
        }

        // 口調チェック 空の場合もしくは20文字を越える場合は警告ポップアップを出して元の値に戻し、保存処理を中止
        if (string.IsNullOrWhiteSpace(t) || t.Length > 20)
        {
            toneInput.text = originalTone ?? "";
            warningTonePopup.Open();
            StartCoroutine(OpenNextFrame(warningTonePopup));
            return;
        }

        // 一人称チェック 空の場合もしくは20文字を越える場合は警告ポップアップを出して元の値に戻し、保存処理を中止
        if (string.IsNullOrWhiteSpace(f) || f.Length > 20)
        {
            pronounInput.text = originalPronoun ?? "";
            warningFpPopup.Open();
            StartCoroutine(OpenNextFrame(warningFpPopup));
            return;
        }

        // ポップアップを正しく表示するために1フレーム待つ
        IEnumerator OpenNextFrame(PopupManager popup)
        {
            yield return null; // 1フレーム待つ
            popup.Open();
        }

        // 全てOKの場合、入力値をターゲットに反映し保存処理を開始
        target.personality = c;
        target.tone = t;
        target.pronoun = f;

        ProfileManager.Instance.UpdateProfileCustom(
            target,
            () =>
            {
                Close();
            }
        );
    }
}