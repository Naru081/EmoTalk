using UnityEngine;

public class CreateProfileModelSelect : MonoBehaviour
{
    public CreateProfileInputPanel inputPanel; // CreatePanel_P を指定

    // ==============================
    // モデル選択画面(新規)を開く
    // ==============================
    public void Open()
    {
        gameObject.SetActive(true);
    }

    // ==============================
    // モデル選択画面(新規)を閉じる
    // ==============================
    public void Close()
    {
        gameObject.SetActive(false);
    }

    // ==============================
    // モデルを選択したときの処理
    // ==============================
    public void OnSelectModel(int index)
    {
        Close();                      // 自分を閉じる
        inputPanel.Open(index);       // 入力画面へ
    }
}