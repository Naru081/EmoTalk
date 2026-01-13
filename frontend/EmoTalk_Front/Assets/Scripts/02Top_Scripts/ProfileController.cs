using UnityEngine;
using UnityEngine.UI;

public class ProfileController : MonoBehaviour
{
    public Image iconImage;
    public Text nameText;
    public Button button;

    private ProfileData data;
    private System.Action<ProfileData> onClick;

    // 選択中のハイライト設定
    public Image background;

    // ==============================
    // プロファイル情報のセットアップ
    // ==============================
    public void Setup(ProfileData data, System.Action<ProfileData> onClick)
    {
        this.data = data;
        this.onClick = onClick;

        nameText.text = data.displayName;
        iconImage.sprite = GetIconSprite(data.modelIndex);

         var selected = ProfileManager.Instance != null
                   && ProfileManager.Instance.GetSelectedProfile() != null
                   && ProfileManager.Instance.GetSelectedProfile().profileId == data.profileId;

        if (background != null)
            background.color = selected ? new Color(1f, 1f, 0.5f, 1f) : Color.white;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => this.onClick?.Invoke(this.data));
    }

    // ==============================
    // モデルのアイコン取得
    // ==============================
    public static Sprite GetIconSprite(int modelIndex)
    {
        // 例：Resources/ModelIcons/model_0.png などから読み込む
        var sprite = Resources.Load<Sprite>($"ModelIcons/model_{modelIndex}");
        return sprite;
    }

    // ==============================
    // モデルの選択画像取得
    // ==============================
    public static Sprite GetImgSprite(int modelIndex)
    {
        var sprite = Resources.Load<Sprite>($"ModelImgs/mimg_{modelIndex}");
        return sprite;
    }
}