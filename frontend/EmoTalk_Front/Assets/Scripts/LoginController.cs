using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoginController : MonoBehaviour
{
    [Header("Login UI")]
    public InputField emailInput;
    public InputField passwordInput;

    public Text loadingText;      // ログイン中表示用（LoadingPanel 内）
    public Text messageText;      // ログイン画面のエラー表示用

    [Header("Loading Panel")]
    public GameObject loadingPanel; // ログイン処理中の全画面パネル

    [Header("Register Panel")]
    public GameObject registerPanel;            // 新規登録用パネル（背景＋window）
    public InputField registerEmailInput;       // 新規登録：メール
    public InputField registerPasswordInput;    // 新規登録：パスワード
    public Text registerMessageText;            // 新規登録：エラーメッセージ

    [Header("Register Complete Panel")]
    public GameObject registerCompletePanel;    // 登録完了パネル

    // ===== ログイン処理 =====

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

        ClearMessage();

        // ローディング表示
        SetLoading(true, "ログイン中です...");

        // コルーチンでAPI呼び出し（今はダミー）
        StartCoroutine(LoginRequest(email, password));
    }

    private IEnumerator LoginRequest(string email, string password)
    {
        // APIができたらここを差し替え
        yield return new WaitForSeconds(0.5f);

        // 処理終了 → ローディング非表示
        SetLoading(false);

        // ダミー結果（とりあえず成功）
        bool loginSuccess = true;

        if (loginSuccess)
        {
            ClearMessage();
            SceneManager.LoadScene("TopScene");
        }
        else
        {
            ShowError("ログインに失敗しました。");
        }
    }

    private void ShowInfo(string msg)
    {
        if (loadingText == null) return;
        loadingText.color = new Color(0.2f, 0.6f, 1f); // 青
        loadingText.text = msg;
    }

    private void ShowError(string msg)
    {
        if (messageText == null) return;
        messageText.color = new Color(1f, 0.2f, 0.2f); // 赤
        messageText.text = msg;
    }

    private void ClearMessage()
    {
        if (messageText == null) return;
        messageText.text = "";
    }

    private void SetLoading(bool isLoading, string message = "")
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(isLoading);

        if (isLoading)
        {
            ShowInfo(message);
        }
        else
        {
            if (loadingText != null) loadingText.text = "";
        }
    }

    // ===== 新規登録関連 =====

    // 「新規登録」ボタンから呼ぶ：入力ウィンドウを開く
    public void OnOpenRegisterPanel()
    {
        if (registerPanel == null) return;

        if (registerEmailInput != null) registerEmailInput.text = "";
        if (registerPasswordInput != null) registerPasswordInput.text = "";
        if (registerMessageText != null) registerMessageText.text = "";

        registerPanel.SetActive(true);
    }

    // 新規登録ウィンドウの「閉じる」ボタンから呼ぶ
    public void OnCloseRegisterPanel()
    {
        if (registerPanel == null) return;
        registerPanel.SetActive(false);
    }

    // 新規登録ウィンドウの「登録」ボタンから呼ぶ
    public void OnRegisterButtonClicked()
    {
        string email = registerEmailInput != null ? registerEmailInput.text.Trim() : "";
        string password = registerPasswordInput != null ? registerPasswordInput.text : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowRegisterError("メールアドレスとパスワードを入力してください");
            return;
        }

        if (registerMessageText != null) registerMessageText.text = "";

        // ここも今はダミーで実装
        StartCoroutine(RegisterRequest(email, password));
    }

    private IEnumerator RegisterRequest(string email, string password)
    {
        // 後で PHP の /auth/register とつなぐ予定
        yield return new WaitForSeconds(0.5f);

        bool registerSuccess = true; // ダミーで成功扱い

        if (registerSuccess)
        {
            // 入力パネルを閉じて、完了パネルを表示
            if (registerPanel != null) registerPanel.SetActive(false);

            if (registerCompletePanel != null)
                registerCompletePanel.SetActive(true);

            // ログイン画面のメール欄に、今登録したメールを入れておくと親切
            if (emailInput != null)
                emailInput.text = email;
        }
        else
        {
            ShowRegisterError("登録に失敗しました。");
        }
    }

    private void ShowRegisterError(string msg)
    {
        if (registerMessageText == null) return;
        registerMessageText.color = new Color(1f, 0.2f, 0.2f);
        registerMessageText.text = msg;
    }

    // 登録完了パネルの「閉じる」ボタンから呼ぶ
    public void OnCloseRegisterCompletePanel()
    {
        if (registerCompletePanel == null) return;
        registerCompletePanel.SetActive(false);
        // ログイン画面にそのまま戻る想定なのでシーン遷移はしない
    }

    /*
    // API実装後に使う予定のクラス（今はコメントアウトのままでOK）

    [System.Serializable]
    public class LoginRequestData
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    public class LoginResponseData
    {
        public bool success;
        public int user_id;
        public string message;
        public string error_code;
    }

    [System.Serializable]
    public class RegisterRequestData
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    public class RegisterResponseData
    {
        public bool success;
        public int user_id;
        public string message;
        public string error_code;
    }
    */
}