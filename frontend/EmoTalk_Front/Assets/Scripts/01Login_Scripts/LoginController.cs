using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;

public class LoginController : MonoBehaviour
{
    // ログイン画面
    [Header("Login UI (Canvas root)")]
    public InputField loginUserMail; // Canvas/user_mail
    public InputField loginUserPass; // Canvas/user_pass
    public Text loginErrorText; // Canvas/error_text

    // ローディングパネル
    [Header("Loading Panel")]
    public GameObject loadingPanel; // Canvas/loadingPanel
    public Text loadingText; // loadingPanel/loading_text


    // 新規登録パネル
    [Header("Register Panel")]
    public GameObject registerPanel; // Canvas/RegisterPanel
    public InputField registerNewMail; // RegisterPanel/Window/new_mail
    public InputField registerNewPass; // RegisterPanel/Window/new_pass
    public Text registerErrorText; // RegisterPanel/Window/error_text

     public GameObject registerCompletePanel; // Canvas/RegisterComletePanel


    // パスワード再設定：メール送信パネル
    [Header("Reset Mail Panel")]
    public GameObject resetMailPanel; // Canvas/ResetMailPanel
    public InputField resetMailUserMail; // ResetMailPanel/Window/user_mail
    public Text resetMailErrorText; // ResetMailPanel/Window/error_text


    // パスワード再設定：ワンタイムキーパネル
    [Header("Reset Key Panel")]
    public GameObject resetKeyPanel; // Canvas/ResetKeyPanel
    public InputField resetKeyInput; // ResetKeyPanel/Window/key_in
    public Text resetKeyErrorText; // ResetKeyPanel/Window/error_text


    // パスワード再設定：新パスワード入力パネル
    [Header("Reset Password Panel")]
    public GameObject resetPasswordPanel; // Canvas/ResetPasswordPanel
    public InputField resetNewPass; // ResetPasswordPanel/Window/new_pass
    public InputField resetNewPass2; // ResetPasswordPanel/Window/new_pass2
    public Text resetPasswordErrorText; // ResetPasswordPanel/Window/error_text


    // パスワード再設定：完了パネル
    [Header("Reset Complete Panel")]
    public GameObject resetCompletePanel;     // Canvas/ResetCompletePanel

    // =====================================================================
    //  自動ログイン処理
    // =====================================================================

    // 自動ログイン機能
    void Start()
    {
        // デバイスに保存されているトークンを取得
        string token = EncryptedPlayerPrefs.LoadString("token", "");

        // トークンが存在する場合、自動ログインの照合を行うコルーチンを開始
        if (!string.IsNullOrEmpty(token))
        {
            Debug.Log("起動時にトークンが見つかりました: " + token);
            // サーバーに照合リクエストを送る
            StartCoroutine(CheckToken(token));
        }
    }

    // トークン照合処理を行うコルーチン
    IEnumerator CheckToken(string token)
    {
        // リクエストデータの準備
        TokenRequest request = new TokenRequest
        {
            token = token
        };

        // トークン照合処理を行うPHPとの通信を開始
        yield return StartCoroutine(ApiConnect.Post<TokenRequest, TokenResponse>(
            "PHP_user/auto_login.php",
            request,
            (res) =>
            {
                // 結果の処理
                if (res.success)
                {
                    // トークンが有効ならログイン情報を取得しデバイスに保存
                    UserData.SaveUserId(res.user_id);
                    UserData.SaveUserMail(registerNewMail.text);
                    UserData.SaveUserCurrentProfId(res.user_currentprof);

                    SEManager.Instance?.PlayLoginSuccess(); // ログイン成功時SE再生

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
                // 通信エラー時の処理
                Debug.LogError("トークン照合通信エラー: " + error);
            }
        ));
    }

    // =====================================================================
    //  ログイン処理
    // =====================================================================

    // ログインボタンを押したときの処理
    public void OnLoginButtonClicked()
    {
        // ユーザが入力したメールアドレスとパスワードを取得
        string email = loginUserMail.text.Trim();
        string password = loginUserPass.text;

        // 入力チェック
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowLoginError("メールアドレスとパスワードを入力してください");
            return;
        }
        ClearLoginError();  // エラーメッセージクリア

        // リクエストデータの準備
        UserRequest request = new UserRequest
        {
            user_mail = email,
            user_pass = password
        };

        // ログイン認証処理を行うPHPとの通信を開始
        StartCoroutine(ApiConnect.Post<UserRequest, LoginResponse>(
        "PHP_user/login.php",
        request,
        (res) =>
        {
            // 結果の処理
            if (res.success)
            {
                // ログイン成功時の処理

                // 取得したuser_idとuser_mailとtokenをデバイスに保存
                UserData.SaveUserId(res.user_id);
                UserData.SaveUserMail(registerNewMail.text);
                UserData.SaveUserCurrentProfId(res.user_currentprof); ;

                // 自動ログイン用のトークンを暗号化してから暗号化用PlayerPrefsに保存
                EncryptedPlayerPrefs.SaveString("token", res.token);

                // ログイン成功時、成功SEを再生しロード画面表示
                SEManager.Instance?.PlayLoginSuccess();
                SetLoading(true, "ログインに成功しました");

                // TOP画面へ遷移する処理を開始
                StartCoroutine(LoginSuccessLoading());
            }
            else
            {
                // ログイン失敗時はエラーを表示
                ShowLoginError(res.message);
            }
        },
        (error) => { ShowLoginError(error); }
        ));
    }

    // TOP画面遷移処理
    private IEnumerator LoginSuccessLoading()
    {
        // ロード画面を1秒間表示する
        yield return new WaitForSeconds(1f);

        // その後非表示にする
        loadingPanel.SetActive(false);

        // シーン移動(TOP画面へ)
        SceneManager.LoadScene("TopScene");
    }

    // ログインエラーを表示する関数
    private void ShowLoginError(string msg)
    {
        if (loginErrorText == null) return;
        loginErrorText.color = new Color(1f, 0.2f, 0.2f);
        loginErrorText.text = msg;
    }

    // ログインエラーメッセージをクリアする関数
    private void ClearLoginError()
    {
        if (loginErrorText == null) return;
        loginErrorText.text = "";
    }

    // ローディングパネルの表示・非表示を切り替える関数
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

    // 「新規登録」ボタン（Canvas/new_user）を押したときの処理
    public void OnOpenRegisterPanel()
    {
        if (registerPanel == null) return;

        if (registerNewMail != null) registerNewMail.text = "";
        if (registerNewPass != null) registerNewPass.text = "";
        if (registerErrorText != null) registerErrorText.text = "";

        // 新規登録パネルを表示
        registerPanel.SetActive(true);
    }

    // 「新規登録」パネルの閉じるボタンを押したときの処理
    public void OnCloseRegisterPanel()
    {
        if (registerPanel == null) return;

        // 新規登録パネルを非表示
        registerPanel.SetActive(false);
    }

    // 「新規登録」パネルの「登録」ボタンを押したときの処理
    public void OnRegisterButtonClicked()
    {
        // ユーザが入力したメールアドレスとパスワードを取得
        string email = registerNewMail != null ? registerNewMail.text.Trim() : "";
        string password = registerNewPass != null ? registerNewPass.text : "";

        // 入力チェック
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowRegisterError("メールアドレスとパスワードを入力してください");
            return;
        }

        // エラーメッセージクリア
        if (registerErrorText != null) registerErrorText.text = "";

        // リクエストデータの準備
        UserRequest request = new UserRequest
        {
            user_mail = email,
            user_pass = password
        };

        // ユーザ新規登録処理を行うPHPとの通信を開始
        StartCoroutine(ApiConnect.Post<UserRequest, RegisterResponse>(
            "PHP_user/register.php",
            request,
            (res) =>
            {
                // 結果の処理
                if (res.success)
                {
                    // 新規登録成功時の処理

                    // UserDataに登録情報をデバイスに保存
                    UserData.SaveUserId(res.user_id);
                    UserData.SaveUserMail(registerNewMail.text);
                    UserData.SaveUserCurrentProfId(res.user_currentprof);

                    // TOP画面のハンバーガーメニュー内で選択中のプロファイルを強調表示させる
                    ProfileManager.Instance?.SyncSelectedProfileFromUserData();

                    // 新規登録パネルを非表示にし、成功画面を表示
                    if (registerPanel != null) registerPanel.SetActive(false);
                    if (registerCompletePanel != null) registerCompletePanel.SetActive(true);
                }
                else
                {
                    // 新規登録失敗時はエラー表示
                    ShowRegisterError(res.message);
                }
            },
            (error) => { ShowRegisterError(error); }
        ));
    }

    // 新規登録エラー表示
    private void ShowRegisterError(string msg)
    {
        if (registerErrorText == null) return;
        registerErrorText.color = new Color(1f, 0.2f, 0.2f);
        registerErrorText.text = msg;
    }

    // 「新規登録成功」パネルの閉じるボタンを押したときの処理
    public void OnCloseRegisterCompletePanel()
    {
        if (registerCompletePanel == null) return;

        // 新規登録成功パネルを非表示
        registerCompletePanel.SetActive(false);
    }

    // =====================================================================
    //  パスワード再設定
    // =====================================================================

    // 「パスワードを忘れた場合はこちら」（Canvas/loss_pass）ボタンから呼ぶ
    public void OnOpenResetMailPanel()
    {
        // すべてのエラーメッセージをクリア
        ClearAllResetErrors();

        // メールアドレス入力欄をクリア
        if (resetMailUserMail != null) resetMailUserMail.text = "";

        // パスワード再設定：メール送信パネルを表示
        if (resetMailPanel != null) resetMailPanel.SetActive(true);

        // 他のパスワード再設定パネルは非表示にする
        if (resetKeyPanel != null) resetKeyPanel.SetActive(false);
        if (resetPasswordPanel != null) resetPasswordPanel.SetActive(false);
        if (resetCompletePanel != null) resetCompletePanel.SetActive(false);
    }

    // 各「閉じる」ボタンを押したときの処理（メール/キー/パスワード）
    public void OnCloseResetPanels()
    {
        // すべてのパスワード再設定パネルを非表示にする
        if (resetMailPanel != null) resetMailPanel.SetActive(false);
        if (resetKeyPanel != null) resetKeyPanel.SetActive(false);
        if (resetPasswordPanel != null) resetPasswordPanel.SetActive(false);
        if (resetCompletePanel != null) resetCompletePanel.SetActive(false);

        // すべてのエラーメッセージをクリア
        ClearAllResetErrors();
    }

    //  パスワード再設定：メール送信パネルの「送信」ボタンを押したときの処理
    public void OnSendResetMailButtonClicked()
    {
        // ユーザが入力したメールアドレスを取得
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

        // パスワード再設定用ワンタイムキーを発行しユーザのメールアドレスに送信するPHPとの通信を開始
        StartCoroutine(ApiConnect.Post<User_mailRequest, BaseResponseData>(
            "PHP_user/otk_create.php",
            request,
            (res) =>
            {
                // 結果の処理
                if (res.success)
                {
                    // 成功時、メール送信パネルを非表示にしワンタイムキー入力パネルを表示
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

    // メール送信エラー表示を行う関数
    private void ShowResetMailError(string msg)
    {
        if (resetMailErrorText == null) return;
        resetMailErrorText.color = new Color(1f, 0.2f, 0.2f);
        resetMailErrorText.text = msg;
    }

    // ワンタイムキー入力パネルの「確認」ボタンを押したときの処理
    public void OnSendResetKeyButtonClicked()
    {
        // ユーザが入力したワンタイムキーとメールアドレスを取得
        string otk = resetKeyInput != null ? resetKeyInput.text.Trim() : "";
        string mail = resetMailUserMail != null ? resetMailUserMail.text.Trim() : "";

        // ワンタイムキー未入力チェック
        if (string.IsNullOrEmpty(otk))
        {
            ShowResetKeyError("ワンタイムキーを入力してください");
            return;
        }

        // エラーメッセージクリア
        ClearAllResetErrors();

        // リクエストデータの準備
        OtkAuthRequest request = new OtkAuthRequest
        {
            user_mail = mail,
            otk = otk
        };

        // ワンタイムキーの認証を行うPHPとの通信を開始
        StartCoroutine(ApiConnect.Post<OtkAuthRequest, BaseResponseData>(
            "PHP_user/otk_auth.php",
            request,
            (res) =>
            {
                // 結果の処理
                if (res.success)
                {
                    // ワンタイムキー認証成功時

                    // ワンタイムキー認証をパネルを非表示にし新パスワード入力パネルを表示
                    if (resetKeyPanel != null) resetKeyPanel.SetActive(false);
                    if (resetPasswordPanel != null) resetPasswordPanel.SetActive(true);

                    // 新パスワード入力欄をクリア
                    if (resetNewPass != null) resetNewPass.text = "";
                    if (resetNewPass2 != null) resetNewPass2.text = "";
                }
                else
                {
                    // ワンタイムキー認証失敗時はエラーを表示
                    ShowResetKeyError(res.message);
                }
            },
            (error) => { ShowResetKeyError(error); }
        ));
    }

    // ワンタイムキーエラーを表示する関数
    private void ShowResetKeyError(string msg)
    {
        if (resetKeyErrorText == null) return;
        resetKeyErrorText.color = new Color(1f, 0.2f, 0.2f);
        resetKeyErrorText.text = msg;
    }

    // 新パスワード入力パネルの「再設定」ボタンを押したときの処理
    public void OnResetPasswordButtonClicked()
    {
        // ユーザが入力したメールアドレスと新しいパスワード、確認用パスワードを取得
        string mail = resetMailUserMail != null ? resetMailUserMail.text.Trim() : "";
        string newPass = resetNewPass != null ? resetNewPass.text : "";
        string confirm = resetNewPass2 != null ? resetNewPass2.text : "";

        // 未入力チェック
        if (string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(confirm))
        {
            ShowResetPasswordError("新しいパスワードと確認用を入力してください");
            return;
        }

        // パスワードと確認用パスワードの一致チェック
        if (newPass != confirm)
        {
            ShowResetPasswordError("確認用パスワードが一致しません");
            return;
        }

        // エラーメッセージクリア
        ClearAllResetErrors();

        // リクエストデータの準備
        ResetPassRequest request = new ResetPassRequest
        {
            user_mail = mail,
            newpassword = newPass
        };

        // パスワード再設定処理を行うPHPとの通信を開始
        StartCoroutine(ApiConnect.Post<ResetPassRequest, BaseResponseData>(
            "PHP_user/reset_pass.php",
            request,
            (res) =>
            {
                // 結果の処理
                if (res.success)
                {
                    // パスワードの再設定成功時

                    // パスワード再設定パネルを非表示にし、完了パネルを表示
                    if (resetPasswordPanel != null) resetPasswordPanel.SetActive(false);
                    if (resetCompletePanel != null) resetCompletePanel.SetActive(true);
                }
                else
                {
                    // パスワードの再設定失敗時はエラーを表示
                    ShowResetPasswordError(res.message);
                }
            },
            (error) => { ShowResetPasswordError(error); }
        ));
    }

    // パスワード再設定エラーを表示する関数
    private void ShowResetPasswordError(string msg)
    {
        if (resetPasswordErrorText == null) return;
        resetPasswordErrorText.color = new Color(1f, 0.2f, 0.2f);
        resetPasswordErrorText.text = msg;
    }

    // 完了パネルの閉じるボタンを押したときの処理
    public void OnCloseResetCompletePanel() // 完了パネルを閉じる
    {
        if (resetCompletePanel != null)
            resetCompletePanel.SetActive(false);
    }

    // すべてのパスワード再設定エラーメッセージをクリアする関数
    private void ClearAllResetErrors()
    {
        if (resetMailErrorText != null) resetMailErrorText.text = "";
        if (resetKeyErrorText != null) resetKeyErrorText.text = "";
        if (resetPasswordErrorText != null) resetPasswordErrorText.text = "";
    }
}