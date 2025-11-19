using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoginController : MonoBehaviour
{
    // ログイン画面
    [Header("Login UI (Canvas root)")]
    public InputField loginUserMail;      // Canvas/user_mail
    public InputField loginUserPass;      // Canvas/user_pass
    public Text loginErrorText;           // Canvas/error_text

    [Header("Loading Panel")]
    public GameObject loadingPanel;       // Canvas/loadingPanel
    public Text loadingText;              // loadingPanel/loading_text


    // 新規登録
    [Header("Register Panel")]
    public GameObject registerPanel;          // Canvas/RegisterPanel
    public InputField registerNewMail;        // RegisterPanel/Window/new_mail
    public InputField registerNewPass;        // RegisterPanel/Window/new_pass
    public Text registerErrorText;            // RegisterPanel/Window/error_text

    public GameObject registerCompletePanel;  // Canvas/RegisterComletePanel


    // パスワード再設定：メール送信 
    [Header("Reset Mail Panel")]
    public GameObject resetMailPanel;         // Canvas/ResetMailPanel
    public InputField resetMailUserMail;      // ResetMailPanel/Window/user_mail
    public Text resetMailErrorText;           // ResetMailPanel/Window/error_text


    // パスワード再設定：ワンタイムキー 
    [Header("Reset Key Panel")]
    public GameObject resetKeyPanel;          // Canvas/ResetKeyPanel
    public InputField resetKeyInput;          // ResetKeyPanel/Window/key_in
    public Text resetKeyErrorText;            // ResetKeyPanel/Window/error_text


    // パスワード再設定：新パスワード入力 
    [Header("Reset Password Panel")]
    public GameObject resetPasswordPanel;     // Canvas/ResetPasswordPanel
    public InputField resetNewPass;           // ResetPasswordPanel/Window/new_pass
    public InputField resetNewPass2;          // ResetPasswordPanel/Window/new_pass2
    public Text resetPasswordErrorText;       // ResetPasswordPanel/Window/error_text


    // パスワード再設定：完了
    [Header("Reset Complete Panel")]
    public GameObject resetCompletePanel;     // Canvas/ResetCompletePanel



    // =====================================================================
    //  ログイン処理
    // =====================================================================
    public void OnLoginButtonClicked()
    {
        string email = loginUserMail.text.Trim();
        string password = loginUserPass.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowLoginError("メールアドレスとパスワードを入力してください");
            return;
        }

        ClearLoginError();
        SetLoading(true, "ログイン中です...");

        StartCoroutine(LoginRequest(email, password));
    }

    private IEnumerator LoginRequest(string email, string password)
    {
        // TODO: 後で API 呼び出しに差し替え
        yield return new WaitForSeconds(0.5f);

        SetLoading(false);

        bool loginSuccess = true; // ダミー

        if (loginSuccess)
        {
            ClearLoginError();
            SceneManager.LoadScene("TopScene");
        }
        else
        {
            ShowLoginError("ログインに失敗しました。");
        }
    }

    private void ShowLoginError(string msg)
    {
        if (loginErrorText == null) return;
        loginErrorText.color = new Color(1f, 0.2f, 0.2f);
        loginErrorText.text = msg;
    }

    private void ClearLoginError()
    {
        if (loginErrorText == null) return;
        loginErrorText.text = "";
    }

    private void SetLoading(bool isLoading, string message = "")
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(isLoading);

        if (isLoading)
        {
            if (loadingText != null)
            {
                loadingText.color = new Color(0.2f, 0.6f, 1f);
                loadingText.text = message;
            }
        }
        else
        {
            if (loadingText != null) loadingText.text = "";
        }
    }


    // =====================================================================
    //  新規登録
    // =====================================================================

    // 「新規登録」ボタン（Canvas/new_user）から呼ぶ
    public void OnOpenRegisterPanel()
    {
        if (registerPanel == null) return;

        if (registerNewMail != null) registerNewMail.text = "";
        if (registerNewPass != null) registerNewPass.text = "";
        if (registerErrorText != null) registerErrorText.text = "";

        registerPanel.SetActive(true);
    }

    public void OnCloseRegisterPanel()
    {
        if (registerPanel == null) return;
        registerPanel.SetActive(false);
    }

    // RegisterPanel/Window/entry ボタンから呼ぶ
    public void OnRegisterButtonClicked()
    {
        string email = registerNewMail != null ? registerNewMail.text.Trim() : "";
        string password = registerNewPass != null ? registerNewPass.text : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowRegisterError("メールアドレスとパスワードを入力してください");
            return;
        }

        if (registerErrorText != null) registerErrorText.text = "";

        StartCoroutine(RegisterRequest(email, password));
    }

    private IEnumerator RegisterRequest(string email, string password)
    {
        // TODO: 後で API に接続
        yield return new WaitForSeconds(0.5f);

        bool registerSuccess = true; // ダミー

        if (registerSuccess)
        {
            if (registerPanel != null) registerPanel.SetActive(false);
            if (registerCompletePanel != null) registerCompletePanel.SetActive(true);

            // ログイン画面のメール欄に反映
            if (loginUserMail != null) loginUserMail.text = email;
        }
        else
        {
            ShowRegisterError("登録に失敗しました。");
        }
    }

    private void ShowRegisterError(string msg)
    {
        if (registerErrorText == null) return;
        registerErrorText.color = new Color(1f, 0.2f, 0.2f);
        registerErrorText.text = msg;
    }

    // RegisterComletePanel/Window/close ボタンから呼ぶ
    public void OnCloseRegisterCompletePanel()
    {
        if (registerCompletePanel == null) return;
        registerCompletePanel.SetActive(false);
    }


    // =====================================================================
    //  パスワード再設定フロー
    // =====================================================================

    // 「パスワードを忘れた場合はこちら」（Canvas/loss_pass）から呼ぶ
    public void OnOpenResetMailPanel()
    {
        ClearAllResetErrors();

        if (resetMailUserMail != null) resetMailUserMail.text = "";

        if (resetMailPanel != null) resetMailPanel.SetActive(true);
        if (resetKeyPanel != null) resetKeyPanel.SetActive(false);
        if (resetPasswordPanel != null) resetPasswordPanel.SetActive(false);
        if (resetCompletePanel != null) resetCompletePanel.SetActive(false);
    }

    // 各「閉じる」ボタンから呼ぶ（メール/キー/パスワード）
    public void OnCloseResetPanels()
    {
        if (resetMailPanel != null) resetMailPanel.SetActive(false);
        if (resetKeyPanel != null) resetKeyPanel.SetActive(false);
        if (resetPasswordPanel != null) resetPasswordPanel.SetActive(false);
        if (resetCompletePanel != null) resetCompletePanel.SetActive(false);

        ClearAllResetErrors();
    }

    // ResetMailPanel/Window/send_mail から呼ぶ
    public void OnSendResetMailButtonClicked()
    {
        string mail = resetMailUserMail != null ? resetMailUserMail.text.Trim() : "";

        if (string.IsNullOrEmpty(mail))
        {
            ShowResetMailError("メールアドレスを入力してください");
            return;
        }

        ClearAllResetErrors();
        StartCoroutine(SendResetMailRequest(mail));
    }

    private IEnumerator SendResetMailRequest(string mail)
    {
        // TODO: 後でメール送信APIと接続
        yield return new WaitForSeconds(0.5f);

        bool mailExists = true; // ダミーで成功

        if (mailExists)
        {
            if (resetMailPanel != null) resetMailPanel.SetActive(false);
            if (resetKeyPanel != null) resetKeyPanel.SetActive(true);

            if (resetKeyInput != null) resetKeyInput.text = "";
        }
        else
        {
            ShowResetMailError("このメールアドレスは登録されていません。");
        }
    }

    private void ShowResetMailError(string msg)
    {
        if (resetMailErrorText == null) return;
        resetMailErrorText.color = new Color(1f, 0.2f, 0.2f);
        resetMailErrorText.text = msg;
    }

    // ResetKeyPanel/Window/send_key から呼ぶ
    public void OnSendResetKeyButtonClicked()
    {
        string key = resetKeyInput != null ? resetKeyInput.text.Trim() : "";

        if (string.IsNullOrEmpty(key))
        {
            ShowResetKeyError("ワンタイムキーを入力してください");
            return;
        }

        ClearAllResetErrors();
        StartCoroutine(CheckResetKeyRequest(key));
    }

    private IEnumerator CheckResetKeyRequest(string key)
    {
        // TODO: 後でキー検証APIと接続
        yield return new WaitForSeconds(0.5f);

        bool keyValid = true; // ダミー成功

        if (keyValid)
        {
            if (resetKeyPanel != null) resetKeyPanel.SetActive(false);
            if (resetPasswordPanel != null) resetPasswordPanel.SetActive(true);

            if (resetNewPass != null) resetNewPass.text = "";
            if (resetNewPass2 != null) resetNewPass2.text = "";
        }
        else
        {
            ShowResetKeyError("ワンタイムキーが正しくありません。");
        }
    }

    private void ShowResetKeyError(string msg)
    {
        if (resetKeyErrorText == null) return;
        resetKeyErrorText.color = new Color(1f, 0.2f, 0.2f);
        resetKeyErrorText.text = msg;
    }

    // ResetPasswordPanel/Window/re_entry ボタンから呼ぶ
    public void OnResetPasswordButtonClicked()
    {
        string newPass = resetNewPass != null ? resetNewPass.text : "";
        string confirm = resetNewPass2 != null ? resetNewPass2.text : "";

        if (string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(confirm))
        {
            ShowResetPasswordError("新しいパスワードと確認用を入力してください");
            return;
        }

        if (newPass != confirm)
        {
            ShowResetPasswordError("確認用パスワードが一致しません");
            return;
        }

        ClearAllResetErrors();
        StartCoroutine(ResetPasswordRequest(newPass));
    }

    private IEnumerator ResetPasswordRequest(string newPass)
    {
        // TODO: 後でパスワード再設定APIと接続
        yield return new WaitForSeconds(0.5f);

        bool resetSuccess = true; // ダミー成功

        if (resetSuccess)
        {
            if (resetPasswordPanel != null) resetPasswordPanel.SetActive(false);
            if (resetCompletePanel != null) resetCompletePanel.SetActive(true);
        }
        else
        {
            ShowResetPasswordError("パスワードの再設定に失敗しました。");
        }
    }

    private void ShowResetPasswordError(string msg)
    {
        if (resetPasswordErrorText == null) return;
        resetPasswordErrorText.color = new Color(1f, 0.2f, 0.2f);
        resetPasswordErrorText.text = msg;
    }

    // ResetCompletePanel/Window/close ボタンから呼ぶ
    public void OnCloseResetCompletePanel()
    {
        if (resetCompletePanel != null)
            resetCompletePanel.SetActive(false);
    }

    private void ClearAllResetErrors()
    {
        if (resetMailErrorText != null) resetMailErrorText.text = "";
        if (resetKeyErrorText != null) resetKeyErrorText.text = "";
        if (resetPasswordErrorText != null) resetPasswordErrorText.text = "";
    }

    /*
    // 後でAPIに繋ぐとき用のクラス（今はコメントアウトのままでOK）

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

    [System.Serializable] 
    public class ResetMailRequestData 
    { 
        public string email; 
    }
    [System.Serializable] 
    public class ResetKeyRequestData 
    { 
        public string email;
        public string key;
    }
    [System.Serializable] 
    public class ResetPasswordRequestData 
    { 
        public string email; 
        public string newPassword;
    }
    */
}