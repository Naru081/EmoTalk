using UnityEngine;
using UnityEngine.UI;

public class ProfileController : MonoBehaviour
{
    public Image iconImage;
    public Text nameText;
    public Button button;

    private ProfileData data;
    private System.Action<ProfileData> onClick;

    public void Setup(ProfileData data, System.Action<ProfileData> onClick)
    {
        this.data = data;
        this.onClick = onClick;

        if (nameText != null)
            nameText.text = data.displayName;

        if (iconImage != null)
            iconImage.sprite = GetIconSprite(data.modelIndex);

        // ボタンのクリックイベントを登録（InspectorでOnClick設定しなくてOK）
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                onClick?.Invoke(this.data);
            });
        }
    }

    // ProfileEditWindow からも使いたいので static にしておく
    public static Sprite GetIconSprite(int modelIndex)
    {
        // 例：Resources/ModelIcons/model_0.png などから読み込む
        var sprite = Resources.Load<Sprite>($"ModelIcons/model_{modelIndex}");
        return sprite;
    }
    // プロファイル画面専用
    public static Sprite GetImgSprite(int modelIndex)
    {
        var sprite = Resources.Load<Sprite>($"ModelImgs/mimg_{modelIndex}");
        return sprite;
    }
}