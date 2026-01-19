using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;

    // プロファイル一覧
    public List<ProfileData> Profiles = new List<ProfileData>();

    // 選択中プロファイルID (UserData管理)
    private int selectedProfileId = -1;

    // プロファイルリスト変更通知
    public event Action OnProfilesChanged;
    public void NotifyChanged()
    {
        // リスト更新処理
        OnProfilesChanged?.Invoke();

        if (ProfileListWindow.Instance != null)
        {
            ProfileListWindow.Instance.RefreshList();
        }
    }

    // ==============================
    // 起動
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

        // 起動時に選択中のプロファイルIDを取得(UserDataから)
        selectedProfileId = UserData.GetUserCurrentProfId();
    }

    // ==============================
    //  DBからプロファイル一覧を取得
    // ==============================
    public void LoadProfilesFromDB()
    {
        // user_id取得 (UserDataから)
        int user_id = UserData.GetUserId();

        // DB接続関数を呼び出してプロファイル一覧を取得
        StartCoroutine(
            GetProfileList.GetProfiles(
                user_id,
                OnProfilesLoaded,
                OnProfileLoadError
            )
        );
    }

    private void OnProfilesLoaded(GetProfileResponse res)
    {
        Profiles.Clear();

        foreach (var dbProf in res.profile_data)
        {
            Profiles.Add(ConvertFromDB(dbProf));
        }

        // 選択中プロフの確認
        NotifyChanged();
    }

    private void OnProfileLoadError(string error)
    {
        Debug.LogError("プロファイル取得失敗: " + error);
    }

    // ==============================
    //  DBデータからProfileDataへ変換
    // ==============================
    private ProfileData ConvertFromDB(ProfileDataFromDB db)
    {
        ProfileData data = new ProfileData(
            db.model_id,
            db.prof_title
        );

        // DBデータをセット
        data.profileId = db.prof_id;
        data.personality = db.prof_chara;
        data.tone = db.prof_tone;
        data.pronoun = db.prof_fp;

        return data;
    }

    // ==============================
    // プロファイル作成
    // ==============================
    public void CreateProfile
    (
        int modelIndex,
        string title,
        string chara,
        string tone,
        string firstPerson
    )
    {
        StartCoroutine(CreateProfileCoroutine(
            modelIndex,
            title,
            chara,
            tone,
            firstPerson
        ));
    }

    private IEnumerator CreateProfileCoroutine(
        int modelIndex,
        string title,
        string chara,
        string tone,
        string firstPerson
        )
    {
        var req = new CreateProfileRequest
        {
            user_id = UserData.GetUserId(),
            model_id = modelIndex,
            prof_title = title,
            prof_chara = chara,
            prof_tone = tone,
            prof_fp = firstPerson
        };

        yield return ApiConnect.Post<CreateProfileRequest, CreateProfileResponse>(
            "PHP_profile/create_profile.php",
            req,
            (res) =>
            {
                if (!res.success)
                {
                    Debug.LogError(res.message);
                    return;
                }

                LoadProfilesFromDB();   // プロファイル一覧を再取得して更新
            }
        );
    }

    // ==============================
    // プロファイル更新
    // ==============================
    public void UpdateProfile(ProfileData data)
    {
        // 後にDB更新処理を追加予定
        NotifyChanged();
    }

    // プロファイルのタイトルを変更
    public void UpdateProfileTitle(ProfileData data)
    {
        StartCoroutine(UpdateProfileTitleCoroutine(data));
    }

    private IEnumerator UpdateProfileTitleCoroutine(ProfileData data)
    {
        var req = new UpdateProfileTitleRequest
        {
            user_id = UserData.GetUserId(),
            prof_id = data.profileId,
            prof_title = data.displayName
        };

        yield return ApiConnect.Post<UpdateProfileTitleRequest, GetProfileResponse>(
            "PHP_profile/update_profile_title.php",
            req,
            (res) =>
            {
                if (!res.success)
                {
                    Debug.LogError("プロファイルタイトル更新失敗");
                    return;
                }

                LoadProfilesFromDB();
            }
        );
    }

    // プロファイルの性格・口調・一人称を変更
    public void UpdateProfileCustom(ProfileData data)
    {
        StartCoroutine(UpdateProfileCustomCoroutine(data));
    }

    private IEnumerator UpdateProfileCustomCoroutine(ProfileData data)
    {
        var req = new UpdateProfileCustomRequest
        {
            user_id = UserData.GetUserId(),
            prof_id = data.profileId,
            prof_chara = data.personality,
            prof_tone = data.tone,
            prof_fp = data.pronoun
        };
        yield return ApiConnect.Post<UpdateProfileCustomRequest, GetProfileResponse>(
            "PHP_profile/update_profile_custom.php",
            req,
            (res) =>
            {
                if (!res.success)
                {
                    Debug.LogError("プロファイルカスタム更新失敗");
                    return;
                }

                LoadProfilesFromDB();
            },
            error => Debug.LogError("プロファイルカスタム更新エラー: " + error)
        );
    }

    // プロファイルのモデルを変更
    public void UpdateProfileModel(ProfileData data)
    {
        StartCoroutine(UpdateProfileModelCoroutine(data));
    }

    private IEnumerator UpdateProfileModelCoroutine(ProfileData data)
    {
        var req = new UpdateProfileModelRequest
        {
            user_id = UserData.GetUserId(),
            prof_id = data.profileId,
            model_id = data.modelIndex
        };
        yield return ApiConnect.Post<UpdateProfileModelRequest, GetProfileResponse>(
            "PHP_profile/update_profile_model.php",
            req,
            (res) =>
            {
                if (!res.success)
                {
                    Debug.LogError("プロファイルモデル更新失敗");
                    return;
                }

                LoadProfilesFromDB();
            },
            error => Debug.LogError("プロファイルモデル更新エラー: " + error)
        );
    }

    // ==============================
    // プロファイル削除
    // ==============================
    public void DeleteProfile(int profileId)
    {
        StartCoroutine(DeleteProfilesCoroutine(profileId));
    }

    private IEnumerator DeleteProfilesCoroutine(int profileId)
    {
        var req = new DeleteProfileRequest
        {
            user_id = UserData.GetUserId(),
            prof_id = profileId
        };

        yield return ApiConnect.Post<DeleteProfileRequest, GetProfileResponse>(
            "PHP_profile/delete_profile.php",
            req,
            (res) =>
            {
                if (!res.success)
                {
                    Debug.LogError("プロファイル削除失敗: " + res.message);
                    return;
                }

                LoadProfilesFromDB();   // プロファイル一覧を再取得して更新
            },
            error =>
            {
                Debug.LogError("プロファイル削除エラー: " + error);
            }
        );
    }

    public void RemoveProfileFromList(int profileId)
    {
        var target = Profiles.Find(p => p.profileId == profileId);
        if (target != null)
            Profiles.Remove(target);

        NotifyChanged();    // プロファイルリスト更新通知
    }

    // ==============================
    // プロファイル選択
    // ==============================
    public void SelectProfile(ProfileData data)
    {
        int newId = data.profileId;
        int oldId = UserData.GetUserCurrentProfId();

        if (newId == oldId)
        {
            // 同じプロファイルならDB更新は不要
            selectedProfileId = newId;
            NotifyChanged();
            ModelManager.Instance?.ShowModel(data.modelIndex);
            return;
        }

        selectedProfileId = newId;

        // **DB更新を呼ぶ前にUserDataに保存はしない**
        // StartCoroutine(UpdateCurrentProfileOnDB(selectedProfileId)) の中で更新後に保存する
        StartCoroutine(UpdateCurrentProfileOnDB(newId, data.modelIndex));
    }

    // 選択中プロファイルを取得
    public ProfileData GetSelectedProfile()
    {
        return Profiles.Find(p => p.profileId == selectedProfileId);
    }

    public int GetSelectedProfileId()
    {
        return selectedProfileId;
    }

    // ==============================
    // DBへ選択中プロファイルを保存
    // ==============================
    private IEnumerator UpdateCurrentProfileOnDB(int newProfileId, int modelIndex)
    {
        int oldProfileId = UserData.GetUserCurrentProfId(); // 変更前のIDをここで取得

        var req = new UserCurrentProfileRequest
        {
            user_id = UserData.GetUserId(),
            prof_id = newProfileId,
            current_prof_id = oldProfileId
        };

        yield return ApiConnect.Post<UserCurrentProfileRequest, GetProfileResponse>(
            "PHP_profile/change_profile.php",
            req,
            (res) =>
            {
                if (!res.success)
                {
                    Debug.LogError("現在のプロファイル更新失敗: " + res.message);
                    return;
                }

                // DB更新成功後にUserDataに保存
                UserData.SaveUserCurrentProfId(newProfileId);

                // モデル反映
                ModelManager.Instance?.ShowModel(modelIndex);

                LoadProfilesFromDB();
                NotifyChanged();
            },
            (error) =>
            {
                Debug.LogError("現在のプロファイル更新エラー: " + error);
            }
        );
    }
}