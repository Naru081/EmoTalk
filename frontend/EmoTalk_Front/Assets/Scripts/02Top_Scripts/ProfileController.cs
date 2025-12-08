using UnityEngine;
using UnityEngine.UI;

public class ProfileItemController : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;   // キャラ画像
    public Text nameText;     // モデル名
    public Button button;     // タップ全体

    private ProfileData profileData;
    private System.Action<ProfileData> onClickCallback;

    public void Setup(ProfileData data, System.Action<ProfileData> onClick)
    {
        profileData = data;
        onClickCallback = onClick;

        // 名前を表示
        nameText.text = data.displayName;

        // モデルの画像設定（modelIndex に応じて差し替え）
        iconImage.sprite = GetIconSprite(data.modelIndex);

        // ボタン押したらコールバック
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback?.Invoke(profileData));
    }

    // モデルのアバター画像を返す処理（必要に応じて差し替える）
    private Sprite GetIconSprite(int index)
    {
        // ★ 必要ならここにモデルごとのアイコンを入れる
        // 今は仮で Resources から読み込む例にしておきます

        return Resources.Load<Sprite>($"ModelIcons/model_{index}");
    }
}