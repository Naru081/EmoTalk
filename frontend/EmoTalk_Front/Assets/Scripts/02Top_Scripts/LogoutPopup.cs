using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// ログアウトによる認証情報破棄とログイン画面遷移
public class LogoutPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;   //　ポップアップの表示実態　 
    [SerializeField] private Button yesButton;  // 実行ボタン
    [SerializeField] private Button noButton;   // キャンセルボタン

    // ログイン画面
    [SerializeField] private string loginSceneName = "LoginScene";

    // ==============================
    // ログアウト確認画面表示
    // ==============================
    public void Open()
    {
        if (root == null) root = gameObject;

        // ボタンイベント初期化
        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        // 各ボタンに処理を割り当てる
        yesButton.onClick.AddListener(DoLogout);
        noButton.onClick.AddListener(Close);

        root.SetActive(true);
    }
    // ==============================
    // ログアウト確認画面非表示
    // ==============================
    public void Close()
    {
        if (root != null) root.SetActive(false);
    }

    // ==============================
    // ログアウト実行
    // ==============================
    private void DoLogout()
    {
        // 自動ログインのためのトークンを削除
        EncryptedPlayerPrefs.DeleteKey("token");

        // ログイン画面へ遷移して、アプリを再起動した状態にする
        SceneManager.LoadScene(loginSceneName);
    }
}