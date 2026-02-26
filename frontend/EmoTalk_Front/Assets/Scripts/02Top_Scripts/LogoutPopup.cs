using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

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

   private void DoLogout()
    {
        int userId = UserData.GetUserId();

        LogoutRequest request = new LogoutRequest
        {
            user_id = userId
        };

        StartCoroutine(ApiConnect.Post<LogoutRequest, BaseResponseData>(
            "PHP_user/logout.php",
            request,
            (res) =>
            {
                if (res.success)
                {
                    // --- 追加：不要な DontDestroyOnLoad オブジェクトを破棄 ---
                    if (ProfileManager.Instance != null)
                    {
                        Destroy(ProfileManager.Instance.gameObject);
                    }

                    // 端末からトークンを削除
                    EncryptedPlayerPrefs.DeleteKey("token");

                    // ログイン画面へ遷移
                    SceneManager.LoadScene(loginSceneName);
                }
            },
            (err) =>
            {
                Debug.LogError("Logout API Error: " + err);
            }
        ));
    }
}