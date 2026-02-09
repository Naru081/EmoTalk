using UnityEngine;

// プロファイル新規制作のウィンドウ
public class CreateProfileWindow : MonoBehaviour
{
    // ウィンドウ本体
    public GameObject window;

    // ユーザーが現在選んでいるモデルの番号
    private int selectedModelIndex = 0;

    // ==============================
    // ウィンドウを開く
    // ==============================
    public void Open()
    {
        selectedModelIndex = 0;  // 初期は model_0
        gameObject.SetActive(true);
    }

    // ==============================
    // ウィンドウを閉じる
    // ==============================
    public void Close()
    {
        gameObject.SetActive(false);
    }

    // ==============================
    // モデルを選択したときの処理
    // ==============================
    public void SelectModel(int index)
    {
        selectedModelIndex = index;
        Debug.Log("選択したモデル: " + index);
    }

    // ==============================
    // 保存画面に進む
    // ==============================
    public void OnCreate()
    {
        // 名前・性格などの編集画面を開く
        // 作成したい ProfileData を保持して渡す
        // → 後で ProfileManager に登録する
    }
}