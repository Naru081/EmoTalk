using UnityEngine;
using System;
using System.IO;

// WAV(PCM16)とUnityのAudioClip間の変換ユーティリティ
public static class WavUtility
{
    // ==============================
    // wavバイト配列 → AudioClip に変換（再生用）
    // ==============================
    public static AudioClip ToAudioClip(byte[] wavFile, string clipName = "wav")
    {
        int headerSize = 44;    // PCM16 WAVヘッダーサイズ

        int channels = 1;       // モノラル固定
        int sampleRate = 16000; // 16kHz固定

        int sampleCount = (wavFile.Length - headerSize) / 2;    // 2バイト/サンプル
        float[] samples = new float[sampleCount];               // PCM16をfloatに変換

        // PCM16データをfloat配列に変換
        int offset = headerSize;
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(wavFile, offset);
            samples[i] = sample / 32768f;
            offset += 2;
        }

        // AudioClipを作成
        AudioClip clip = AudioClip.Create(
            clipName,
            sampleCount / channels,
            channels,
            sampleRate,
            false
        );

        // データをセット
        clip.SetData(samples, 0);
        return clip;
    }

    // ==============================
    // AudioClip → wavバイト配列に変換（Whisper API送信用）
    // ==============================
    public static byte[] FromAudioClip(AudioClip clip)
    {
        // PCM16に変換
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // PCM16へ変換するため 2バイト/サンプルの配列を確保
        byte[] pcmBytes = new byte[samples.Length * 2];
        int offset = 0;

        // float配列をPCM16バイト配列に変換
        foreach (float sample in samples)
        {
            short s = (short)Mathf.Clamp(sample * 32767f, -32768f, 32767f);
            byte[] bytes = BitConverter.GetBytes(s);
            pcmBytes[offset++] = bytes[0];
            pcmBytes[offset++] = bytes[1];
        }

        // WAVヘッダーを追加して返す
        return AddWavHeader(
            pcmBytes,
            clip.channels,
            clip.frequency
        );
    }

    // ==============================
    // AudioClip → WAVバイト配列に変換（Whisper API送信用）
    // ==============================
    public static byte[] AudioClipToWav(AudioClip clip)
    {
        int sampleRate = clip.frequency;    // サンプリングレート
        int channels = clip.channels;       // チャンネル数
        float[] samples = new float[clip.samples * channels];   // サンプルデータ取得
        clip.GetData(samples, 0);                               // AudioClipからサンプルデータを取得

        // PCM16に変換
        byte[] pcm16 = new byte[samples.Length * 2];
        int offset = 0;

        // float配列をPCM16バイト配列に変換
        foreach (var sample in samples)
        {
            short s = (short)Mathf.Clamp(sample * short.MaxValue, short.MinValue, short.MaxValue);
            pcm16[offset++] = (byte)(s & 0xff);
            pcm16[offset++] = (byte)((s >> 8) & 0xff);
        }

        // WAVヘッダーを追加して返す
        return AddWavHeader(pcm16, sampleRate, channels);
    }

    // ==============================
    // WAVヘッダーを追加
    // ==============================
    private static byte[] AddWavHeader(byte[] pcmData, int sampleRate, int channels)
    {
        int fileSize = 44 + pcmData.Length; // ヘッダー44バイト + PCMデータサイズ
        byte[] wav = new byte[fileSize];    // WAVファイル全体のバイト配列

        // ヘッダーを書き込むヘルパー関数
        void WriteString(string s, int i)
        {
            for (int j = 0; j < s.Length; j++)
                wav[i + j] = (byte)s[j];
        }

        WriteString("RIFF", 0);                                             // ChunkID
        BitConverter.GetBytes(fileSize - 8).CopyTo(wav, 4);                 // ChunkSize
        WriteString("WAVE", 8);                                             // Format
        WriteString("fmt ", 12);                                            // Subchunk1ID
        BitConverter.GetBytes(16).CopyTo(wav, 16);                          // Subchunk1Size
        BitConverter.GetBytes((short)1).CopyTo(wav, 20);                    // PCM
        BitConverter.GetBytes((short)channels).CopyTo(wav, 22);             // NumChannels
        BitConverter.GetBytes(sampleRate).CopyTo(wav, 24);                  // SampleRate
        BitConverter.GetBytes(sampleRate * channels * 2).CopyTo(wav, 28);   // ByteRate
        BitConverter.GetBytes((short)(channels * 2)).CopyTo(wav, 32);       // BlockAlign
        BitConverter.GetBytes((short)16).CopyTo(wav, 34);                   // BitsPerSample
        WriteString("data", 36);                                            // Subchunk2ID
        BitConverter.GetBytes(pcmData.Length).CopyTo(wav, 40);              // Subchunk2Size

        // PCMデータをコピー
        Buffer.BlockCopy(pcmData, 0, wav, 44, pcmData.Length);
        return wav;
    }
}