using UnityEngine;

// ボタンクリック時などのUI操作に合わせて個別のSEを再生するクラス
public class UISEPlayer : MonoBehaviour
{
    public AudioClip se;    // inspectorからコンポーネント専用に割り当てる効果音ファイル

    private AudioSource audioSource;    // SEManagerが持つ共通のAudioSourceを参照するための変数

    // ==============================
    // シーン内の　SEManager　を探し、それが持つ　AudioSource　を取得
    // ==============================
    private void Awake()
    {
        audioSource = FindObjectOfType<SEManager>()?.GetComponent<AudioSource>();
    }

    // ==============================
    // ボタンのクリック時のイベント
    // ==============================
    public void Play()
    {
        if (se == null || audioSource == null) return;
        audioSource.PlayOneShot(se);
    }
}
