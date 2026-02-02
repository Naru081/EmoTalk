using UnityEngine;

public class SEManager : MonoBehaviour
{
    public static SEManager Instance;

    [Header("System SE")]
    public AudioClip whisperSuccessSE;
    public AudioClip whisperErrorSE;

    [Header("Login SE")]
    public AudioClip loginSuccessSE;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    public void PlayLoginSuccess()
    {
        Play(loginSuccessSE);
    }

    public void PlayWhisperSuccess()
    {
        Play(whisperSuccessSE);
    }

    public void PlayWhisperError()
    {
        Play(whisperErrorSE);
    }
}
