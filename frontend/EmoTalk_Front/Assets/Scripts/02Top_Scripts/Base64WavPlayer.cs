using UnityEngine;
using System;
using System.Collections;

public class Base64WavPlayer : MonoBehaviour
{
    // 互換用：未指定ならこれを使う
    public AudioSource defaultAudioSource;

    // 音声再生終了イベント
    public Action OnVoiceFinished;

    private void Awake()
    {
        if (defaultAudioSource == null)
        {
            defaultAudioSource = GetComponent<AudioSource>();
        }
    }

    // 再生先を指定可能
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

        StopAllCoroutines(); // 既存の再生待機を停止
        audioSource.clip = clip;
        audioSource.Play();

        Debug.Log($"CoeiroInk 音声再生開始: {clip.length:F2} 秒");

        // 再生終了を検知
        StartCoroutine(WaitVoiceEnd(audioSource));
    }

    // 音声再生終了待機コルーチン
    private IEnumerator WaitVoiceEnd(AudioSource source)
    {
        while (source != null && source.isPlaying)
        {
            yield return null;
        }

        Debug.Log("CoeiroInk 音声再生終了");
        OnVoiceFinished?.Invoke();
    }

    // 既存コード互換（呼び出し側を変えられない場合用）
    public void PlayFromBase64(string base64)
    {
        PlayFromBase64(base64, null);
    }
}