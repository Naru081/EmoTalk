using UnityEngine;

public class DeleteDeniedPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private float autoCloseSeconds = 3f;

    // ==============================
    // 警告ポップアップ表示/非表示
    // ==============================
    public void Open()
    {
        if (root == null) root = gameObject;

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
        CancelInvoke();

        if (root != null) root.SetActive(false);
    }
}