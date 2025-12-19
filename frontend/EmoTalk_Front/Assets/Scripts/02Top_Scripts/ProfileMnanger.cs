using System;
using System.Collections.Generic;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;

    private const string KEY_PROFILES = "UserProfiles";
    private const string KEY_SELECTED = "SelectedProfileId";

    public List<ProfileData> Profiles = new List<ProfileData>();

    // 選択中プロファイルID
    private int selectedProfileId = -1;

    // プロフィールリスト変更通知
    public event Action OnProfilesChanged;
    public void NotifyChanged()
    {
        OnProfilesChanged?.Invoke();
    }

    // ==============================
    //  読み込み
    // ==============================
    private void Awake()
    {
        // すでに別のインスタンスがある場合は自分を破棄
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 最初の1個をシングルトンとして保持
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 起動時にプロフィールと選択状態を読み込む
        LoadProfiles();
        EnsureProfileIds();
        LoadSelectedProfile();
        FixSelectedIfMissing();
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
    //  プロフIDの整合性チェック
    // ==============================
    private void EnsureProfileIds()
    {
        int nextId = PlayerPrefs.GetInt("NextProfileId", 1);

        bool changed = false;

        foreach (var profile in Profiles)
        {
            if (profile.profileId <= 0)
            {
                profile.profileId = nextId++;
                changed = true;
            }
        }
        if (changed)
        {
            PlayerPrefs.SetInt("NextProfileId", nextId);
            PlayerPrefs.Save();
            SaveProfiles();
            NotifyChanged();
        }
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
        // 表示名用の番号を決める（空いている番号）
        int number = GetNextModelNumber();
        string displayName = $"Model#{number}"; // ← ここはお好みで

        ProfileData data = new ProfileData(modelIndex, displayName);

        // ★ユニークIDを付与（PlayerPrefsで連番管理）
        int nextId = PlayerPrefs.GetInt("NextProfileId", 1);
        data.profileId = nextId;
        PlayerPrefs.SetInt("NextProfileId", nextId + 1);
        PlayerPrefs.Save();

        Profiles.Add(data);
        SaveProfiles();
        return data;
    }


    // ==============================
    //  プロフィール削除
    // ==============================
    public void DeleteProfile(int profileId)
    {
        Profiles.RemoveAll(p => p.profileId == profileId);
        SaveProfiles();
        NotifyChanged();
    }

    // ==============================
    //  プロフィール更新
    // ==============================
    public void UpdateProfile(ProfileData data)
    {
        SaveProfiles();
        NotifyChanged();
    }

    // ==============================
    //  選択中プロフの管理
    // ==============================
    private void LoadSelectedProfile()
    {
        selectedProfileId = PlayerPrefs.GetInt(KEY_SELECTED, -1);
    }
    // 選択中プロファイルを設定
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

        NotifyChanged();
    }
    // 選択中プロファイルを取得
    public ProfileData GetSelectedProfile()
    {
        foreach (var p in Profiles)
        {
            if (p.profileId == selectedProfileId)
                return p;
        }
        return null;
    }
    // 選択中プロファイルIDを取得
    public int GetSelectedProfileId()
    {
        return selectedProfileId;
    }
    // 選択中プロフが存在しない場合、先頭を選択する
    private void FixSelectedIfMissing()
    {
        // 選択IDがProfilesに存在しない場合、先頭を選択
        bool exists = Profiles.Exists(p => p.profileId == selectedProfileId);
        if (!exists && Profiles.Count > 0)
        {
            selectedProfileId = Profiles[0].profileId;
            PlayerPrefs.SetInt(KEY_SELECTED, selectedProfileId);
            PlayerPrefs.Save();
            NotifyChanged();
        }
    }

    // ==============================
    //  表示名用の番号を決める
    // ==============================
    private int GetNextModelNumber()
    {
        // すでに使われている番号を集める
        var usedNumbers = new HashSet<int>();

        foreach (var p in Profiles)
        {
            if (string.IsNullOrEmpty(p.displayName))
            continue;

            // "Model#2" 形式だけを対象にする
            if (p.displayName.StartsWith("Model#"))
            {
                var numPart = p.displayName.Replace("Model#", "");
                if (int.TryParse(numPart, out int n))
                {
                    usedNumbers.Add(n);
                }
            }
        }
        // 1 から順に空いている番号を探す
        int candidate = 1;
        while (usedNumbers.Contains(candidate))
        {
            candidate++;
        }

        return candidate;
    }
}