using System;
using System.Collections;
using UnityEngine;

// プロファイル一覧を取得するための静的クラス
public static class GetProfileList
{
    // ==============================
    // PHPへ送信するリクエストデータを定義
    // ==============================
    [Serializable]
    public class Request
    {
        public int user_id; // 取得対象となるユーザーの識別ID
    }

    // ==============================
    // プロファイル一覧を取得するためのコルーチンメゾット
    // ==============================
    public static IEnumerator GetProfiles(
        int user_id,
        Action<GetProfileResponse> onSuccess,
        Action<string> onError = null
    )
    {
        // リクエストオブジェクトを作成し、ユーザIDを格納
        var req = new Request
        {
            user_id = user_id
        };

        //共通通信を利用して、指定したPHPエンドポイントにPOSTリクエストを送信
        yield return ApiConnect.Post<GetProfileList.Request, GetProfileResponse>(
            "PHP_profile/get_profile.php",  // APIの相対パス
            req,                            // リクエスト本体
            onSuccess,                      // 成功時処理
            onError                         // 失敗時処理
        );
    }
}
