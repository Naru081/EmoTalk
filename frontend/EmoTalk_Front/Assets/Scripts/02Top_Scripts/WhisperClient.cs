using System;
using System.Collections;
using UnityEngine;

public class WhisperClient : MonoBehaviour
{
    public TopController TopController; // ChatGPTÇ÷ëóêMÇ∑ÇÈÇΩÇﬂÇÃTopControlleréQè∆

    public event Action<string> OnWisperCompleted;

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
            Debug.LogError("Whisperé∏îs: " + res.message);
            OnWisperCompleted?.Invoke("");
            return;
        }

        //Debug.Log("Whisperê¨å˜: " + res.text);
        OnWisperCompleted?.Invoke(res.text);

        TopController.SendMessageFromVoice(res.text);
    }

    private void OnError(string error)
    {
        Debug.LogError(error);
    }

    [Serializable]
    public class WhisperResponse
    {
        public bool success;
        public string text;
        public string message;
    }
}
