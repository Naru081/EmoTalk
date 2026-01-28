using UnityEngine;
using System;

public class Base64WavPlayer : MonoBehaviour
{
    // 互換用：未指定ならこれを使う
    public AudioSource defaultAudioSource;

    private void Awake()
    {
        if (defaultAudioSource == null)
        {
            defaultAudioSource = GetComponent<AudioSource>();
        }
    }

    // ★再生先を指定できる版
    public void PlayFromBase64(string base64, AudioSource target)
    {
        AudioSource audioSource = target != null ? target : defaultAudioSource;

        if (audioSource == null)
        {
            Debug.LogError("AudioSourceが設定されていません");
            return;
        }

        if (string.IsNullOrEmpty(base64))
        {
            Debug.LogError("base64 文字列が空です");
            return;
        }

        byte[] wavBytes;
        try
        {
            wavBytes = Convert.FromBase64String(base64);
        }
        catch (Exception e)
        {
            Debug.LogError("Base64 デコードエラー: " + e.Message);
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

        Debug.Log($"CoeiroInk 音声再生開始: {clip.length:F2} 秒");
    }

    // 既存コード互換（呼び出し側をすぐ変えられない場合用）
    public void PlayFromBase64(string base64)
    {
        PlayFromBase64(base64, null);
    }
}