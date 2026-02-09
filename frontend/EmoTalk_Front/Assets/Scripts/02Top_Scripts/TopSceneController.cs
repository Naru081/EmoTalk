using UnityEngine;

// TOP画面のシーンコントローラー
public class TopSceneController : MonoBehaviour
{

    // 初期化済みフラグ
    private bool initialized = false;

    // ==============================
    // MonoBehaviour関数
        // ==============================
    private void Start()
    {
        // 初期化処理
        if (initialized) return;
        initialized = true;

        // プロフィール情報の読み込み
        ProfileManager.Instance.LoadProfilesFromDB();
    }

}
