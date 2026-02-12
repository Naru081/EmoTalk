using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 音声入力の開始、無音検出、録音停止、Whisper送信を管理するクラス
public class MicController : MonoBehaviour
{
    [Header("TEST")]
    public bool TEST_MODE = false;   // テストモードフラグ

    [Header("Panel")]
    public GameObject micPanel;      // マイクUIパネル

    [Header("UI")]
    public Text titleText;           // 状態テキスト
    public Image micImage;           // マイクアイコン
    public Sprite micOnSprite;       // 録音中アイコン
    public Sprite micDisableSprite;  // 無効アイコン
    public GameObject close;         // 閉じるボタン

    [Header("Auto Stop")]
    public float silenceThreshold = 0.01f; // 無音判定閾値
    public float silenceDuration = 1.5f;   // 無音継続時間
    public float maxRecordTime = 10f;      // 最大録音時間

    [Header("Refs")]
    public MicRecorder micRecorder;        // 録音処理クラス
    public WhisperClient whisperClient;    // Whisper送信クラス

    private bool isRecording = false;      // 録音中フラグ
    private float silenceTimer = 0f;       // 無音時間
    private float recordTimer = 0f;        // 録音時間
    private bool hasDetectedSound = false; // 発話検出フラグ

    // マイクボタン押下処理
    public void OnMicButton()
    {
        micPanel.SetActive(true);

        // テストモード処理
        if (TEST_MODE)
        {
            titleText.text = "解析中";
            whisperClient.Test_SendText("これはテスト音声からの認識結果です");
            AudioClip dummy = micRecorder.CreateDummyClip();
            whisperClient.SendToWhisper(dummy);
            StartCoroutine(AutoClose());
            return;
        }

        // 録音開始処理
        if (!micRecorder.StartRecording())
        {
            ShowMicDisabled();
            return;
        }

        // 録音状態の初期化
        isRecording = true;
        recordTimer = 0f;
        silenceTimer = 0f;
        hasDetectedSound = false;

        titleText.text = "録音中";
        micImage.sprite = micOnSprite;
    }

    private void Update()
    {
        if (!isRecording)
        {
            return;
        }

        // 最大録音時間のチェック
        recordTimer += Time.deltaTime;
        if (recordTimer >= maxRecordTime)
        {
            if (!hasDetectedSound)
            {
                OnRecordingFailed("音声が検出されませんでした");
            }
            else
            {
                OnStopRecording();
            }
            return;
        }

        // 音量取得と無音判定
        float volume = micRecorder.GetCurrentVolume();

        // 発話検出処理
        if (volume >= silenceThreshold)
        {
            hasDetectedSound = true;
            silenceTimer = 0f;
            return;
        }

        // 発話前の無音は無視
        if (!hasDetectedSound)
        {
            return;
        }

        // 発話後の無音時間計測
        silenceTimer += Time.deltaTime;
        if (silenceTimer >= silenceDuration)
        {
            OnStopRecording();
        }
    }

    // 録音停止処理
    public void OnStopRecording()
    {
        if (!isRecording)
        {
            return;
        }

        isRecording = false;
        titleText.text = "解析中";

        // iOS対応の非同期録音停止処理
        StartCoroutine(micRecorder.StopRecordingAsync((clip) =>
        {
            if (clip == null)
            {
                ShowMicDisabled();
                return;
            }

            // Whisper APIへ送信
            whisperClient.SendToWhisper(clip);
        }));
    }

    // マイク無効時の表示処理
    private void ShowMicDisabled()
    {
        titleText.text = "マイクが無効です";
        micImage.sprite = micDisableSprite;

        if (close != null)
        {
            close.SetActive(false);
        }

        StartCoroutine(AutoClose());
    }

    // パネル自動クローズ処理
    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(3f);
        micPanel.SetActive(false);
    }

    public void OnCloseButton()
    {
        micPanel.SetActive(false);
    }

    public void Start()
    {
        // Whisper解析完了イベント登録
        whisperClient.OnWhisperCompleted += OnWhisperResult;
    }

    private void OnDestroy()
    {
        whisperClient.OnWhisperCompleted -= OnWhisperResult;
    }

    // Whisper解析結果受信処理
    private void OnWhisperResult(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            titleText.text = "音声を認識できませんでした";
            if (close != null)
            {
                close.SetActive(true);
            }
            return;
        }

        micPanel.SetActive(false);
    }

    // 録音失敗処理
    private void OnRecordingFailed(string message)
    {
        isRecording = false;
        titleText.text = message;
        micImage.sprite = micDisableSprite;
        StartCoroutine(AutoClose());
    }
}
