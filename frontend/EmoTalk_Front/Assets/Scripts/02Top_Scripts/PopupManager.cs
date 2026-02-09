using UnityEngine;

// ポップアップの表示・非表示を管理するクラス
public class PopupManager : MonoBehaviour
{
    [SerializeField] private GameObject root;               // ポップアップのルートオブジェクト
    [SerializeField] private bool autoClose = true;         // 自動で閉じるかどうか
    [SerializeField] private float autoCloseSeconds = 3f;   // 自動で閉じるまでの秒数

    // ==============================
    // 初期化処理
    // ==============================
    private void Awake()
    {
        // rootが未設定の場合は自分自身をルートに設定
        if (root == null) root = gameObject;
        root.SetActive(false);
    }

    // ==============================
    // ポップアップの表示
    // ==============================
    public void Open()
    {
        // 自動閉じる処理のキャンセル
        CancelInvoke();
        root.SetActive(true);

        // 自動で閉じる設定の場合は指定秒数後にCloseを呼び出す
        if (autoClose)
        {
            Invoke(nameof(Close), autoCloseSeconds);
        }
    }

    // ==============================
    // ポップアップの非表示
    // ==============================
    public void Close()
    {
        CancelInvoke();
        root.SetActive(false);
    }
}
