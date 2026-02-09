using UnityEngine;
using UnityEngine.UI;

// プロファイル削除確認のポップアップ
public class DeleteConfirmPopup : MonoBehaviour
{
    [SerializeField] private GameObject root; // 親オブジェクト
    [SerializeField] private Button okButton;   // 実行ボタン
    [SerializeField] private Button cancelButton; // キャンセルボタン

    // 実行時のコールバック
    private System.Action onConfirm;

    // ==============================
    // 削除確認画面を開く
    // ==============================
    public void Open(System.Action onConfirm)
    {
        if (root == null) root = gameObject;

        this.onConfirm = onConfirm;

        // ボタンのクリックイベントをリセットして重複登録を防止
        okButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        // 処理後、閉じる
        okButton.onClick.AddListener(() =>
        {
            this.onConfirm?.Invoke();
            Close();
        });
        
        // 何もせず閉じる
        cancelButton.onClick.AddListener(Close);

        root.SetActive(true);
    }

    // ==============================
    // 削除確認画面をを閉じる
    // ==============================
    public void Close()
    {
        root.SetActive(false);
        onConfirm = null;   // メモリリーク防止のためコールバックをクリア
    }
}