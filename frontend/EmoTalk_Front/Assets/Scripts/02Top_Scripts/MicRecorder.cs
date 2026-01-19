using UnityEngine;

public class MicRecorder : MonoBehaviour
{
    private AudioClip recording;
    private string device;
    private int sampleRate = 16000;

    // 録音開始
    public void StartRecording()
    {
        // マイクデバイスの取得
        device = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        if (device == null)
        {
            Debug.LogError("マイクが見つかりません");
            return;
        }

        recording = Microphone.Start(device, false, 30, sampleRate);
        Debug.Log("録音開始");
    }

    // 録音終了
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
            Debug.LogWarning("録音が正常に行われませんでした");
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
}
