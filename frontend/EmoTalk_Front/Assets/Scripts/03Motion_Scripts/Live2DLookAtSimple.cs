using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Live2D.Cubism.Core;

// Live2Dモデルの視線追従を行うスクリプト
public class Live2DLookAtSimple : MonoBehaviour
{
    // Live2Dのモデルとパラメータの参照
    private CubismModel model;
    private CubismParameter angleX;     // 顔の向きX (左右)
    private CubismParameter angleY;     // 顔の向きY (上下)
    private CubismParameter bodyAngleX; // 体の向きX (左右)

    // 視線の追従設定(最大回転角度)
    public float faceStrength = 30f;    // 顔の動く強さ
    public float bodyStrength = 10f;    // 体の動く強さ

    // スムース設定(追従の滑らかさ)
    public float faceSmooth = 10f;
    public float bodySmooth = 5f;

    // 一定範囲内で視線の向きを保持する目標値
    private Vector2 target = Vector2.zero;

    [Header("UI Block")]
    [SerializeField] private string blockTag = "Emo_UI";
    [SerializeField] private float idleReturnSpeed = 5f;

    // ==============================
    // 初期化
    // ==============================
    void Start()
    {
        model = GetComponent<CubismModel>();

        angleX = model.Parameters.FindById("ParamAngleX");
        angleY = model.Parameters.FindById("ParamAngleY");
        bodyAngleX = model.Parameters.FindById("ParamBodyAngleX");
    }

    // ==============================
    // 毎フレーム視線追従を更新
    // ==============================
    void Update()
    {
        // Emo_UIタグ上の操作なら追従しない
        if (IsPointerOnTaggedUI(blockTag))
        {
            target = Vector2.Lerp(target, Vector2.zero, Time.deltaTime * idleReturnSpeed);
        }
        else
        {
            HandleInput();
        }

        angleX.Value = Mathf.Lerp(angleX.Value, target.x * faceStrength, Time.deltaTime * faceSmooth);
        angleY.Value = Mathf.Lerp(angleY.Value, target.y * faceStrength, Time.deltaTime * faceSmooth);
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
            Touch t = Input.GetTouch(0);
            Vector2 pos = NormalizeScreenPos(t.position);
            target = pos;
            return;
        }

        // マウス処理(PC)
        if (Input.GetMouseButton(0))
        {
            Vector2 pos = NormalizeScreenPos(Input.mousePosition);
            target = pos;
            return;
        }

        // 何も押していないとき
        target = Vector2.Lerp(target, Vector2.zero, Time.deltaTime * idleReturnSpeed);
    }

    // ==============================
    // UIレイキャストして、指定タグのUI上か判定
    // ==============================
    bool IsPointerOnTaggedUI(string tagName)
    {
        if (EventSystem.current == null) return false;

        Vector2 screenPos;
        bool pressed;

#if UNITY_EDITOR || UNITY_STANDALONE
        pressed = Input.GetMouseButton(0);
        screenPos = Input.mousePosition;
#else
        pressed = Input.touchCount > 0;
        screenPos = pressed ? Input.GetTouch(0).position : Vector2.zero;
#endif

        if (!pressed) return false;

        PointerEventData ped = new PointerEventData(EventSystem.current);
        ped.position = screenPos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        for (int i = 0; i < results.Count; i++)
        {
            GameObject hit = results[i].gameObject;
            if (hit != null && hit.CompareTag(tagName))
            {
                return true;
            }
        }

        return false;
    }

    // ==============================
    // スクリーン座標を [-1.0~1.0] の範囲に正規化
    // ==============================
    Vector2 NormalizeScreenPos(Vector2 screenPos)
    {
        Vector2 pos;
        pos.x = (screenPos.x / Screen.width) * 2f - 1f;
        pos.y = (screenPos.y / Screen.height) * 2f - 1f;

        return Vector2.ClampMagnitude(pos, 1f);
    }
}