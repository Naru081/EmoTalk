using UnityEngine;
using Live2D.Cubism.Core;

public class SimpleLive2DLipSync : MonoBehaviour
{
    public AudioSource audioSource;
    public CubismModel model;

    public string mouthParamId = "ParamMouthOpenY";

    [Header("LipSync Settings")]
    public int sampleSize = 1024;
    public float gain = 30f;
    public float smoothing = 0.2f;
    public float noiseGate = 0.01f;

    private float[] samples;
    private CubismParameter mouthParam;
    private float currentValue;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (model == null) model = GetComponent<CubismModel>();

        samples = new float[sampleSize];

        foreach (var p in model.Parameters)
        {
            if (p.Id == mouthParamId)
            {
                mouthParam = p;
                break;
            }
        }

        if (mouthParam == null)
            Debug.LogError("ParamMouthOpenY が見つかりません");
    }

    void LateUpdate()
    {
        if (mouthParam == null || audioSource == null) return;

        float target = 0f;

        if (audioSource.isPlaying)
        {
            audioSource.GetOutputData(samples, 0);

            float sum = 0f;
            for (int i = 0; i < samples.Length; i++)
                sum += samples[i] * samples[i];

            float rms = Mathf.Sqrt(sum / samples.Length);

            if (rms < noiseGate)
                rms = 0f;

            target = Mathf.Clamp01(rms * gain);
        }

        currentValue = Mathf.Lerp(currentValue, target, smoothing);
        mouthParam.Value = currentValue;
    }
}