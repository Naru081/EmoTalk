using UnityEngine;

public class ProfilePropertyWindow : MonoBehaviour
{
    // ==============================
    // プロパティ画面を開く
    // ==============================
    public void Open(ProfileData data, System.Action onSaved)
    {
        // 後で実装
        gameObject.SetActive(true);
    }

    // ==============================
    // プロパティ画面を閉じる
    // ==============================
    public void Close()
    {
        gameObject.SetActive(false);
    }
}