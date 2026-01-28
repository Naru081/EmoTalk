using UnityEngine;

public class MicRecorder : MonoBehaviour
{
    private AudioClip recording;
    private string device;
    private int sampleRate = 16000;

    // 録音開始
    public bool StartRecording()
    {
        // マイクデバイスの取得
        device = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        if (device == null)
        {
            Debug.LogError("マイクデバイスが見つかりません。");
            return false;
        }

        recording = Microphone.Start(device, false, 30, sampleRate);
        Debug.Log("録音開始");
        return true;
    }

    // 音量の取得
    public float GetCurrentVolume()
    {
        if (recording == null || device == null) return 0f;

        int micPosition = Microphone.GetPosition(device);
        if (micPosition <= 0) return 0f;

        int sampleSize = 256;
        float[] samples = new float[sampleSize];

        int startPosition = Mathf.Max(0, micPosition - sampleSize);
        recording.GetData(samples, startPosition);

        float sum = 0f;
        for (int i = 0; i < sampleSize; i++)
        {
            sum += samples[i] * samples[i];
        }

        return Mathf.Sqrt(sum / sampleSize); // RMS値を返す
    }

    // 録音停止
    public AudioClip StopRecording()
    {
        if (recording == null || device == null)
        {
            return null;
        }

        int position = Microphone.GetPosition(device);
        Microphone.End(device);

        if (position <= 0)
        {
            Debug.LogWarning("録音が正常に行われませんでした。");
            return null;
        }

        int originalChannels = recording.channels;
        float[] data = new float[position * originalChannels];
        recording.GetData(data, 0);

        // モノラル変換
        float[] monoData = new float[position];
        if (originalChannels == 2)
        {
            for (int i = 0; i < position; i++)
            {
                monoData[i] = (data[i * 2] + data[i * 2 + 1]) * 0.5f;
            }
        }
        else
        {
            monoData = data;
        }

        AudioClip clip = AudioClip.Create(
            "RecordedClip",
            position,
            1, // モノラル
            recording.frequency,
            false
        );
        Debug.Log($"clip.length={clip.length}, expected={position / (float)clip.frequency}");

        clip.SetData(monoData, 0);

        Debug.Log("録音終了");
        return clip;
    }

    // ==============================
    // ダミー音声生成（テスト用）
    // ==============================
    public AudioClip CreateDummyClip(float lengthSec = 1.2f)
    {
        int samples = Mathf.CeilToInt(sampleRate * lengthSec);
        float[] data = new float[samples]; // 完全な無音

        AudioClip clip = AudioClip.Create(
            "DummyClip",
            samples,
            1,
            sampleRate,
            false
        );
        clip.SetData(data, 0);
        return clip;
    }


}
