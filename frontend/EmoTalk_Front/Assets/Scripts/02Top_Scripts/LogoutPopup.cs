using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogoutPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    // ログイン画面
    [SerializeField] private string loginSceneName = "LoginScene";

    // ==============================
    // ログアウト確認画面表示
    // ==============================
    public void Open()
    {
        if (root == null) root = gameObject;

        // 毎回クリーンに
        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

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

        // 今回は「仮ログアウト」なのでシーン遷移のみ
        SceneManager.LoadScene(loginSceneName);
    }
}