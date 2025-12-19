using UnityEngine;

public class ModelManager : MonoBehaviour
{
    public static ModelManager Instance { get; private set; }

    public Transform modelRoot;
    public GameObject[] modelPrefabs;

    // indexごとの補正（Inspectorで調整）
    public Vector3[] modelOffsets;   // 例: 0=(0,0,0) 1=(0,-0.8,0) 2=(0.2,-1.1,0)
    public Vector3[] modelScales;    // 必要なら

    private int currentIndex = -1;
    private GameObject currentModel;

    // ==============================
    // Singletonパターン
    // ==============================
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ==============================
    // モデル表示
    // ==============================
    void Start()
    {
        int savedIndex = PlayerPrefs.GetInt("SelectedModel", 0);
        ShowModel(savedIndex);
    }

    // ==============================
    // 選択したモデルを表示
    // ==============================
    public void ShowModel(int index)
    {
        if (modelPrefabs == null || modelPrefabs.Length == 0) return;
        if (index < 0 || index >= modelPrefabs.Length) return;
        if (index == currentIndex && currentModel != null) return;

        if (currentModel != null) Destroy(currentModel);

        currentModel = Instantiate(modelPrefabs[index], modelRoot);
        currentModel.transform.localRotation = Quaternion.identity;

        var scale  = (modelScales  != null && index < modelScales.Length)  ? modelScales[index]  : Vector3.one;
        var offset = (modelOffsets != null && index < modelOffsets.Length) ? modelOffsets[index] : Vector3.zero;

        currentModel.transform.localScale = scale;
        currentModel.transform.localPosition = offset;

        currentIndex = index;

        PlayerPrefs.SetInt("SelectedModel", index);
        PlayerPrefs.Save();
    }
}