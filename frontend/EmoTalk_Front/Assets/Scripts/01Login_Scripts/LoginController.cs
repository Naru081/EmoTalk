using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;

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
    public GameObject resetCompletePanel;     // Canvas/ResetCompletePanel

    // =====================================================================
    //  自動ログイン処理
    // =====================================================================

    // 自動ログイン機能
    void Start()
    {
        //#if UNITY_EDITOR
        //    Debug.Log("エディタでは自動ログインをスキップ");
        //    return;
        //#endif

        string token = EncryptedPlayerPrefs.LoadString("token", "");
        if (!string.IsNullOrEmpty(token))
        {
            Debug.Log("起動時にトークンが見つかりました: " + token);
            // サーバーに照合リクエストを送る
            StartCoroutine(CheckToken(token));
        }
    }

    // トークン照合コルーチン
    IEnumerator CheckToken(string token)
    {
        // リクエストデータの準備
        TokenRequest request = new TokenRequest
        {
            token = token
        };
        // 通信を呼び出す トークン照合処理
        yield return StartCoroutine(ApiConnect.Post<TokenRequest, TokenResponse>(
            "PHP_user/auto_login.php",
            request,
            (res) =>
            {
                // 結果の処理
                if (res.success)
                {
                    // トークンが有効ならログイン情報を取得・保存してTOP画面へ遷移
                    UserData.SaveUserId(res.user_id);
                    UserData.SaveUserMail(registerNewMail.text);
                    UserData.SaveUserCurrentProfId(res.user_currentprof);

                    // ロード画面表示
                    SceneManager.LoadScene("TopScene");
                }
                else
                {
                    // トークンが無効なら削除
                    EncryptedPlayerPrefs.DeleteKey("token");
                }
            },
            (error) =>
            {
                Debug.LogError("トークン照合エラー: " + error);
            }
        ));
    }

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
        //yield return new WaitForSeconds(0.5f);

        // リクエストデータの準備
        UserRequest request = new UserRequest
        {
            user_mail = email,
            user_pass = password
        };

        // 通信を呼び出す ログイン認証処理
        StartCoroutine(ApiConnect.Post<UserRequest, LoginResponse>(
        "PHP_user/login.php",
        request,
        (res) =>
        {
            // 結果の処理
            if (res.success)
            {
                // 取得したuser_idとuser_mailとtokenをPlayerPrefsに格納
                UserData.SaveUserId(res.user_id);
                UserData.SaveUserMail(registerNewMail.text);
                UserData.SaveUserCurrentProfId(res.user_currentprof);;

                // 自動ログイン用のトークンを暗号化保存
                EncryptedPlayerPrefs.SaveString("token", res.token);

                // ログイン成功時、ロード画面表示
                SetLoading(true, "ログインに成功しました");

                // 1秒待ってTOP画面へ行く処理を行う関数を呼び出す
                StartCoroutine(LoginSuccessLoading());
            }
            else
            {
                // エラー表示
                ShowLoginError(res.message);
            }
        },
        (error) => { ShowLoginError(error); }
        ));
    }

    private IEnumerator LoginSuccessLoading()
    {
        // ロード画面を1秒間表示する
        yield return new WaitForSeconds(1f);

        // その後非表示にする
        loadingPanel.SetActive(false);

        // シーン移動(TOP画面へ)
        SceneManager.LoadScene("TopScene");
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

    private void SetLoading(bool isLoading, string message)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(isLoading);
        }

        if (loadingText != null)
        {
            if (isLoading)
            {
                loadingText.color = new Color(0.2f, 0.6f, 1f, 1f);
                loadingText.text = message;
            }
            else
            {
                loadingText.text = "";
            }
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

        // リクエストデータの準備
        UserRequest request = new UserRequest
        {
            user_mail = email,
            user_pass = password
        };

        // 通信を呼び出す ユーザ新規登録処理
        StartCoroutine(ApiConnect.Post<UserRequest, RegisterResponse>(
            "PHP_user/register.php",
            request,
            (res) =>
            {
                // 結果の処理
                if (res.success)
                {
                    // UserDataに登録情報を保存
                    UserData.SaveUserId(res.user_id);
                    UserData.SaveUserMail(registerNewMail.text);
                    UserData.SaveUserCurrentProfId(res.user_currentprof);
                    // 成功画面へ
                    if (registerPanel != null) registerPanel.SetActive(false);
                    if (registerCompletePanel != null) registerCompletePanel.SetActive(true);
                }
                else
                {
                    // エラー表示
                    ShowRegisterError(res.message);
                }
            },
            (error) => { ShowRegisterError(error); }
        ));
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
    //  パスワード再設定
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
    // メール入力と確認
    public void OnSendResetMailButtonClicked()
    {
        string email = resetMailUserMail != null ? resetMailUserMail.text.Trim() : "";

        // メールアドレス未入力チェック
        if (string.IsNullOrEmpty(email))
        {
            ShowResetMailError("メールアドレスを入力してください");
            return;
        }

        // エラーメッセージクリア
        ClearAllResetErrors();

        // リクエストデータの準備
        User_mailRequest request = new User_mailRequest
        {
            user_mail = email
        };

        // 通信を呼び出す ワンタイムキーを発行しユーザのメールアドレスに送信
        StartCoroutine(ApiConnect.Post<User_mailRequest, BaseResponseData>(
            "PHP_user/otk_create.php",
            request,
            (res) =>
            {
                // 結果の処理
                if (res.success)
                {
                    // 成功時、ワンタイムキー入力パネルへ
                    if (resetMailPanel != null) resetMailPanel.SetActive(false);
                    if (resetKeyPanel != null) resetKeyPanel.SetActive(true);
                    if (resetKeyInput != null) resetKeyInput.text = "";
                }
                else
                {
                    // エラー表示
                    ShowResetMailError(res.message);
                }
            },
            (error) => { ShowResetMailError(error); }
        ));
    }

    // メール送信エラー表示
    private void ShowResetMailError(string msg)
    {
        if (resetMailErrorText == null) return;
        resetMailErrorText.color = new Color(1f, 0.2f, 0.2f);
        resetMailErrorText.text = msg;
    }

    // ResetKeyPanel/Window/send_key から呼ぶ
    // ワンタイムキー入力後の確認
    public void OnSendResetKeyButtonClicked()
    {
        string otk = resetKeyInput != null ? resetKeyInput.text.Trim() : "";
        string mail = resetMailUserMail != null ? resetMailUserMail.text.Trim() : "";


        if (string.IsNullOrEmpty(otk))
        {
            ShowResetKeyError("ワンタイムキーを入力してください");
            return;
        }

        ClearAllResetErrors();
        // リクエストデータの準備
        OtkAuthRequest request = new OtkAuthRequest
        {
            user_mail = mail,
            otk = otk
        };

        // 通信を呼び出す ワンタイムキーの認証
        StartCoroutine(ApiConnect.Post<OtkAuthRequest, BaseResponseData>(
            "PHP_user/otk_auth.php",
            request,
            (res) =>
            {
                // 結果の処理
                if (res.success)
                {
                    // 成功時、新パスワード入力パネルへ
                    if (resetKeyPanel != null) resetKeyPanel.SetActive(false);
                    if (resetPasswordPanel != null) resetPasswordPanel.SetActive(true);

                    if (resetNewPass != null) resetNewPass.text = "";
                    if (resetNewPass2 != null) resetNewPass2.text = "";
                }
                else
                {
                    // エラー表示
                    ShowResetKeyError(res.message);
                }
            },
            (error) => { ShowResetKeyError(error); }
        ));
    }

    // ワンタイムキーエラー表示
    private void ShowResetKeyError(string msg)
    {
        if (resetKeyErrorText == null) return;
        resetKeyErrorText.color = new Color(1f, 0.2f, 0.2f);
        resetKeyErrorText.text = msg;
    }

    // ResetPasswordPanel/Window/re_entry ボタンから呼ぶ
    // パスワード再設定
    public void OnResetPasswordButtonClicked()
    {
        string mail = resetMailUserMail != null ? resetMailUserMail.text.Trim() : "";
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

        // リクエストデータの準備
        ResetPassRequest request = new ResetPassRequest
        {
            user_mail = mail,
            newpassword = newPass
        };

        // 通信を呼び出す パスワード再設定処理
        StartCoroutine(ApiConnect.Post<ResetPassRequest, BaseResponseData>(
            "PHP_user/reset_pass.php",
            request,
            (res) =>
            {
                // 結果の処理
                if (res.success)
                {
                    // 成功時、完了パネルへ
                    if (resetPasswordPanel != null) resetPasswordPanel.SetActive(false);
                    if (resetCompletePanel != null) resetCompletePanel.SetActive(true);
                }
                else
                {
                    // エラー表示
                    ShowResetPasswordError(res.message);
                }
            },
            (error) => { ShowResetPasswordError(error); }
        ));
    }

    // パスワード再設定エラー表示
    private void ShowResetPasswordError(string msg)
    {
        if (resetPasswordErrorText == null) return;
        resetPasswordErrorText.color = new Color(1f, 0.2f, 0.2f);
        resetPasswordErrorText.text = msg;
    }

    // ResetCompletePanel/Window/close ボタンから呼ぶ
    public void OnCloseResetCompletePanel() // 完了パネルを閉じる
    {
        if (resetCompletePanel != null)
            resetCompletePanel.SetActive(false);
    }

    // すべてのパスワード再設定エラーメッセージをクリア
    private void ClearAllResetErrors()
    {
        if (resetMailErrorText != null) resetMailErrorText.text = "";
        if (resetKeyErrorText != null) resetKeyErrorText.text = "";
        if (resetPasswordErrorText != null) resetPasswordErrorText.text = "";
    }
}