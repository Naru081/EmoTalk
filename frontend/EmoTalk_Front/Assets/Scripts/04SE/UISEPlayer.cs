using UnityEngine;

public class UISEPlayer : MonoBehaviour
{
    public AudioClip se;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = FindObjectOfType<SEManager>()?.GetComponent<AudioSource>();
    }

    public void Play()
    {
        if (se == null || audioSource == null) return;
        audioSource.PlayOneShot(se);
    }
}
