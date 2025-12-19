using UnityEngine;

public class ModelSelectWindow : MonoBehaviour
{
    private ProfileData currentProfile;
    private System.Action<int> onSelected;

    // ==============================
    // モデル選択ウィンドウの表示
    // ==============================
    public void Open(ProfileData data, System.Action<int> onSelected)
    {
        currentProfile = data;
        this.onSelected = onSelected;
        gameObject.SetActive(true);
    }

    // ==============================
    // モデル選択ウィンドウの非表示
    // ==============================
    public void Close()
    {
        gameObject.SetActive(false);
    }

    // ==============================
    // モデル選択時の処理
    // ==============================
    public void OnSelectModel(int index)
    {
        onSelected?.Invoke(index);
        Close();
    }
}