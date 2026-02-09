using UnityEngine;

// SEを管理するクラス
public class SEManager : MonoBehaviour
{
    public static SEManager Instance;   // 外部からでもアクセスできる静的変数

    [Header("System SE")]
    public AudioClip whisperSuccessSE;  // Wisper成功時に再生するオーディオクリップ
    public AudioClip whisperErrorSE;    // Wisper失敗時に再生するオーディオクリップ

    [Header("Login SE")]
    public AudioClip loginSuccessSE;    // ログイン成功時に再生するオーディオクリップ

    private AudioSource audioSource;    // 音声を実際に再生するコンポーネント


    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            // このオブジェクトを唯一のインスタンスとして設定
            Instance = this;
            // シーンを切り替えてもこのオブジェクトが削除されないように設定
            DontDestroyOnLoad(gameObject);
            // 自身にアタッチされている　AudioSource　を取得して保持
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            // 重複を避けるため自信を破棄
            Destroy(gameObject);
        }
    }

    // ==============================
    // SEを再生する共通の内部ロジック
    // ==============================
    public void Play(AudioClip clip)
    {
        // クリップが設定されていな場合
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    // ==============================
    // ログイン成功時
    // ==============================
    public void PlayLoginSuccess()
    {
        Play(loginSuccessSE);
    }

    // ==============================
    // 音声認識が成功したとき
    // ==============================
    public void PlayWhisperSuccess()
    {
        Play(whisperSuccessSE);
    }

    // ==============================
    // 音声認識が失敗したとき
    // ==============================
    public void PlayWhisperError()
    {
        Play(whisperErrorSE);
    }
}
