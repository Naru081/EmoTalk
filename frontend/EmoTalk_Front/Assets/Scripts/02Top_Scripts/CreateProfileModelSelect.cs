using UnityEngine;

public class CreateProfileModelSelect : MonoBehaviour
{
    public CreateProfileInputPanel inputPanel; // CreatePanel_P を指定

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    // モデルを選択したときに呼ぶ（0,1,2）
    public void OnSelectModel(int index)
    {
        Close();                      // 自分を閉じる
        inputPanel.Open(index);       // 入力画面へ
    }
}