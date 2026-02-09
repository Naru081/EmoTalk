using UnityEngine;

// 新規プロファイル作成：モデル選択画面の制御
public class CreateProfileModelSelect : MonoBehaviour
{
    // プロファイルを入力する画面の参照
    public CreateProfileInputPanel inputPanel;

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
        Close();                      // 閉じる
        inputPanel.Open(index);       // 入力画面へ
    }
}