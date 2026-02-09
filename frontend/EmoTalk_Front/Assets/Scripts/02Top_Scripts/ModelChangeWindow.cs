using UnityEngine;

// 既存プロファイルの Live2D モデル（アバター）を変更するためのウィンドウを制御するクラス
public class ModelChangeWindow : MonoBehaviour
{
    public GameObject root;                 // パネルのルートオブジェクト
    public ProfileEditWindow editWindow;    // 編集画面への参照

    private ProfileData targetProfile;      // モデル変更対象のプロファイルデータ

    // ==============================
    // モデル変更ウィンドウの表示
    // ==============================
    public void Open(ProfileData profile)
    {
        targetProfile = profile;    // 対象プロファイルをセット
        if (root != null) root.SetActive(true);
        else gameObject.SetActive(true);
    }

    // ==============================
    // モデル変更ウィンドウの非表示
    // ==============================
    public void Close()
    {
        if (root != null) root.SetActive(false);
        else gameObject.SetActive(false);
    }
    
    // ==============================
    // モデル選択ボタンから呼ばれる
    // ==============================
    public void SelectModel(int modelIndex)
    {
        // UI上のアバター画像ボタン（引数に modelIndex）が押された際の処理
        if (targetProfile == null) return;

        // 保持しているプロファイルデータのモデル番号を書き換え
        targetProfile.modelIndex = modelIndex;

        // データベース（サーバー/ローカル保存）へ変更を同期
        // ProfileManager を介して永続化を行う
        ProfileManager.Instance.UpdateProfileModel(targetProfile);

        // 呼び出し元の編集画面（ProfileEditWindow）の見た目を最新の状態に反映
        if (editWindow != null)
            editWindow.RefreshCurrentView();

        // 他のUIコンポーネント（サイドメニューのリストなど）へ変更を通知
        ProfileManager.Instance.NotifyChanged();

        // ウィンドウを閉じる
        Close();
    }
}