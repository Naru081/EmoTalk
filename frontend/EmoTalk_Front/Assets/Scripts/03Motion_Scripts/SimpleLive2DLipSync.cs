using UnityEngine;
using Live2D.Cubism.Core;

// オーディオの音量に基づいてLive2Dの口を動かすリップシンク制御
public class SimpleLive2DLipSync : MonoBehaviour
{
    public AudioSource audioSource; // 音声再生元のソース
    public CubismModel model;       // 操作対象となるLive2Dモデル
    public string mouthParamId = "ParamMouthOpenY"; // 口を開閉を制御するパラメータのID

    [Header("LipSync Settings")]
    public int sampleSize = 1024;   // 音声データの解析サンプル数(２の累乗)
    public float gain = 30f;        // 口の空き具合倍率(入力音量に対する感度)
    public float smoothing = 0.2f;  // 動きの滑らかさ
    public float noiseGate = 0.01f; // 反応させない最小音量のしきい値

    private float[] samples;                // 音量データを一時的に格納する配列
    private CubismParameter mouthParam;     // 制御対象の口パラメータ
    private float currentValue;             // 現在の口の開き具合

    // ==============================
    // スクリプトの読み込み時に実行
    // ==============================
    void Awake()
    {
        // 必要なコンポーネントが未設定の場合は自身から取得
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (model == null) model = GetComponent<CubismModel>();

        // 指定されたサンプルサイズで配列を確保
        samples = new float[sampleSize];

        // モデルの全パラメータから口のIDと一致するものを検索し、参照を保持する
        foreach (var p in model.Parameters)
        {
            if (p.Id == mouthParamId)
            {
                mouthParam = p;
                break;
            }
        }
        // 対象パラメータが見つからない場合はエラーを表示して注意を促す
        if (mouthParam == null)
            Debug.LogError("ParamMouthOpenY が見つかりません");
    }

    // ==============================
    // アニメーションの適用後に値を更新する処理
    // ==============================
    void LateUpdate()
    {
        // 必要な情報が欠けている場合は、エラーを避けるため処理を中断
        if (mouthParam == null || audioSource == null) return;

        // 今フレームでも目標とする口の開き具合
        float target = 0f;

        // 音声再生中の場合のみ解析を実行
        if (audioSource.isPlaying)
        {
            // 音声波形データをバッファにコピー
            audioSource.GetOutputData(samples, 0);  // コピー先

            // 全サンプルの二乗和を計算(音量のエネルギーを計算)
            float sum = 0f;
            for (int i = 0; i < samples.Length; i++)
                sum += samples[i] * samples[i];

            // 実行値を算出
            float rms = Mathf.Sqrt(sum / samples.Length);

            // 設定されたしきい値以下の小さな音は、無音として扱う
            if (rms < noiseGate)
                rms = 0f;

            // 音量に倍率を掛け合わせ、0~1.0の範囲に収める
            target = Mathf.Clamp01(rms * gain);
        }

        // Mathf.Lerpを使用して、現在の値を目標値に近づける
        currentValue = Mathf.Lerp(currentValue, target, smoothing);

        // 最終的な計算結果をLive2Dモデルのパラメータに直接代入する
        mouthParam.Value = currentValue;
    }
}