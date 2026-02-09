using System;
using System.Collections;
using UnityEngine;

// Whisper APIクライアント
public class WhisperClient : MonoBehaviour
{
    public TopController TopController; // ChatGPTへ送信するためのTopController参照

    // 音声認識が完了した際に実行されるイベント（引数は認識されたテキスト）
    public event Action<string> OnWhisperCompleted; 

    // ==============================
    // 音声データをWhisperへ送信
    // ==============================
    public void SendToWhisper(AudioClip clip)
    {
        // AudioClipをバイナリ（WAV形式）に変換
        byte[] wavBytes = WavUtility.AudioClipToWav(clip);

        // サーバー（PHP）のAPIへPostWav通信を開始
        StartCoroutine(
            ApiConnect.PostWav<WhisperResponse>(
                "PHP_api/wisper_api.php",
                wavBytes,
                OnSuccess,
                OnError
            )
        );
    }

    // ==============================
    // 通信成功時の処理
    // ==============================
    private void OnSuccess(WhisperResponse res)
    {
        //  レスポンスが失敗、またはテキストが空の場合のバリデーション
        if (!res.success || string.IsNullOrEmpty(res.text))
        {
            // 失敗時のSE（効果音）を鳴らす
            SEManager.Instance?.PlayWhisperError();

            Debug.LogError("Whisper_response: " + res.message);
            OnWhisperCompleted?.Invoke("");
            return;
        }

        // 成功時のSEを鳴らす
        SEManager.Instance?.PlayWhisperSuccess();

        // 認識結果をイベントで通知
        OnWhisperCompleted?.Invoke(res.text);
        
        // TopControllerにテキストを渡し、AI（ChatGPT）への送信シーケンスを開始させる
        TopController.SendMessageFromVoice(res.text);
    }

    // ==============================
    // 通信エラー時の処理
    // ==============================
    private void OnError(string error)
    {
        Debug.LogError(error);
        OnWhisperCompleted?.Invoke("");
    }
    
    // ==============================
    // テスト用のデバック
    // ==============================
    public void Test_SendText(string testText)
    {
        Debug.Log($"[TEST] Whisper result = {testText}");
        OnWhisperCompleted?.Invoke(testText);
    }

    // ==============================
    // APIレスポンス用データ構造
    // ==============================
    [Serializable]
    public class WhisperResponse
    {
        public bool success;    // 通信と処理が成功したか
        public string text;     // 文字起こしされたテキスト結果
        public string message;  // エラー時などのメッセージ
    }
}
