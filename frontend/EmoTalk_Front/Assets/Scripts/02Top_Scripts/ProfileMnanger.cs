using System.Collections.Generic;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance { get; private set; }

    private const string KEY_PROFILES = "UserProfiles";
    private const string KEY_SELECTED = "SelectedProfileId";

    public List<ProfileData> Profiles = new List<ProfileData>();

    private int selectedProfileId = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Profiles = new List<ProfileData>();
    }


    // ==============================
    //  プロフ読み込み
    // ==============================
    private void LoadProfiles()
    {
        string json = PlayerPrefs.GetString(KEY_PROFILES, "");

        if (string.IsNullOrEmpty(json))
        {
            Profiles = new List<ProfileData>();
            return;
        }

        ProfileListWrapper wrapper = JsonUtility.FromJson<ProfileListWrapper>(json);
        Profiles = wrapper.profiles ?? new List<ProfileData>();
    }

    // ==============================
    //  プロフ保存
    // ==============================
    public void SaveProfiles()
    {
        ProfileListWrapper wrapper = new ProfileListWrapper();
        wrapper.profiles = Profiles;

        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(KEY_PROFILES, json);
        PlayerPrefs.Save();
    }

    // ==============================
    //  新規プロフィール追加
    // ==============================
    public ProfileData CreateProfile(int modelIndex)
    {
        ProfileData data = new ProfileData(modelIndex, $"モデル{modelIndex + 1}");
        Profiles.Add(data);
        SaveProfiles();
        return data;
    }

    // ==============================
    //  プロフィール削除
    // ==============================
    public bool DeleteProfile(ProfileData data)
    {
        if (data.profileId == selectedProfileId)
        {
            Debug.LogWarning("選択中プロファイルのため削除不可");
            return false;
        }

        Profiles.Remove(data);
        SaveProfiles();
        return true;
    }

    // ==============================
    //  プロフィール更新
    // ==============================
    public void UpdateProfile(ProfileData data)
    {
        SaveProfiles();
    }

    // ==============================
    //  選択中プロフの管理
    // ==============================
    private void LoadSelectedProfile()
    {
        selectedProfileId = PlayerPrefs.GetInt(KEY_SELECTED, -1);
    }

    public void SelectProfile(ProfileData data)
    {
        selectedProfileId = data.profileId;
        PlayerPrefs.SetInt(KEY_SELECTED, data.profileId);
        PlayerPrefs.Save();

        // ModelManager に反映して Live2D を変更
        if (ModelManager.Instance != null)
        {
            ModelManager.Instance.ShowModel(data.modelIndex);
        }
    }

    public ProfileData GetSelectedProfile()
    {
        foreach (var p in Profiles)
        {
            if (p.profileId == selectedProfileId)
                return p;
        }
        return null;
    }

    #if UNITY_EDITOR
    void OnApplicationQuit()
    {
        Profiles.Clear();
    }
    #endif

}