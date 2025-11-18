using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    // あとでinputFieldをここに紐付けてAPIに投げるように改良します。

    // ボタンから呼ぶ用のメゾット
    public void GoToTop()
    {
        // ログイン処理をここに追加します。

        // TOP画面に遷移
        SceneManager.LoadScene("TopScene");
    }

    public void GoToLogin()
    {
        // ログイン画面に遷移
        SceneManager.LoadScene("LoginScene");
    }
}