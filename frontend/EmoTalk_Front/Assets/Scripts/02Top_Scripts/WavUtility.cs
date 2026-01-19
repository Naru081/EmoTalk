using UnityEngine;
using System;
using System.IO;

public static class WavUtility
{
    // ==============================
    // wavバイト配列をAudioClipに変換（再生用）
    // ==============================
    public static AudioClip ToAudioClip(byte[] wavFile, string clipName = "wav")
    {
        int headerSize = 44;

        int channels = 1;
        int sampleRate = 16000;

        int sampleCount = (wavFile.Length - headerSize) / 2;
        float[] samples = new float[sampleCount];

        int offset = headerSize;
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(wavFile, offset);
            samples[i] = sample / 32768f;
            offset += 2;
        }

        AudioClip clip = AudioClip.Create(
            clipName,
            sampleCount / channels,
            channels,
            sampleRate,
            false
        );

        clip.SetData(samples, 0);
        return clip;
    }

    // ==============================
    // AudioClipをwavバイト配列に変換（WisperAPI送信用）
    // ==============================
    public static byte[] FromAudioClip(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        byte[] pcmBytes = new byte[samples.Length * 2];
        int offset = 0;

        foreach (float sample in samples)
        {
            short s = (short)Mathf.Clamp(sample * 32767f, -32768f, 32767f);
            byte[] bytes = BitConverter.GetBytes(s);
            pcmBytes[offset++] = bytes[0];
            pcmBytes[offset++] = bytes[1];
        }

        return AddWavHeader(
            pcmBytes,
            clip.channels,
            clip.frequency
        );
    }

    public static byte[] AudioClipToWav(AudioClip clip)
    {
        int sampleRate = clip.frequency;
        int channels = clip.channels;
        float[] samples = new float[clip.samples * channels];
        clip.GetData(samples, 0);

        byte[] pcm16 = new byte[samples.Length * 2];
        int offset = 0;

        foreach (var sample in samples)
        {
            short s = (short)Mathf.Clamp(sample * short.MaxValue, short.MinValue, short.MaxValue);
            pcm16[offset++] = (byte)(s & 0xff);
            pcm16[offset++] = (byte)((s >> 8) & 0xff);
        }

        return AddWavHeader(pcm16, sampleRate, channels);
    }

    private static byte[] AddWavHeader(byte[] pcmData, int sampleRate, int channels)
    {
        int fileSize = 44 + pcmData.Length;
        byte[] wav = new byte[fileSize];

        void WriteString(string s, int i)
        {
            for (int j = 0; j < s.Length; j++)
                wav[i + j] = (byte)s[j];
        }

        WriteString("RIFF", 0);
        BitConverter.GetBytes(fileSize - 8).CopyTo(wav, 4);
        WriteString("WAVE", 8);
        WriteString("fmt ", 12);
        BitConverter.GetBytes(16).CopyTo(wav, 16);
        BitConverter.GetBytes((short)1).CopyTo(wav, 20); // PCM
        BitConverter.GetBytes((short)channels).CopyTo(wav, 22);
        BitConverter.GetBytes(sampleRate).CopyTo(wav, 24);
        BitConverter.GetBytes(sampleRate * channels * 2).CopyTo(wav, 28);
        BitConverter.GetBytes((short)(channels * 2)).CopyTo(wav, 32);
        BitConverter.GetBytes((short)16).CopyTo(wav, 34);
        WriteString("data", 36);
        BitConverter.GetBytes(pcmData.Length).CopyTo(wav, 40);

        Buffer.BlockCopy(pcmData, 0, wav, 44, pcmData.Length);
        return wav;
    }
}
