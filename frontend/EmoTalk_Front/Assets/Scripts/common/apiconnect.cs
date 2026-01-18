// UnityからPHPのAPIに接続するための共通クラス

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

public static class ApiConnect
{
    // PC実機テスト時のURL
    private const string BASE_URL = "http://localhost/backend/";

    // 実機テスト時のURL(※ngrok http 80で起動したURLを指定すること)
    // private const string BASE_URL = "http://ernestine-geoidal-gaynelle.ngrok-free.dev/backend/";

    [Serializable]
    // PHPからは、successとmessageを返す。追加のデータは別クラスを作り対応
    public class BasicResponse
    {
        public bool success;
        public string message;
    }

    // 通信処理を行う共通メソッド
    public static IEnumerator Post<TRequest, TResponse>(
        string endpoint,    // PHPのファイル名
        TRequest requestData,   // 送信するデータをクラスで受け取る
        Action<TResponse> onSuccess,    // 成功時に呼び出されるコールバック
        Action<string> onError = null   // エラー時に呼び出されるコールバック
    )
    {
        // URL作成
        string url = BASE_URL + endpoint;

        // C#のデータをJSON形式に変換
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

        // 通信エラーの確認
        if (request.result != UnityWebRequest.Result.Success)   // 通信が失敗した場合
        {
            onError?.Invoke("通信エラー: " + request.error);
            yield break;
        }

        // 返送されたJSONをTResponse型に変換
        try
        {
            Debug.Log("受信したJSONデータ: [" + request.downloadHandler.text + "]");
            TResponse res = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
            onSuccess?.Invoke(res);
        }
        catch
        {
            onError?.Invoke("JSONの解析に失敗しました");
        }
    }
}
