using System.Collections.Generic;
using UnityEngine;

// 要録されているプロファイルデータをリスト表示するウインドウ
public class ProfileListWindow : MonoBehaviour
{
    [Header("UI")]
    public Transform contentRoot;          // プロファイルアイテムのコンテンツルート
    public GameObject profileItemPrefab;    // プロファイルアイテムのプレハブ

    public ProfileEditWindow editWindow;    // プロファイル編集画面を開く参照

    public static ProfileListWindow Instance{ get; private set;}    // プロファイルデータ更新

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

        // プロファイルデータの変更を監視
        ProfileManager.Instance.OnProfilesChanged += RefreshList;

        // 最初のリスト表示
        ProfileManager.Instance.LoadProfilesFromDB();
    }

    // ==============================
    // プロファイルデータの変更監視解除
    // ==============================
    void OnDisable()
    {
        // プロファイルデータの変更監視解除
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

        // 既存のリストをクリア
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        // 新しいプロファイルデータでリストを再生成
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
        // プロファイル編集画面を開く
        if (editWindow != null)
            editWindow.Open(data);
    }

}