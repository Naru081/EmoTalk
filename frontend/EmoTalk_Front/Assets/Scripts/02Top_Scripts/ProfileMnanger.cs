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

    // 警告文ポップアップ
    [SerializeField] private PopupManager successSavePopup;
    [SerializeField] private PopupManager warningTitlePopup;
    [SerializeField] private PopupManager warningCharaPopup;
    [SerializeField] private PopupManager warningTonePopup;
    [SerializeField] private PopupManager warningFpPopup;
    [SerializeField] private PopupManager maskSendPopup;

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

    // ユーザの新規登録後、選択中のプロファイルを強調表示する
    public void SyncSelectedProfileFromUserData()
    {
        selectedProfileId = UserData.GetUserCurrentProfId();
        NotifyChanged();
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
        SyncSelectedProfileFromUserData();
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
        if (string.IsNullOrEmpty(data.displayName))
        {
            // タイトルが空の場合は警告ポップアップを表示して終了
            warningTitlePopup.Open();
            StartCoroutine(OpenNextFrame(warningTitlePopup));
            return;
        }

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

                    if (res.message == "タイトルは10文字以内で入力してください")
                    {
                        warningTitlePopup.Open();
                        StartCoroutine(OpenNextFrame(warningTitlePopup));
                    }
                    return;
                }
                // 成功時
                LoadProfilesFromDB();

                // 保存成功ポップアップ表示
                successSavePopup.Open();
                StartCoroutine(OpenNextFrame(successSavePopup));
            }
        );
    }

    // プロファイルの性格・口調・一人称を変更
    public void UpdateProfileCustom(ProfileData data, Action onSuccess)
    {
        //Debug.Log("UpdateProfileCustom 呼び出し完了");

        StartCoroutine(UpdateProfileCustomCoroutine(data, onSuccess));
    }

    private IEnumerator UpdateProfileCustomCoroutine(ProfileData data, Action onSuccess)
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

                    if (res.message == "性格は20文字以内で入力してください")
                    {
                        warningCharaPopup.Open();
                        StartCoroutine(OpenNextFrame(warningCharaPopup));
                    }
                    else if (res.message == "口調は20文字以内で入力してください")
                    {
                        warningTonePopup.Open();
                        StartCoroutine(OpenNextFrame(warningTonePopup));
                    }
                    else if (res.message == "一人称は20文字以内で入力してください")
                    {
                        warningFpPopup.Open();
                        StartCoroutine(OpenNextFrame(warningFpPopup));
                    }
                    return;
                }
                // 成功時
                LoadProfilesFromDB();  // プロファイル一覧を再取得して更新

                // 保存成功ポップアップ表示
                successSavePopup.Open();
                StartCoroutine(OpenNextFrame(successSavePopup));

                onSuccess?.Invoke();   // 成功通知コールバック
            },
            error => Debug.LogError("プロファイルカスタム更新エラー: " + error)

        );
    }

    // ポップアップを正しく表示するために1フレーム待つ
    IEnumerator OpenNextFrame(PopupManager popup)
    {
        yield return null; // 1フレーム待つ
        popup.Open();
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

    // ==============================
    // ポップアップ表示
    // ==============================

    // 保存成功ポップアップ表示
    public void ShowSuccessSavePopup()
    {
        if (successSavePopup == null)
        {
            Debug.LogWarning("successSavePopup が未設定です");
            return;
        }
        successSavePopup.Open();
        StartCoroutine(OpenNextFrame(successSavePopup));
    }

    // タイトル警告ポップアップ表示
    public void ShowWarningTitlePopup()
    {
        if (warningTitlePopup == null)
        {
            Debug.LogWarning("warningTitlePopup が未設定です");
            return;
        }

        warningTitlePopup.Open();
        StartCoroutine(OpenNextFrame(warningTitlePopup));
    }

    // 性格警告ポップアップ表示
    public void ShowWarningCharaPopup()
    {
        if (warningCharaPopup == null)
        {
            Debug.LogWarning("warningCharaPopup が未設定です");
            return;
        }
        warningCharaPopup.Open();
        StartCoroutine(OpenNextFrame(warningCharaPopup));
    }

    // 口調警告ポップアップ表示
    public void ShowWarningTonePopup()
    {
        if (warningTonePopup == null)
        {
            Debug.LogWarning("warningTonePopup が未設定です");
            return;
        }
        warningTonePopup.Open();
        StartCoroutine(OpenNextFrame(warningTonePopup));
    }

    // 一人称警告ポップアップ表示
    public void ShowWarningFpPopup()
    {
        if (warningFpPopup == null)
        {
            Debug.LogWarning("warningFpPopup が未設定です");
            return;
        }
        warningFpPopup.Open();
        StartCoroutine(OpenNextFrame(warningFpPopup));
    }

    // チャット機能処理中の入力受付禁止マスクポップアップ表示
    public void ShowMaskSendPopup()
    {
        if (maskSendPopup == null)
        {
            Debug.LogWarning("maskSendPopup が未設定です");
            return;
        }
        maskSendPopup.Open();
        StartCoroutine(OpenNextFrame(maskSendPopup));
    }
}