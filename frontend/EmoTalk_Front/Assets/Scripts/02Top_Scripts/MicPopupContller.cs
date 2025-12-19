using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MicController : MonoBehaviour
{
    [Header("Panel")]
    public GameObject micPanel;            // MicPanel

    [Header("UI")]
    public Text titleText;                 // txt_title
    public Image micImage;                 // img_mic
    public Sprite micOnSprite;             // mic_on.png
    public Sprite micDisableSprite;        // mic_disable.png

    [Header("Animation")]
    public float blinkSpeed = 0.6f;        // 点滅速度

    private Coroutine blinkCoroutine;

    // ==============================
    // マイクボタンが押された処理
    // ==============================
    public void OnMicButton()
    {
        micPanel.SetActive(true);

        // 権限チェック
        if (!HasMicrophonePermission())
        {
            ShowMicDisabled();
            return;
        }

        StartRecordingUI();
    }

    // ==============================
    // 録音UIを開始（録音中の見た目）
    // ==============================
    private void StartRecordingUI()
    {
        titleText.text = "録音中・・・";
        micImage.sprite = micOnSprite;

        // 点滅開始
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkMicIcon());
    }

    // ==============================
    // エラー表示：聞き取れなかった場合
    // ==============================
    public void ShowNoInput()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

        titleText.text = "聞き取れませんでした";
        micImage.sprite = micOnSprite;
        micImage.color = Color.white; // 点滅終了
    }

    // ===============================
    // エラー表示：マイクが無効の場合
    // ===============================
    private void ShowMicDisabled()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

        titleText.text = "マイクが無効になっています";
        micImage.sprite = micDisableSprite;
        micImage.color = Color.white;
    }

    // ==============================
    // マイクを閉じる
    // ==============================
    public void ClosePanel()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        micPanel.SetActive(false);
    }

    // ==============================
    // 点滅アニメーション
    // ==============================
    private IEnumerator BlinkMicIcon()
    {
        while (true)
        {
            // フェードアウト
            for (float a = 1; a >= 0.3f; a -= Time.deltaTime * 2f)
            {
                micImage.color = new Color(1, 1, 1, a);
                yield return null;
            }
            // フェードイン
            for (float a = 0.3f; a <= 1; a += Time.deltaTime * 2f)
            {
                micImage.color = new Color(1, 1, 1, a);
                yield return null;
            }
        }
    }

    // ==============================
    // マイク権限チェック（実装は後で追加）
    // ==============================
    private bool HasMicrophonePermission()
    {
        #if UNITY_EDITOR
        {
            return true; // エディタでは常に許可
        }
        #else
        {
            return Application.HasUserAuthorization(UserAuthorization.Microphone);
        }    
        #endif
    }
}