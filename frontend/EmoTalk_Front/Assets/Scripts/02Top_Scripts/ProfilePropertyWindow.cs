using UnityEngine;

public class ProfilePropertyWindow : MonoBehaviour
{
    public void Open(ProfileData data, System.Action onSaved)
    {
        // 後で実装
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}