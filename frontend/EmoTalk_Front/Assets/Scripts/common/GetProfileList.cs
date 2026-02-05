using System;
using System.Collections;
using UnityEngine;

// プロファイル一覧を取得するクラス
public static class GetProfileList
{
    // PHPからのレスポンスを受け取るクラス
    [Serializable]
    public class Request
    {
        public int user_id;
    }

    // PHPからのレスポンスを受け取るクラス
    public static IEnumerator GetProfiles(
        int user_id,
        Action<GetProfileResponse> onSuccess,
        Action<string> onError = null
    )
    {
        var req = new Request
        {
            user_id = user_id
        };

        // プロファイル一覧を取得する処理を行うPHPを呼び出す
        yield return ApiConnect.Post<GetProfileList.Request, GetProfileResponse>(
            "PHP_profile/get_profile.php",
            req,
            onSuccess,
            onError
        );
    }
}
