using UnityEngine;

// Unityのマイク機能を使用して音声を録音し、解析に適した形式（モノラル）に整えて出力するクラス
public class MicRecorder : MonoBehaviour
{
    private AudioClip recording;    // Microphone.Start で確保される生の録音バッファ
    private string device;          // 使用するマイクデバイス名
    private int sampleRate = 16000; // サンプリングレート（Whisper等の推奨値 16kHz に設定）

    // ==============================
    // 録音開始
    // ==============================
    public bool StartRecording()
    {
        // 接続されているマイクデバイスを確認
        device = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        if (device == null)
        {
            Debug.LogError("マイクデバイスが見つかりません。");
            return false;
        }

        // 録音バッファの確保（最大30秒、ループなし）
        recording = Microphone.Start(device, false, 30, sampleRate);
        Debug.Log("録音開始");
        return true;
    }

    // ==============================
    // 音量の取得
    // ==============================
    public float GetCurrentVolume()
    {
        // MicController が「無音かどうか」を判定するために、直近の波形データの平均振幅を計算する
        if (recording == null || device == null) return 0f;

        // 録音位置を取得
        int micPosition = Microphone.GetPosition(device);
        if (micPosition <= 0) return 0f;

        // // 256を解析対象とする
        int sampleSize = 256;
        float[] samples = new float[sampleSize];

        // 録音位置から遡ってデータを取得
        int startPosition = Mathf.Max(0, micPosition - sampleSize);
        recording.GetData(samples, startPosition);

        // RMSを計算して音量を算出
        float sum = 0f;
        for (int i = 0; i < sampleSize; i++)
        {
            sum += samples[i] * samples[i];
        }

        return Mathf.Sqrt(sum / sampleSize); // RMS値を返す
    }

    // ==============================
    // 録音停止
    // ==============================
    public AudioClip StopRecording()
    {
        // 録音を終了し、実際に喋った長さ分だけを切り出したモノラル AudioClip を生成する
        if (recording == null || device == null)
        {
            return null;
        }

        // 録音位置を取得して録音停止
        int position = Microphone.GetPosition(device);
        Microphone.End(device);

        if (position <= 0)
        {
            Debug.LogWarning("録音が正常に行われませんでした。");
            return null;
        }

        // 録音データを取得
        int originalChannels = recording.channels;
        float[] data = new float[position * originalChannels];
        recording.GetData(data, 0);

        // モノラル変換
        // Whisperなどの音声認識APIはモノラルを好むため、ステレオの場合は平均化する
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

        // 切り出したデータで新しいAudioClipを作成
        AudioClip clip = AudioClip.Create(
            "RecordedClip",
            position,
            1, // モノラル
            recording.frequency,
            false
        );
        Debug.Log($"clip.length={clip.length}, expected={position / (float)clip.frequency}");

        // 整形したデータをセット
        clip.SetData(monoData, 0);

        Debug.Log("録音終了");
        return clip;
    }

    // ==============================
    // ダミー音声生成（テスト用）
    // ==============================
    public AudioClip CreateDummyClip(float lengthSec = 1.2f)
    {
        // 指定秒数の無音AudioClipを生成
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
