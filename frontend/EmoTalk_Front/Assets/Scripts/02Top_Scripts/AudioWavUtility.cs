using System;
using System.IO;
using UnityEngine;

public static class AudioWavUtility
{
    /// <summary>
    /// AudioClip を wav(byte[]) に変換
    /// </summary>
    public static byte[] FromAudioClip(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip is null");
            return null;
        }

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        byte[] wavData = ConvertSamplesToWav(samples, clip.channels, clip.frequency);
        return wavData;
    }

    private static byte[] ConvertSamplesToWav(float[] samples, int channels, int sampleRate)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            int byteRate = sampleRate * channels * 2;
            int dataSize = samples.Length * 2;

            // RIFFヘッダ
            WriteString(stream, "RIFF");
            WriteInt(stream, 36 + dataSize);
            WriteString(stream, "WAVE");

            // fmtチャンク
            WriteString(stream, "fmt ");
            WriteInt(stream, 16);
            WriteShort(stream, 1); // PCM
            WriteShort(stream, (short)channels);
            WriteInt(stream, sampleRate);
            WriteInt(stream, byteRate);
            WriteShort(stream, (short)(channels * 2));
            WriteShort(stream, 16);

            // dataチャンク
            WriteString(stream, "data");
            WriteInt(stream, dataSize);

            foreach (float sample in samples)
            {
                short intSample = (short)Mathf.Clamp(sample * 32767f, short.MinValue, short.MaxValue);
                WriteShort(stream, intSample);
            }

                return stream.ToArray();
        }
    }

    private static void WriteString(Stream stream, string value)
    {
        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void WriteInt(Stream stream, int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void WriteShort(Stream stream, short value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }
}