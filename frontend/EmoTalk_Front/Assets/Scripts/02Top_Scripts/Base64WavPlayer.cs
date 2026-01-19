using UnityEngine;
using System;

public class Base64WavPlayer : MonoBehaviour
{
    public AudioSource audioSource;

    public void PlayFromBase64(string base64)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource が設定されていません");
            return;
        }

        if (string.IsNullOrEmpty(base64))
        {
            Debug.LogError("base64 データが空です");
            return;
        }

        byte[] wavBytes;
        try
        {
            wavBytes = Convert.FromBase64String(base64);
        }
        catch (Exception e)
        {
            Debug.LogError("Base64 デコード失敗: " + e.Message);
            return;
        }

        AudioClip clip = WavUtility.ToAudioClip(wavBytes, "CoeiroInk");

        if (clip == null)
        {
            Debug.LogError("WAV decode failed");
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();

        // ★ 再生開始ログ
        Debug.Log($"CoeiroInk 音声再生開始（長さ: {clip.length:F2} 秒）");
    }
}
