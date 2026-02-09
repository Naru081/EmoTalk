using UnityEngine;
using Live2D.Cubism.Core;

// Live2Dモデルの視線追従を行うスクリプト
public class Live2DLookAtSimple : MonoBehaviour
{
    // Live2Dのモデルとパラメータの参照
    private CubismModel model;
    private CubismParameter angleX;     // 顔の向きX (左右)
    private CubismParameter angleY;     //　顔の向きY (上下)
    private CubismParameter bodyAngleX; //　体の向きX (左右)

    // 視線の追従設定(最大回転角度)
    public float faceStrength = 30f;    // 顔の動く強さ
    public float bodyStrength = 10f;    // 体の動く強さ

    // スムース設定(追従の滑らかさ)
    public float faceSmooth = 10f;
    public float bodySmooth = 5f;

    // 一定範囲内で視線の向きを保持する目標値
    private Vector2 target = Vector2.zero;

    // ==============================
    // 初期化
    // ==============================
    void Start()
    {
        // 自身のオブジェクトから"CubismModel"を取得
        model = GetComponent<CubismModel>();
        
        //IDを指定して制御対象のパラメータをキャッシュしておく
        angleX = model.Parameters.FindById("ParamAngleX");
        angleY = model.Parameters.FindById("ParamAngleY");
        bodyAngleX = model.Parameters.FindById("ParamBodyAngleX");
    }

    // ==============================
    // 毎フレーム視線追従を更新
    // ==============================
    void Update()
    {
        // マウスやタッチによる入力座標の計算
        HandleInput();

        // パラメータの更新
        // 目標値 = target.x(現在の値) * faceStrength
        angleX.Value = Mathf.Lerp(angleX.Value, target.x * faceStrength, Time.deltaTime * faceSmooth);
        angleY.Value = Mathf.Lerp(angleY.Value, target.y * faceStrength, Time.deltaTime * faceSmooth);

        // 体も向きも同時に更新
        bodyAngleX.Value = Mathf.Lerp(bodyAngleX.Value, target.x * bodyStrength, Time.deltaTime * bodySmooth);
    }

    // ==============================
    // スマホとPCで画面をタップもしくはクリックした場所のポジションを取得
    // ==============================
    void HandleInput()
    {
        // タッチ処理(スマホ)
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);                    // 最初の指のタッチ情報を取得
            Vector2 pos = NormalizeScreenPos(t.position);   // スクリーン座標を正規化座標に変換
            target = pos;
            return;
        }

        // マウス処理(PC)
        if (Input.GetMouseButton(0))    // 左クリック中
        {
            Vector2 pos = NormalizeScreenPos(Input.mousePosition);  // マウスのクリーン座標を正規化座標に変換
            target = pos;
            return;
        }

        // 何も押していないとき
        target = Vector2.Lerp(target, Vector2.zero, Time.deltaTime * 5f);
    }

    // ==============================
    // スクリーン座標を [-1.0~1.0] の範囲に正規化
    Vector2 NormalizeScreenPos(Vector2 screenPos)   // スクリーン上の位置(x,y)
    {
        Vector2 pos;
        // 0~1の範囲にしてから２倍して1引くことで　 -1~1 に変換
        pos.x = (screenPos.x / Screen.width) * 2f - 1f;
        pos.y = (screenPos.y / Screen.height) * 2f - 1f;

        // 斜め方向などで 1.0 を超えないようにベクトルの長さを最大 1.0 に制限
        return Vector2.ClampMagnitude(pos, 1f);
    }
}
