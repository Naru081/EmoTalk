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

    [Header("Audio & API")]
    public MicRecorder micRecorder;        // 録音担当クラス
    public WhisperClient whisperClient;    // Whisper + ChatGpt + CoeiroInk 担当クラス
        
    private void Awake()
    {
        // Whisper完了時のイベント読み込み
        whisperClient.OnWisperCompleted += HandleWhisperCompleted;
    }

    private void OnDestroy()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        whisperClient.OnWisperCompleted -= HandleWhisperCompleted;
    }

    // Whiper完了時の処理
    private void HandleWhisperCompleted(string recognizedText)
    {
        //Debug.Log($"HandleWhisperCompleted called / text='{recognizedText}'");

        Debug.Log($"titleText={titleText}");
        Debug.Log($"micImage={micImage}");
        Debug.Log($"micPanel={micPanel}");

        if (string.IsNullOrEmpty(recognizedText))
        {
            ShowNoInput();
            return;
        }

        Debug.Log("Whisper認識完了 " + recognizedText);
        ClosePanel();
    }

    // イベントを1回のみにするための変数
    private bool isRecording = false;

    // ==============================
    // マイクボタンが押された処理
    // ==============================
    public void OnMicButton()
    {
        if (isRecording) return; // 2回目以降の呼び出しを無視

        micPanel.SetActive(true);

        // 権限チェック
        if (!HasMicrophonePermission())
        {
            ShowMicDisabled();
            return;
        }

        // 録音開始
        isRecording = true;
        micRecorder.StartRecording();
        StartRecordingUI();
    }

    // ==============================
    // 録音停止ボタン or 自動停止時の処理
    // ==============================
    public void OnStopRecording()
    {
        Debug.Log("StopRecording called");

        if (!isRecording) return; // 2回目以降の呼び出しを無視
        isRecording = false;

        AudioClip clip = micRecorder.StopRecording();

        if (clip == null)
        {
            Debug.LogError("録音失敗：clipがnull");
            ShowNoInput();
            return;
        }

        Debug.Log($"length={clip.length}");
        Debug.Log($"samples={clip.samples}");
        Debug.Log($"channels={clip.channels}");
        Debug.Log($"frequency={clip.frequency}");

        if (clip.length < 0.3f)
        {
            ShowNoInput();
            return;
        }

        ClosePanel();
        titleText.text = "解析中・・・";

        // Whisperへ送信
        whisperClient.SendToWhisper(clip);
    }

    // ==============================
    // 録音UIを開始（録音中の見た目）
    // ==============================
    private void StartRecordingUI()
    {
        titleText.text = "録音中・・・";
        micImage.sprite = micOnSprite;
        micImage.color = Color.white;

        // 点滅開始
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkMicIcon());
    }

    // ==============================
    // エラー表示：聞き取れなかった場合
    // ==============================
    public void ShowNoInput()
    {
        //if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

        titleText.text = "聞き取れませんでした";
        micImage.sprite = micOnSprite;
        micImage.color = Color.white; // 点滅終了
    }

    // ===============================
    // エラー表示：マイクが無効の場合
    // ===============================
    private void ShowMicDisabled()
    {
        //if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

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

        //micImage.color = Color.white;
        micPanel.SetActive(false);
    }

    // ==============================
    // 点滅アニメーション
    // ==============================
    private IEnumerator BlinkMicIcon()
    {
        while (micPanel.activeInHierarchy)
        {
            for (float a = 1; a >= 0.3f; a -= Time.deltaTime * 2f)
            {
                if (!micPanel.activeInHierarchy) yield break;
                micImage.color = new Color(1, 1, 1, a);
                yield return null;
            }

            for (float a = 0.3f; a <= 1; a += Time.deltaTime * 2f)
            {
                if (!micPanel.activeInHierarchy) yield break;
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