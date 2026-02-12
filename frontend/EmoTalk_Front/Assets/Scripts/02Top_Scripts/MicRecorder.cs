using UnityEngine;
using System.Collections;

// マイク録音とデータ整形を行うクラス
public class MicRecorder : MonoBehaviour
{
    private AudioClip recording;         // 録音バッファ
    private string device;               // 使用デバイス名
    private int sampleRate = 44100;      // iOSで安定するサンプリングレート

    // 録音開始処理
    public bool StartRecording()
    {
        // 使用可能なマイクデバイスを取得
        device = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        if (device == null)
        {
            Debug.LogError("マイクデバイスが見つかりません");
            return false;
        }

        // 録音バッファを確保して録音を開始
        recording = Microphone.Start(device, false, 30, sampleRate);
        Debug.Log("録音開始");
        return true;
    }

    // 現在の音量を取得する処理
    public float GetCurrentVolume()
    {
        if (recording == null || device == null)
        {
            return 0f;
        }

        // 現在の録音位置を取得
        int micPosition = Microphone.GetPosition(device);
        if (micPosition <= 0)
        {
            return 0f;
        }

        // 直近256サンプルを取得して音量を計算
        int sampleSize = 256;
        float[] samples = new float[sampleSize];
        int startPosition = Mathf.Max(0, micPosition - sampleSize);
        recording.GetData(samples, startPosition);

        // RMS値を計算
        float sum = 0f;
        for (int i = 0; i < sampleSize; i++)
        {
            sum += samples[i] * samples[i];
        }

        return Mathf.Sqrt(sum / sampleSize);
    }

    // 録音停止とデータ整形処理
    public IEnumerator StopRecordingAsync(System.Action<AudioClip> callback)
    {
        if (recording == null || device == null)
        {
            callback(null);
            yield break;
        }

        // iOSは停止直後にデータが反映されないため待機
        yield return new WaitForSeconds(0.1f);

        // 録音位置を取得して録音を終了
        int position = Microphone.GetPosition(device);
        Microphone.End(device);

        // 録音データが無い場合は終了
        if (position <= 0)
        {
            Debug.LogWarning("録音データが取得できませんでした");
            callback(null);
            yield break;
        }

        // 録音データを取得
        int channels = recording.channels;
        float[] data = new float[position * channels];
        recording.GetData(data, 0);

        // モノラル化処理
        float[] mono = new float[position];
        if (channels == 2)
        {
            for (int i = 0; i < position; i++)
            {
                mono[i] = (data[i * 2] + data[i * 2 + 1]) * 0.5f;
            }
        }
        else
        {
            mono = data;
        }

        // モノラルデータをAudioClipとして生成
        AudioClip clip = AudioClip.Create(
            "RecordedClip",
            position,
            1,
            sampleRate,
            false
        );
        clip.SetData(mono, 0);

        Debug.Log("録音終了");
        callback(clip);
    }

    // ダミー音声クリップ生成処理
    public AudioClip CreateDummyClip(float lengthSec = 1.2f)
    {
        int samples = Mathf.CeilToInt(sampleRate * lengthSec);
        float[] data = new float[samples];

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
