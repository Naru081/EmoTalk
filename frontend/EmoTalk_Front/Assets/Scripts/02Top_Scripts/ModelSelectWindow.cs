using UnityEngine;

public class ModelSelectWindow : MonoBehaviour
{
    private ProfileData currentProfile;
    private System.Action<int> onSelected;

    public void Open(ProfileData data, System.Action<int> onSelected)
    {
        currentProfile = data;
        this.onSelected = onSelected;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    // 例：UIボタンから呼ぶ
    public void OnSelectModel(int index)
    {
        onSelected?.Invoke(index);
        Close();
    }
}