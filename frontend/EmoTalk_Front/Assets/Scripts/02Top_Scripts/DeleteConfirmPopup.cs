using UnityEngine;
using UnityEngine.UI;

public class DeleteConfirmPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Button okButton;
    [SerializeField] private Button cancelButton;

    private System.Action onConfirm;

    // ==============================
    // 削除確認画面を開く
    // ==============================
    public void Open(System.Action onConfirm)
    {
        if (root == null) root = gameObject;

        this.onConfirm = onConfirm;

        // 念のため毎回クリア
        okButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        okButton.onClick.AddListener(() =>
        {
            this.onConfirm?.Invoke();
            Close();
        });

        cancelButton.onClick.AddListener(Close);

        root.SetActive(true);
    }

    // ==============================
    // 削除確認画面をを閉じる
    // ==============================
    public void Close()
    {
        root.SetActive(false);
        onConfirm = null;
    }
}