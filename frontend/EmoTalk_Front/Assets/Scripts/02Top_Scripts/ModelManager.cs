using UnityEngine;

public class ModelManager : MonoBehaviour
{
    public static ModelManager Instance { get; private set; }

    [Header("modelRoot")]
    public Transform modelRoot;          // TOP画面のモデル表示位置用のEmpty

    [Header("modelPrefabs（0:Model1, 1:Model2, 2:Model3）")]
    public GameObject[] modelPrefabs;    // vtemモデル + Cubismサンプル2体

    private int currentIndex = -1;
    private GameObject currentModel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 起動時に前回選択していたモデルを復元
        int savedIndex = PlayerPrefs.GetInt("SelectedModel", 0);
        ShowModel(savedIndex);
    }

    public void ShowModel(int index)
    {
        if (modelPrefabs == null || modelPrefabs.Length == 0)
        {
            Debug.LogWarning("ModelManager: modelPrefabs が設定されていません。");
            return;
        }
        if (index < 0 || index >= modelPrefabs.Length)
        {
            Debug.LogWarning("ModelManager: index が範囲外です: " + index);
            return;
        }
        if (index == currentIndex && currentModel != null)
        {
            // 同じモデルなら何もしない
            return;
        }

        // 旧モデルを削除
        if (currentModel != null)
        {
            Destroy(currentModel);
            currentModel = null;
        }

        // 新しいモデルを生成
        GameObject prefab = modelPrefabs[index];
        currentModel = Instantiate(prefab, modelRoot);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localScale    = Vector3.one;

        currentIndex = index;

        // 選択状態を保存（次回起動時に復元）
        PlayerPrefs.SetInt("SelectedModel", index);
        PlayerPrefs.Save();
    }
}