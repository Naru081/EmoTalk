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

        StartCoroutine(LoginRequest(email, password));
    }

    private IEnumerator LoginRequest(string email, string password)
    {
        yield return new WaitForSeconds(0.5f);

        // ログイン処理を行うPHPのurl (実機稼働時にはサーバーのURLに変更すること)
        string url = "http://localhost/backend/PHP_user/login.php";

        // リクエストデータ作成
        RequestData reqData = new RequestData();
        reqData.email = email;
        reqData.password = password;

        // JSONに変換
        string json = JsonUtility.ToJson(reqData);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        // 通信準備
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 通信開始
        yield return request.SendWebRequest();

        // 通信エラー
        if (request.result != UnityWebRequest.Result.Success)
        {
            ShowLoginError("通信エラーが発生しました。");
            yield break;
        }

        string responseText = request.downloadHandler.text;
        Debug.Log("サーバー応答:" + responseText);

        // JSON解析
        ResponseData res;
        try
        {
            res = JsonUtility.FromJson<ResponseData>(responseText);
        }
        catch
        {
            ShowLoginError("サーバー応答の解析に失敗しました。");
            yield break;
        }

        // 結果の処理
        if (res.success)
        {
            // ログイン成功時、ロード画面表示
            SetLoading(true, "ログインに成功しました");
            // ロード画面を1秒間表示する
            yield return new WaitForSeconds(1f);
            // その後非表示にする
            loadingPanel.SetActive(false);

            // シーン移動(TOP画面へ)
            SceneManager.LoadScene("TopScene");
        }
        else
        {
            // エラー表示
            ShowLoginError(res.message);
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

        StartCoroutine(RegisterRequest(email, password));
    }

    private IEnumerator RegisterRequest(string email, string password)
    {
        yield return new WaitForSeconds(0.5f);

        // 新規登録処理を行うPHPのurl (実機稼働時にはサーバーのURLに変更すること)
        string url = "http://localhost/backend/PHP_user/register.php";

        // リクエストデータ作成
        RequestData reqData = new RequestData();
        reqData.email = email;
        reqData.password = password;

        // JSONに変換
        string json = JsonUtility.ToJson(reqData);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        // 通信準備
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 通信開始
        yield return request.SendWebRequest();

        // 通信エラー
        if (request.result != UnityWebRequest.Result.Success)
        {
            ShowRegisterError("通信エラーが発生しました。");
            yield break;
        }

        string responseText = request.downloadHandler.text;
        Debug.Log("サーバー応答:" + responseText);

        // JSON解析
        ResponseData res;
        try
        {
            res = JsonUtility.FromJson<ResponseData>(responseText);
        }
        catch
        {
            ShowRegisterError("サーバー応答の解析に失敗しました。");
            yield break;
        }

        // 結果の処理
        if (res.success)
        {
            // 成功画面へ
            if (registerPanel != null) registerPanel.SetActive(false);
            if (registerCompletePanel != null) registerCompletePanel.SetActive(true);
        }
        else
        {
            // エラー表示
            ShowRegisterError(res.message);
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

        // メール送信リクエスト開始
        StartCoroutine(SendResetMailRequest(email));
    }

    // メール送信リクエスト
    private IEnumerator SendResetMailRequest(string email)
    {
        yield return new WaitForSeconds(0.5f);

        // ワンタイムキーを発行してメール送信するPHPのurl (実機稼働時にはサーバーのURLに変更すること)
        string url = "http://localhost/backend/PHP_user/otk_create.php";

        // リクエストデータ作成
        RequestData reqData = new RequestData();
        reqData.email = email;

        // JSONに変換
        string json = JsonUtility.ToJson(reqData);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        // 通信準備
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "qpplication/json");

        // 通信開始
        yield return request.SendWebRequest();

        // 通信エラー
        if (request.result != UnityWebRequest.Result.Success)
        {
            ShowResetMailError("通信エラーが発生しました。");
            yield break;
        }

        // データ取得(レスポンスデータ)
        string responseText = request.downloadHandler.text;
        Debug.Log("サーバー応答:" + responseText);

        // JSON解析
        ResponseData res;
        try
        {
            res = JsonUtility.FromJson<ResponseData>(responseText);
        } catch{
            ShowResetMailError("サーバー応答の解析に失敗しました。");
            yield break;
        }

        // ダミー成功処理 (後で削除)
        res.success = true;

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
            ShowResetMailError(res.message);
            yield break;
        }
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
        StartCoroutine(CheckResetKeyRequest(mail, otk));
    }

    // ワンタイムキー確認リクエスト
    private IEnumerator CheckResetKeyRequest(string mail, string key)
    {
        yield return new WaitForSeconds(0.5f);

        // ワンタイムキーの認証を行うPHPのurl (実機稼働時にはサーバーのURLに変更すること)
        string url = "http://localhost/backend/PHP_user/otk_auth.php";

        // リクエストデータ作成
        RequestData reqData = new RequestData();
        reqData.email = mail;
        reqData.otk = key;

        // JSONに変換
        string json = JsonUtility.ToJson(reqData);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        // 通信準備
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 通信開始
        yield return request.SendWebRequest();

        // 通信エラー
        if (request.result != UnityWebRequest.Result.Success)
        {
            ShowResetKeyError("通信エラーが発生しました。");
            yield break;
        }

        // データ取得(レスポンスデータ)
        string responseText = request.downloadHandler.text;
        Debug.Log("サーバー応答:" + responseText);

        // JSON解析
        ResponseData res;
        try
        {
            res = JsonUtility.FromJson<ResponseData>(responseText);
        } catch {
            ShowResetKeyError("サーバー応答の解析に失敗しました。");
            yield break;
        }

        // ダミー成功処理 (後で削除)
        res.success = true;

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
            ShowResetKeyError(res.message);
        }
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
        StartCoroutine(ResetPasswordRequest(newPass, mail));
    }


    // パスワード再設定リクエスト
    private IEnumerator ResetPasswordRequest(string newPass, string mail)
    {
        yield return new WaitForSeconds(0.5f);

        // パスワード再設定を行うPHPのurl (実機稼働時にはサーバーのURLに変更すること)
        string url = "http://localhost/backend/PHP_user/reset_pass.php";

        // リクエストデータ作成
        RequestData reqData = new RequestData();
        reqData.email = mail;
        reqData.password = newPass;

        // JSONに変換
        string json = JsonUtility.ToJson(reqData);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        // 通信準備
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 通信開始
        yield return request.SendWebRequest();

        // 通信エラー
        if (request.result != UnityWebRequest.Result.Success)
        {
            ShowResetPasswordError("通信エラーが発生しました。");
            yield break;
        }

        // データ取得(レスポンスデータ)
        string responseText = request.downloadHandler.text;
        Debug.Log("サーバー応答:" + responseText);

        // JSON解析
        ResponseData res;
        try
        {
            res = JsonUtility.FromJson<ResponseData>(responseText);
        } catch
        {
            ShowResetPasswordError("サーバー応答の解析に失敗しました。");
            yield break;
        }

        // ダミー成功処理 (後で削除)
        res.success = true;

        // 結果の処理
        if (res.success)
        {
            // 成功時、完了パネルへ
            if (resetPasswordPanel != null) resetPasswordPanel.SetActive(false);
            if (resetCompletePanel != null) resetCompletePanel.SetActive(true);
        }
        else
        {
            ShowResetPasswordError("パスワードの再設定に失敗しました。");
        }
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


    // PHP 通信用データクラス群

    // 全PHPリクエストで共通
    [System.Serializable]
    private class RequestData
    {
        public string email;
        public string password;
        public string otk; // ワンタイムキー用
    }

    // 全PHPレスポンスで共通
    [System.Serializable]
    private class ResponseData
    {
        public bool success;
        public string message;
    }

}