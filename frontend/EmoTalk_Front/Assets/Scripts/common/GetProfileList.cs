using System;
using System.Collections;
using UnityEngine;

public static class GetProfileList
{
    [Serializable]
    public class Request
    {
        public int user_id;
    }

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

        yield return ApiConnect.Post<GetProfileList.Request, GetProfileResponse>(
            "PHP_profile/get_profile.php",
            req,
            onSuccess,
            onError
        );
    }
}
