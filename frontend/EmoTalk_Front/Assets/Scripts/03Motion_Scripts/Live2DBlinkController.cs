using UnityEngine;
using Live2D.Cubism.Core;

// Live2Dモデルのまばたきを制御するスクリプト
public class Live2DBlinkController : MonoBehaviour
{
    // Live2Dモデルとパラメーター参照
    private CubismModel model;
    private CubismParameter eyeLOpen;
    private CubismParameter eyeROpen;

    // まばたきの設定
    public float blinkSpeed = 8.0f;
    public float minBlinkInterval = 2.0f;
    public float maxBlinkInterval = 5.0f;

    // まばたきのタイミング設定
    private float nextBlinkTime = 0f;
    private float blinkTimer = 0f;
    private bool isBlinking = false;

    // 2回連続まばたきをさせる
    private bool doDoubleBlink = false;
    private int blinkCount = 0; 

    // ==============================
    // 初期化
    // ==============================
    void Start()
    {
        // Live2Dモデルを取得
        model = GetComponent<CubismModel>();
        if (model == null) return;

        // 左右の目のパラメータを取得
        eyeLOpen = model.Parameters.FindById("ParamEyeLOpen");
        eyeROpen = model.Parameters.FindById("ParamEyeROpen");


        // 次のまばたきのタイミングを予約
        ScheduleNextBlink();
    }

    // ==============================
    // まばたき処理
    // ==============================
    void Update()
    {
        // パラメータがない場合は処理しない
        if (eyeLOpen == null || eyeROpen == null) return;

        // まばたき制御
        if (!isBlinking)
        {
            // 次のまばたきの時間に達した場合
            if (Time.time >= nextBlinkTime)
            {
                isBlinking = true;  // まばたきを開始
                blinkTimer = 0f;    // まばたきタイマーをリセット
                blinkCount = 0;     // まばたき回数をリセット

                // 20% の確率で2回連続まばたきをする
                doDoubleBlink = Random.value < 0.2f;
            }
        }
        else
        {
            blinkTimer += Time.deltaTime * blinkSpeed;

            float t = blinkTimer;

            // まばたきのアニメーション
            float blinkValue = 1f - Mathf.Abs(t - 1f);

            // 反転モデル対応
            // 0が閉じて１が開いている状態
            float inverted = 1f - blinkValue;

            // 目のパラメータを更新
            eyeLOpen.Value = inverted;
            eyeROpen.Value = inverted;

            // まばたき完了判定
            if (t >= 2f)
            {
                blinkCount++;
                blinkTimer = 0f;

                // ダブルリンク処理
                if (doDoubleBlink && blinkCount < 2)
                {
                    // 2回目のまばたき
                    return;
                }

                // 終了
                isBlinking = false;
                ScheduleNextBlink();
            }
        }
    }

    // ==============================
    // 次回のまばたきのタイミングを設定
    // ==============================
    void ScheduleNextBlink()
    {
        nextBlinkTime = Time.time + Random.Range(minBlinkInterval, maxBlinkInterval);
    }
}
