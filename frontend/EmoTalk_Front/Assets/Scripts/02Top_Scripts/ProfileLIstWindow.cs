using System.Collections.Generic;
using UnityEngine;

public class ProfileListWindow : MonoBehaviour
{
    [Header("UI")]
    public Transform contentRoot;            
    public GameObject profileItemPrefab;

    // プロファイル編集画面を開く
    public ProfileEditWindow editWindow;

    // プロファイルデータ更新
    public static ProfileListWindow Instance{ get; private set;}

    // ==============================
    // 保存されたプロファイルデータをリスト表示
    // ==============================
    public void Awake()
    {
        Instance = this;
    }

    // ==============================
    // プロファイルデータの変更を監視
    // ==============================
    void OnEnable()
    {
        if (ProfileManager.Instance == null) return;

        ProfileManager.Instance.OnProfilesChanged += RefreshList;
        ProfileManager.Instance.LoadProfilesFromDB();
    }

    // ==============================
    // プロファイルデータの変更監視解除
    // ==============================
    void OnDisable()
    {
        if (ProfileManager.Instance != null)
            ProfileManager.Instance.OnProfilesChanged -= RefreshList;
    }

    // ==============================
    // プロファイルリストの更新
    // ==============================
    public void RefreshList()
    {
        if (ProfileManager.Instance == null)
            return;

        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        foreach (var data in ProfileManager.Instance.Profiles)
        {
            GameObject obj = Instantiate(profileItemPrefab, contentRoot);
            ProfileController item = obj.GetComponent<ProfileController>();
            item.Setup(data, OnItemClicked);
        }

        Debug.Log("RefreshList: " + ProfileManager.Instance.Profiles.Count);
    }

    // ==============================
    // データリストが選択されたときの処理
    // ==============================
    private void OnItemClicked(ProfileData data)
    {

        if (editWindow != null)
            editWindow.Open(data);
    }

}