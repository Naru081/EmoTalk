using UnityEngine;
using UnityEngine.UI;

// プロファイル一覧の各項目（行・セル）を制御するクラス
public class ProfileController : MonoBehaviour
{
    public Image iconImage; // モデルアイコン表示用
    public Text nameText;   // プロファイル名表示用
    public Button button;   // 選択ボタン

    private ProfileData data;   // プロファイルデータ
    private System.Action<ProfileData> onClick;  // クリック時のコールバック

    public Image background;    // 背景イメージ（選択状態表示用）

    // ==============================
    // プロファイル情報のセットアップ
    // ==============================
    public void Setup(ProfileData data, System.Action<ProfileData> onClick)
    {
        this.data = data;
        this.onClick = onClick;

        // 表示の更新
        nameText.text = data.displayName;
        iconImage.sprite = GetIconSprite(data.modelIndex);

        // 現在アプリで「選択中」のプロファイルIDと、この項目が持つIDが一致するかチェック
         var selected = ProfileManager.Instance != null
                   && ProfileManager.Instance.GetSelectedProfile() != null
                   && ProfileManager.Instance.GetSelectedProfile().profileId == data.profileId;

        // 選択されている場合は黄色っぽくハイライト、そうでなければ白（通常）
        if (background != null)
            background.color = selected ? new Color(1f, 1f, 0.5f, 1f) : Color.white;
        
        // ボタンのクリックイベント設定
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => this.onClick?.Invoke(this.data));
    }

    // ==============================
    // モデルのアイコン取得
    // ==============================
    public static Sprite GetIconSprite(int modelIndex)
    {
        // モデルアイコンの読み込み
        var sprite = Resources.Load<Sprite>($"ModelIcons/model_{modelIndex}");
        return sprite;
    }

    // ==============================
    // モデルの選択画像取得
    // ==============================
    public static Sprite GetImgSprite(int modelIndex)
    {
        // モデル選択画像の読み込み
        var sprite = Resources.Load<Sprite>($"ModelImgs/mimg_{modelIndex}");
        return sprite;
    }
}