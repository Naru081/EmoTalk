// UnityからPHP(バックエンド(サーバ))へ通信する処理を共通化するためのクラス

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

// APIを接続するクラス
public static class ApiConnect
{
    // 実機テスト時のURL(※ngrok http 80で起動したURLを指定すること)
    // private const string BASE_URL = "https://ernestine-geoidal-gaynelle.ngrok-free.dev/backend/";

    // 実際の環境でのテスト及び本番稼働時では、localhostの部分をサーバを起動している端末のIPアドレスに変更が必要です
    private const string BASE_URL = "http://localhost/backend/";

    [Serializable]
    // 基本的にPHPからはsuccessとmessageを返す。 追加のデータを返す場合は別のクラスをつくって対応
    public class BasicResponse
    {
        public bool success;
        public string message;
    }

    // ==============================
    // 通信の処理を共通化するメソッド
    // ==============================
    public static IEnumerator Post<TRequest, TResponse>(
        string endpoint,    // PHPのファイル名(例：PHP_user/register.php)
        TRequest requestData,   // 送るデータの内容をクラスで受け取る
        Action<TResponse> onSuccess,    // 成功したときに呼ぶ処理
        Action<string> onError = null   // エラー時に呼ぶ処理（省略可)
    )
    {
        // URLの作成
        string url = BASE_URL + endpoint;

        // C#のデータをJSONに変換
        string json = JsonUtility.ToJson(requestData);

        // JSONを配列に変換
        byte[] body = Encoding.UTF8.GetBytes(json);

        // 通信の準備
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");   // JSON形式をセット

        // 通信実行
        yield return request.SendWebRequest();

        // 通信エラーのチェック
        if (request.result != UnityWebRequest.Result.Success)   // 通信に失敗した場合
        {
            onError?.Invoke("通信エラー：" + request.error);
            yield break;
        }

        string raw = request.downloadHandler.text.Trim();
        Debug.Log("レスポンス生データ: [" + raw + "]");

        // レスポンスデータがJSONじゃなかったら即エラーにする
        if (!raw.StartsWith("{"))
        {
            onError?.Invoke("JSONではありません:\n" + raw);
            yield break;
        }

        // JSONをC#のクラスに変換
        try
        {
            TResponse res = JsonUtility.FromJson<TResponse>(raw);
            onSuccess?.Invoke(res);
        }
        catch (Exception e)
        {
            onError?.Invoke("レスポンス解析エラー:\n" + e.Message + "\n" + raw);
        }
    }

    // ==============================
    // 音声ファイル（wav形式）送信用
    // ==============================
    public static IEnumerator PostWav<TResponse>(
        string endpoint,                // PHPのファイル名(PHP_apir/wisper_api.php)
        byte[] wavBytes,                // 送るwavファイルのバイト配列
        Action<TResponse> onSuccess,
        Action<string> onError = null
        )
    {
        // URLの作成
        string url = BASE_URL + endpoint;

        // フォームデータの作成
        WWWForm form = new WWWForm();
        form.AddBinaryData(
            "audio",                    // 送信するファイルのキー名 (php側で$_FILES['audio']で受け取る)
            wavBytes,
            "audio.wav",
            "audio/wav"
        );

        // 通信リクエストの作成
        UnityWebRequest request = UnityWebRequest.Post(url, form);

        // 通信実行
        yield return request.SendWebRequest();

        // 通信エラーのチェック
        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("通信エラー:" + request.error);
            yield break;
        }

        // レスポンスの取得
        string raw = request.downloadHandler.text.Trim();
        Debug.Log("レスポンス生データ: [" + raw + "]");

        // レスポンスデータがJSONじゃなかったら即エラー出力
        if (!raw.StartsWith("{"))
        {
            onError?.Invoke("JSONではありません:\n" + raw);
            yield break;
        }

        // JSONをC#のクラスに変換
        try
        {
            TResponse res = JsonUtility.FromJson<TResponse>(raw);
            onSuccess?.Invoke(res);
        }
        catch (Exception e)
        {
            onError?.Invoke("レスポンス解析エラー:\n" + e.Message + "\n" + raw);
        }
    }
}