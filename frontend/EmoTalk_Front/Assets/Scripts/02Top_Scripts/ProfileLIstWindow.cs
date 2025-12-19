using System.Collections.Generic;
using UnityEngine;

public class ProfileListWindow : MonoBehaviour
{
    [Header("UI")]
    public Transform contentRoot;            
    public GameObject profileItemPrefab;

    // プロフィール編集画面を開く
    public ProfileEditWindow editWindow;

    // プロファイルデータ更新
    public static ProfileListWindow Instance{ get; private set;}

    // ==============================
    // 保存されたプロフィールデータをリスト表示
    // ==============================
    public void Awake()
    {
        Instance = this;
    }
    // プロフィールリストの更新を開始
    public void Start()
    {
        RefreshList();
    }

    // ==============================
    // プロフィールリストの更新
    // ==============================
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
            ProfileController item = obj.GetComponent<ProfileController>();
            item.Setup(data, OnItemClicked);
        }
    }

    // ==============================
    // データリストが選択されたときの処理
    // ==============================
    private void OnItemClicked(ProfileData data)
    {

        if(editWindow != null)
        {
            editWindow.Open(data);
        }
        else
        {
            Debug.LogWarning("editWindow is NULL");
        }
    }
    // ==============================
    // プロフィールデータの変更を監視
    // ==============================
    void OnEnable()
    {
        if (ProfileManager.Instance != null)
            ProfileManager.Instance.OnProfilesChanged += RefreshList;

        RefreshList();
    }
    // ==============================
    // プロフィールデータの変更監視解除
    // ==============================
    void OnDisable()
    {
        if (ProfileManager.Instance != null)
            ProfileManager.Instance.OnProfilesChanged -= RefreshList;
    }
}