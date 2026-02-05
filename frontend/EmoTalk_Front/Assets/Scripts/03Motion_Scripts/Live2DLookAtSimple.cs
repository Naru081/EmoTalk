using UnityEngine;
using Live2D.Cubism.Core;

public class Live2DLookAtSimple : MonoBehaviour
{
    private CubismModel model;

    private CubismParameter angleX;
    private CubismParameter angleY;
    private CubismParameter bodyAngleX;

    public float faceStrength = 30f;
    public float bodyStrength = 10f;

    public float faceSmooth = 10f;
    public float bodySmooth = 5f;

    private Vector2 target = Vector2.zero;

    void Start()
    {
        model = GetComponent<CubismModel>();

        angleX = model.Parameters.FindById("ParamAngleX");
        angleY = model.Parameters.FindById("ParamAngleY");
        bodyAngleX = model.Parameters.FindById("ParamBodyAngleX");
    }

    void Update()
    {
        HandleInput();

        angleX.Value = Mathf.Lerp(angleX.Value, target.x * faceStrength, Time.deltaTime * faceSmooth);
        angleY.Value = Mathf.Lerp(angleY.Value, target.y * faceStrength, Time.deltaTime * faceSmooth);
        bodyAngleX.Value = Mathf.Lerp(bodyAngleX.Value, target.x * bodyStrength, Time.deltaTime * bodySmooth);
    }

    void HandleInput()
    {
        // -------------------------
        // スマホ（タッチ）
        // -------------------------
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            Vector2 pos = NormalizeScreenPos(t.position);
            target = pos;
            return;
        }

        // -------------------------
        // PC（マウス）
        // -------------------------
        if (Input.GetMouseButton(0))
        {
            Vector2 pos = NormalizeScreenPos(Input.mousePosition);
            target = pos;
            return;
        }

        // -------------------------
        // 何も押してない → 正面へ戻す
        // -------------------------
        target = Vector2.Lerp(target, Vector2.zero, Time.deltaTime * 5f);
    }

    // 画面座標 → -1から1 に正規化
    Vector2 NormalizeScreenPos(Vector2 screenPos)
    {
        Vector2 pos;
        pos.x = (screenPos.x / Screen.width) * 2f - 1f;
        pos.y = (screenPos.y / Screen.height) * 2f - 1f;

        return Vector2.ClampMagnitude(pos, 1f);
    }
}
