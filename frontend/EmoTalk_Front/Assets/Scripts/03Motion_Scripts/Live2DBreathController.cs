using UnityEngine;
using Live2D.Cubism.Core;

public class Live2DBreathController : MonoBehaviour
{
    private CubismModel model;
    private CubismParameter breathParam;
    private CubismParameter headZParam;

    private const string BreathParam = "ParamBreath";
    private const string HeadZParam = "ParamAngleZ";

    public float breathSpeed = 1.2f;
    public float breathAmplitude = 0.5f;

    public float headZRange = 5f;

    private bool isDestroyed = false;

    void Start()
    {
        model = GetComponent<CubismModel>();
        if (model == null) return;

        breathParam = model.Parameters.FindById(BreathParam);
        headZParam = model.Parameters.FindById(HeadZParam);
    }

    void Update()
    {
        if (isDestroyed) return;
        if (breathParam == null) return;

        float t = Time.time * breathSpeed;
        float breathValue = (Mathf.Sin(t) * 0.5f + 0.5f) * breathAmplitude;

        breathParam.Value = breathValue;

        if (headZParam != null)
        {
            float normalized = Mathf.InverseLerp(0f, breathAmplitude, breathValue);
            float centered = normalized * 2f - 1f;
            float angle = centered * headZRange;

            headZParam.Value = angle;
        }
    }

    void OnDestroy()
    {
        isDestroyed = true;
        model = null;
        breathParam = null;
        headZParam = null;
    }
}
