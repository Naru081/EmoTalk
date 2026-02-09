using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MicController : MonoBehaviour
{
    [Header("TEST")]
    [SerializeField] private bool TEST_MODE = true;

    [Header("Panel")]
    public GameObject micPanel;

    [Header("UI")]
    public Text titleText;
    public Image micImage;
    public Sprite micOnSprite;
    public Sprite micDisableSprite;
    public GameObject close;

    [Header("Auto Stop")]
    public float silenceThreshold = 0.01f;
    public float silenceDuration = 1.5f;
    public float maxRecordTime = 10f;

    [Header("Refs")]
    public MicRecorder micRecorder;
    public WhisperClient whisperClient;

    private bool isRecording = false;
    private float silenceTimer = 0f;
    private float recordTimer = 0f;

    // 一度でも音を検出したら無音検出を有効化するためのフラグ
    private bool hasDetectedSound = false;

    // ==============================
    // マイクボタン
    // ==============================
    public void OnMicButton()
    {
        micPanel.SetActive(true);

        // ===== テストモード =====
        if (TEST_MODE)
        {
            Debug.Log("TEST_MODE: ダミー音声で実行");

            titleText.text = "解析中・・・";

            whisperClient.Test_SendText("これはテスト音声からの認識結果です。");

            AudioClip dummy = micRecorder.CreateDummyClip();
            whisperClient.SendToWhisper(dummy);

            StartCoroutine(TestAutoClose());
            return;
        }

        // ===== 本番 =====
        if (!micRecorder.StartRecording())
        {
            ShowMicDisabled();
            return;
        }

        // 録音状態初期化
        isRecording = true;
        recordTimer = 0f;
        silenceTimer = 0f;
        hasDetectedSound = false;

        titleText.text = "録音中・・・";
        micImage.sprite = micOnSprite;
    }

    // ==============================
    // 録音監視（無音 / 最大時間）
    // ==============================
    private void Update()
    {
        if (!isRecording) return;

        // 最大録音時間を超えたら自動停止
        recordTimer += Time.deltaTime;
        if (recordTimer >= maxRecordTime)
        {
            if (!hasDetectedSound)
            {
                Debug.Log("10秒間無音のため自動停止及び解析失敗");
                OnRecordingFailed("音声が検出されませんでした");
            }
            else 
            {
                Debug.Log("10秒経過で自動停止");
                OnStopRecording();
            }
            return;
        }

        // 現在の音量を取得
        float volume = micRecorder.GetCurrentVolume();

        if (volume >= silenceThreshold)
        {   
            // 音を検出して値を超えたら無音タイマーリセット
            hasDetectedSound = true;
            silenceTimer = 0f;
            return;
        }

        // 音が出るまでは無音判定なし
        if (!hasDetectedSound) return;

        // 音が出てから無音検出
        silenceTimer += Time.deltaTime;
        if (silenceTimer >= silenceDuration)
        {
            Debug.Log("無音検出で自動停止");
            OnStopRecording();
        }
    }

    // ==============================
    // 停止処理
    // ==============================
    public void OnStopRecording()
    {
        if (!isRecording) return;
        isRecording = false;

        // 録音を停止して音声データを取得
        AudioClip clip = micRecorder.StopRecording();
        if (clip == null)
        {
            ShowMicDisabled();
            return;
        }

        titleText.text = "解析中・・・";
        whisperClient.SendToWhisper(clip);

    }

    // ==============================
    // エラー表示
    // ==============================
    private void ShowMicDisabled()
    {
        titleText.text = "マイクが無効になっています";
        micImage.sprite = micDisableSprite;

        if (close != null)
            close.SetActive(false);

        StartCoroutine(TestAutoClose());
    }

    // ==============================
    // マイクパネルを自動で閉じる
    // ==============================
    private IEnumerator TestAutoClose()
    {
        yield return new WaitForSeconds(3f);
        micPanel.SetActive(false);
    }

    // 閉じるボタン
    public void OnCloseButton()
    {
        micPanel.SetActive(false);
    }

    // ==============================
    // WhisperAPI 完了処理
    // ==============================
    public void Start()
    {
        whisperClient.OnWhisperCompleted += OnWhisperResult;
    }

    private void OnDestroy()
    {   // メモリリーク防止の処理
        whisperClient.OnWhisperCompleted -= OnWhisperResult;
    }

    private void OnWhisperResult(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            // 解析失敗時
            titleText.text = "音声を認識できませんでした";
            if (close != null) close.SetActive(true);
            return;
        }

        // 解析成功時
        micPanel.SetActive(false);
    }

    // ==============================
    // 録音失敗時処理
    // ==============================
    private void OnRecordingFailed(string message)
    {
        // 録音状態終了
        isRecording = false;

        //マイク停止
        micRecorder.StopRecording();
        titleText.text = message;
        micImage.sprite = micDisableSprite;

        // 解析せずに閉じる
        StartCoroutine(TestAutoClose());
    }
}
