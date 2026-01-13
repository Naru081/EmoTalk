using UnityEngine;

public class TopSceneController : MonoBehaviour
{

    private bool initialized = false;

    private void Start()
    {
        // 二重実行防止
        if (initialized) return;
        initialized = true;

        // ProfileManagerのプロファイル一覧をDBから取得
        ProfileManager.Instance.LoadProfilesFromDB();
    }

}
