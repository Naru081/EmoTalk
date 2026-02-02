using System;
using System.Collections;
using UnityEngine;

public class WhisperClient : MonoBehaviour
{
    public TopController TopController; // ChatGPTへ送信するためのTopController参照

    public event Action<string> OnWhisperCompleted;

    public void SendToWhisper(AudioClip clip)
    {
        byte[] wavBytes = WavUtility.AudioClipToWav(clip);

        StartCoroutine(
            ApiConnect.PostWav<WhisperResponse>(
                "PHP_api/wisper_api.php",
                wavBytes,
                OnSuccess,
                OnError
            )
        );
    }

    private void OnSuccess(WhisperResponse res)
    {
        if (!res.success || string.IsNullOrEmpty(res.text))
        {
            SEManager.Instance?.PlayWhisperError();

            Debug.LogError("Whisper_response: " + res.message);
            OnWhisperCompleted?.Invoke("");
            return;
        }

        SEManager.Instance?.PlayWhisperSuccess();

        //Debug.Log("Whisper成功: " + res.text);
        OnWhisperCompleted?.Invoke(res.text);

        TopController.SendMessageFromVoice(res.text);
    }

    private void OnError(string error)
    {
        Debug.LogError(error);
        OnWhisperCompleted?.Invoke("");
    }
    
    // テスト用
    public void Test_SendText(string testText)
    {
        Debug.Log($"[TEST] Whisper result = {testText}");
        OnWhisperCompleted?.Invoke(testText);
    }

    [Serializable]
    public class WhisperResponse
    {
        public bool success;
        public string text;
        public string message;
    }
}
