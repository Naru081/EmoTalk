using UnityEngine;

public class DeleteConfirmDialog : MonoBehaviour
{
    private System.Action confirmAction;

    public void Open(System.Action confirmAction)
    {
        this.confirmAction = confirmAction;
        gameObject.SetActive(true);
    }

    public void OnConfirm()
    {
        confirmAction?.Invoke();
        Close();
    }

    public void OnCancel()
    {
        Close();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}