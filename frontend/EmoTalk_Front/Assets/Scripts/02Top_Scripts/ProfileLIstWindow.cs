using System.Collections.Generic;
using UnityEngine;

public class ProfileListWindow : MonoBehaviour
{
    [Header("UI")]
    public Transform contentRoot;            
    public GameObject profileItemPrefab;    

    // プロフィール編集画面を開く！
    public ProfileEditWindow editWindow;

    // プロファイルデータ更新
    public static ProfileListWindow Instance{ get; private set;}
    public void Awake()
    {
        Instance = this;
    }
    
    public void Start()
    {
        RefreshList();
    }

    public void RefreshList()
    {
        if (ProfileManager.Instance == null || ProfileManager.Instance.Profiles == null)
        {
            Debug.Log("ProfileManager not ready yet");
            return;
        }

        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        List<ProfileData> list = ProfileManager.Instance.Profiles;

        foreach (var data in list)
        {
            GameObject obj = Instantiate(profileItemPrefab, contentRoot);
            ProfileItemController item = obj.GetComponent<ProfileItemController>();
            item.Setup(data, OnItemClicked);
        }
    }

    private void OnItemClicked(ProfileData data)
    {
        // ★ 設計図通り：まずプロフィール編集画面へ！
        editWindow.Open(data);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}