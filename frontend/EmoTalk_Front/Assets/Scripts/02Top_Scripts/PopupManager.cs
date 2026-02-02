using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private bool autoClose = true;
    [SerializeField] private float autoCloseSeconds = 3f;

    private void Awake()
    {
        if (root == null) root = gameObject;
        root.SetActive(false);
    }

    public void Open()
    {
        CancelInvoke();
        root.SetActive(true);

        // é©ìÆÇ≈ï¬Ç∂ÇÈê›íËÇ™óLå¯Ç»èÍçá
        if (autoClose)
            Invoke(nameof(Close), autoCloseSeconds);
    }

    public void Close()
    {
        CancelInvoke();
        root.SetActive(false);
    }
}
