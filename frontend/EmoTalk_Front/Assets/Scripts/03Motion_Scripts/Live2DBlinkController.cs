using UnityEngine;
using Live2D.Cubism.Core;

public class Live2DBlinkController : MonoBehaviour
{
    private CubismModel model;
    private CubismParameter eyeLOpen;
    private CubismParameter eyeROpen;

    public float blinkSpeed = 8.0f;
    public float minBlinkInterval = 2.0f;
    public float maxBlinkInterval = 5.0f;

    private float nextBlinkTime = 0f;
    private float blinkTimer = 0f;
    private bool isBlinking = false;

    // たまに二回連続でまばたきさせて自然にする
    private bool doDoubleBlink = false;
    private int blinkCount = 0; 

    void Start()
    {
        model = GetComponent<CubismModel>();
        if (model == null) return;

        eyeLOpen = model.Parameters.FindById("ParamEyeLOpen");
        eyeROpen = model.Parameters.FindById("ParamEyeROpen");

        ScheduleNextBlink();
    }

    void Update()
    {
        if (eyeLOpen == null || eyeROpen == null) return;

        if (!isBlinking)
        {
            if (Time.time >= nextBlinkTime)
            {
                isBlinking = true;
                blinkTimer = 0f;
                blinkCount = 0;

                // 20% の確率でダブルブリンク
                doDoubleBlink = Random.value < 0.2f;
            }
        }
        else
        {
            blinkTimer += Time.deltaTime * blinkSpeed;

            float t = blinkTimer;

            // 0→1→0 のアニメーション
            float blinkValue = 1f - Mathf.Abs(t - 1f);

            // 反転モデル対応（あなたの元コードのまま）
            float inverted = 1f - blinkValue;

            eyeLOpen.Value = inverted;
            eyeROpen.Value = inverted;

            if (t >= 2f)
            {
                blinkCount++;
                blinkTimer = 0f;

                // ダブルブリンク処理
                if (doDoubleBlink && blinkCount < 2)
                {
                    // 2回目のまばたきを続ける
                    return;
                }

                // 通常終了
                isBlinking = false;
                ScheduleNextBlink();
            }
        }
    }

    void ScheduleNextBlink()
    {
        nextBlinkTime = Time.time + Random.Range(minBlinkInterval, maxBlinkInterval);
    }
}
