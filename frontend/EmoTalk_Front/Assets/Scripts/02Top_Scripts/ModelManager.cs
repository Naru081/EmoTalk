using UnityEngine;

// 画面上に表示するアバター（Live2D/3Dモデル）の実体化と管理を担うシングルトンクラス
public class ModelManager : MonoBehaviour
{
    // どこからでもモデル表示を切り替えられるようにするためのシングルトンインスタンス
    public static ModelManager Instance { get; private set; }

    public Transform modelRoot;         // モデルの親オブジェクト
    public GameObject[] modelPrefabs;   // 選択可能なモデルのプレハブ配列

    // indexごとの補正（Inspectorで調整）
    public Vector3[] modelOffsets;      // プレハブごとに異なる初期位置の補正値
    public Vector3[] modelScales;       // プレハブごとに異なる初期スケールの補正値

    private int currentIndex = -1;      // 現在表示中のモデルのインデックス
    private GameObject currentModel;    // 現在表示中のモデルのインスタンス

    // 外部（リップシンク制御など）から現在のモデルへアクセスするためのプロパティ
    public GameObject CurrentModel => currentModel;

    // ==============================
    // Singletonパターン
    // ==============================
    void Awake()
    {
        // 二重生成を防ぎ、モデルのインスタンスを保証する
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ==============================
    // モデル表示
    // ==============================
    void Start()
    {
        // 前回アプリ終了時に選択されていたモデルを読み込んで表示
        int savedIndex = PlayerPrefs.GetInt("SelectedModel", 0);
        ShowModel(savedIndex);
    }

    // ==============================
    // 選択したモデルを表示
    // ==============================
    public void ShowModel(int index)
    {
        // 指定されたインデックスのモデルを生成し、位置やスケールを補正する
        if (modelPrefabs == null || modelPrefabs.Length == 0) return;
        if (index < 0 || index >= modelPrefabs.Length) return;
        // 既に同じモデルが表示されているなら何もしない
        if (index == currentIndex && currentModel != null) return;

        // 既存のモデルを削除
        if (currentModel != null) Destroy(currentModel);

        // 新しいモデルを生成
        currentModel = Instantiate(modelPrefabs[index], modelRoot);
        currentModel.transform.localRotation = Quaternion.identity;

        // モデルごとの位置・スケール補正を適用
        var scale  = (modelScales  != null && index < modelScales.Length)  ? modelScales[index]  : Vector3.one;
        var offset = (modelOffsets != null && index < modelOffsets.Length) ? modelOffsets[index] : Vector3.zero;

        currentModel.transform.localScale = scale;
        currentModel.transform.localPosition = offset;

        // 現在のモデルインデックスを更新
        currentIndex = index;

        // 選択したモデルインデックスを保存
        PlayerPrefs.SetInt("SelectedModel", index);
        PlayerPrefs.Save();
    }
}