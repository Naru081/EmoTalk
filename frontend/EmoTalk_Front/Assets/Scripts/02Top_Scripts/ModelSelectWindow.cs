using UnityEngine;

// モデルの一覧を表示し、ユーザーが選んだインデックスを呼び出し元へ返却する汎用ウィンドウクラス
public class ModelSelectWindow : MonoBehaviour
{
    // 現在編集中のプロファイルデータ（必要に応じて参照・比較用）
    private ProfileData currentProfile;
    
    // モデルが選択された際に実行する外部メソッドの保持用（int型のインデックスを引数に取る）
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
        // 保持していたコールバック（デリゲート）を実行し、選択された番号を呼び出し元へ通知する
        onSelected?.Invoke(index);
        Close();    // ウィンドウを閉じる
    }
}