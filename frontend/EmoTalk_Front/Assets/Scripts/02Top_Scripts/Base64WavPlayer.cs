using UnityEngine;
using System;
using System.Collections;

// サーバーから受信したBase64エンコードされたWAV音声を再生するクラス
public class Base64WavPlayer : MonoBehaviour
{
    // 互換用：再生対象のAudioSourceが外部から指定されない場合のデフォルトオーディオソース
    public AudioSource defaultAudioSource;

    // 音声再生終了イベント
    public Action OnVoiceFinished;


    private void Awake()
    {
        // インスペクターで未指定の場合、自身にアタッチされたAudioSourceを使用
        if (defaultAudioSource == null)
        {
            defaultAudioSource = GetComponent<AudioSource>();
        }
    }

    // ==============================
    // Base64文字列からWAV音声をデコードして再生
    // ==============================
    // 指定したAudioSourceでBase64文字列を再生
    public void PlayFromBase64(string base64, AudioSource target)
    {
        // 再生対象のAudioSourceを決定
        AudioSource audioSource = target != null ? target : defaultAudioSource;

        // バリデーションチェック
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

        // Base64からバイナリへのデコード
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

        // WAVバイト列からAudioClipを生成
        // WavUtilityを介してUnityが認識できる形式に変換
        AudioClip clip = WavUtility.ToAudioClip(wavBytes, "CoeiroInk");
        if (clip == null)
        {
            Debug.LogError("WAV decode failed");
            return;
        }

        // 音声再生開始
        StopAllCoroutines(); // 既存の再生待機を停止
        audioSource.clip = clip;
        audioSource.Play();

        Debug.Log($"CoeiroInk 音声再生開始: {clip.length:F2} 秒");

        // 再生終了を検知
        StartCoroutine(WaitVoiceEnd(audioSource));
    }

    // ==============================
    // 音声再生終了待機コルーチン
    // ==============================
    private IEnumerator WaitVoiceEnd(AudioSource source)
    {
        // AudioSourceが再生中である限りフレーム待機
        while (source != null && source.isPlaying)
        {
            yield return null;
        }

        // 再生終了イベント
        Debug.Log("CoeiroInk 音声再生終了");
        OnVoiceFinished?.Invoke();
    }

    // ==============================
    // 便利メソッド
    // ==============================
    
    // 既存コード互換（呼び出し側を変えられない場合用）
    public void PlayFromBase64(string base64)
    {
        PlayFromBase64(base64, null);
    }
}