using UnityEngine;

// 警告文を表示を自動で閉じるポップアップ
public class DeleteDeniedPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;   //　ポップアップの表示実態
    [SerializeField] private float autoCloseSeconds = 3f; // 自動で閉じるまでの秒数

    // ==============================
    // 警告ポップアップ表示/非表示
    // ==============================
    //  選択中のアイテムが削除できない場合に表示
    public void Open()
    {
        if (root == null) root = gameObject;

        // 進行中のタイマーをリセット
        CancelInvoke();

        root.SetActive(true);

        // タイマースタート
        Invoke(nameof(Close), autoCloseSeconds);
    }

    // ==============================
    // 警告ポップアップ非表示
    // ==============================
    public void Close()
    {
        // タイマーを解除して、表示を消す
        CancelInvoke();

        if (root != null) root.SetActive(false);
    }
}