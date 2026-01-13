// UnityからPHP(バックエンド(サーバ))へ通信する処理を共通化するためのクラス

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

public static class ApiConnect
{
    // 実際の環境でのテスト及び本番稼働時では、localhostの部分をサーバを起動している端末のIPアドレスに変更が必要です
    private const string BASE_URL = "http://172.20.10.6/backend/";

    [Serializable]
    // 基本的にPHPからはsuccessとmessageを返す。 追加のデータを返す場合は別のクラスをつくって対応
    public class BasicResponse
    {
        public bool success;
        public string message;
    }

    // 通信の処理を共通化するメソッド

    // ==============================
    // テキスト用
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

        // 返送されたJSONをTResponseに変換
        try
        {
            Debug.Log("HTTP: " + request.responseCode);
            Debug.Log("レスポンス生データ: [" + request.downloadHandler.text + "]");
            TResponse res = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
            onSuccess?.Invoke(res);
        }
        catch (Exception e)
        {
            onError?.Invoke("レスポンス解析エラー: " + e.Message +
            "\nHTTP: " + request.responseCode +
            "\nRAW: " + request.downloadHandler.text);
        }
    }


    // ==============================
    // 音声用
    // ==============================
    public static IEnumerator PostVoice<TResponse>(
        string endpoint,
        int profId,
        byte[] wavData,
        Action<TResponse> onSuccess,
        Action<string> onError = null
    )
    {
        string url = BASE_URL + endpoint;

        WWWForm form = new WWWForm();
        form.AddField("prof_id", profId);
        form.AddBinaryData("audio", wavData, "input.wav", "audio/wav");

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("通信エラー：" + request.error);
            yield break;
        }

        try
        {
            Debug.Log("HTTP: " + request.responseCode);
            Debug.Log("レスポンス生データ: [" + request.downloadHandler.text + "]");
            TResponse res = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
            onSuccess?.Invoke(res);
        }
        catch (Exception e)
        {
            onError?.Invoke("レスポンス解析エラー: " + e.Message +
            "\nHTTP: " + request.responseCode +
            "\nRAW: " + request.downloadHandler.text);
        }
    }
}