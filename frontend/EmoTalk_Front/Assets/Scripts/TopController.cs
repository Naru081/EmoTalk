using UnityEngine;
using UnityEngine.UI;

public class TopController : MonoBehaviour
{
    [Header("Log Panel")]
    public RectTransform logPanel;
    public Button handleButton;

    [Header("Positions")]
    public float openX = 0f;          // 開く時のX位置
    public float closeX = -1028f;     // 閉じる時のX位置（あなたのUIに合わせて調整）

    private bool isOpen = false;
    private float slideSpeed = 10f;

    void Start()
    {
        // ハンドルボタンにイベント追加（二重登録されないように Clear → Add）
        handleButton.onClick.RemoveAllListeners();
        handleButton.onClick.AddListener(ToggleLogPanel);

        // 最初は閉じた状態で開始
        Vector3 pos = logPanel.localPosition;
        pos.x = closeX;
        logPanel.localPosition = pos;
        isOpen = false;
    }

    void Update()
    {
        // スライドアニメーション
        float targetX = isOpen ? openX : closeX;
        Vector3 pos = logPanel.localPosition;
        pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * slideSpeed);
        logPanel.localPosition = pos;
    }

    //　OnClick で呼ばれる関数
    public void ToggleLogPanel()
    {
        isOpen = !isOpen;
    }
}