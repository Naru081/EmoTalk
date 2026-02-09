using UnityEngine;
using Live2D.Cubism.Core;

// Live2Dモデルの呼吸モーションとそれに合わせた顔の傾きを制御するスクリプト
public class Live2DBreathController : MonoBehaviour
{
    // Live2Dモデルとパラメータの参照
    private CubismModel model;
    private CubismParameter breathParam;
    private CubismParameter headZParam;

    private const string BreathParam = "ParamBreath";   // 呼吸パラメータID
    private const string HeadZParam = "ParamAngleZ";    // 頭のパラメータID

    // 呼吸モーションの設定
    public float breathSpeed = 1.2f;
    public float breathAmplitude = 0.5f;

    // 頭の傾きの設定
    public float headZRange = 5f;

    // オブジェクトが破棄されたかどうかのフラグ
    private bool isDestroyed = false;

    // ==============================
    // 初期化
    // ==============================
    void Start()
    {
        // Live2Dモデルを取得
        model = GetComponent<CubismModel>();
        if (model == null) return;

        // 呼吸と頭の傾きのパラメータを取得
        breathParam = model.Parameters.FindById(BreathParam);
        headZParam = model.Parameters.FindById(HeadZParam);
    }
    // ==============================
    // 毎フレーム呼吸と頭の傾きを更新
    // ==============================

    void Update()
    {
        // パラメータがない場合は処理しない
        if (isDestroyed) return;
        if (breathParam == null) return;

        // 時間に基づいて呼吸の値を計算
        float t = Time.time * breathSpeed;  // 時間に呼吸速度を掛けた値
        float breathValue = (Mathf.Sin(t) * 0.5f + 0.5f) * breathAmplitude; // 正弦値を基に呼吸値を計算

        // 呼吸パラメータに値を設定
        breathParam.Value = breathValue;

        // 頭の傾きを呼吸に合わせて更新
        if (headZParam != null)
        {
            // 呼吸値に基づいて頭の傾きを計算
            float normalized = Mathf.InverseLerp(0f, breathAmplitude, breathValue);
            float centered = normalized * 2f - 1f;
            float angle = centered * headZRange;

            headZParam.Value = angle;
        }
    }

    // ==============================
    // オブジェクトが破棄されたときに呼び出される
    // ==============================
    void OnDestroy()
    {
        isDestroyed = true;
        model = null;
        breathParam = null;
        headZParam = null;
    }
}
