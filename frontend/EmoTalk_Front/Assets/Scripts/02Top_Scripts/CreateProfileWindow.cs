using UnityEngine;

public class CreateProfileWindow : MonoBehaviour
{
    public GameObject window; // "Window" をアタッチ

    private int selectedModelIndex = 0;

    public void Open()
    {
        selectedModelIndex = 0;  // 初期は model_0
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    // Modelボタンから呼ぶ
    public void SelectModel(int index)
    {
        selectedModelIndex = index;
        Debug.Log("選択したモデル: " + index);
    }

    // 保存画面に進む
    public void OnCreate()
    {
        // 名前・性格などの編集画面を開く
        // 作成したい ProfileData を保持して渡す
        // → 後で ProfileManager に登録する
    }
}