using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
// API実装時に使う予定
// using UnityEngine.Networking;

public class LoginController : MonoBehaviour
{
    [Header("UI References")]
    public InputField emailInput;
    public InputField passwordInput;
    public Text loadingText; // ログイン中表示用
    public Text messageText; // エラーメッセージ表示用

    [Header("Loading Panel")]
    public GameObject loadingPanel; // ローディング表示用パネル

    // ログインボタンから呼ぶ
    public void OnLoginButtonClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        // 未入力チェック
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("メールアドレスとパスワードを入力してください");
            return;
        }

        SetLoding(true,"ログイン中です...");

        // コルーチンでAPI呼び出し
        StartCoroutine(LoginRequest(email, password));
    }

    private IEnumerator LoginRequest(string email, string password)
    {
        // APIができたら後で差し替え
        yield return new WaitForSeconds(0.5f);

        // ローディング解除
        SetLoding(false);

        // サーバーのレスポンスで分岐する
        // 後で差し替え
        bool loginSuccess = true;

        if(loginSuccess)
        {
            // メッセージ消してTOPへ
            ClearMessage();
            SceneManager.LoadScene("TopScene");
        }
        else
        {
            // 後で「誤入力」、「未登録」で分岐させる
            ShowError("ログインに失敗しました。");
        }
    }

    //ログイン処理メッセージ表示
    private void ShowInfo(string msg)
    {
        if(messageText == null)return;
        loadingText.color = new Color(0.2f, 0.6f, 1f); // 青色
        loadingText.text = msg;      
    }
    // エラーメッセージ表示
    private void ShowError(string msg)
    {
        if(messageText == null)return;
        messageText.color = new Color(1f, 0.2f, 0.2f); // 赤色
        messageText.text = msg;      
    }
    // メッセージ削除
    private void ClearMessage()
    {
        if(messageText == null)return;
        messageText.text = "";
    }

    // ローディング表示切替
    private void SetLoding(bool isLoading, string message = "")
    {
        if(loadingPanel != null)
        {
            loadingPanel.SetActive(isLoading);
        }
        if(isLoading)
        {
            ShowInfo(message);
        }
        else
        {
            if(loadingText != null) loadingText.text = "";
        }    
    }

    // API実装後解放
    /*
    [System.Serializable]
    public class LoginResponseData
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    public class LoginResponseData{
        public bool success;
        public int user_id;
        public string message;
        public string error_code;
    }
    */
}