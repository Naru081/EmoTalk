using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TopController : MonoBehaviour
{
    // -------------------------
    // パネル開閉（既存）
    // -------------------------
    [Header("Log Panel")]
    public RectTransform logPanel;
    public Button handleButton;

    [Header("Positions")]
    public float openX = 0f;
    public float closeX = -1028f;
    private bool isOpen = false;
    private float slideSpeed = 10f;

    // -------------------------
    // ここからチャットログ追加
    // -------------------------
    [Header("Chat Log")]
    public Transform logContent;
    public GameObject logItemUserPrefab;    
    public GameObject logItemEmoPrefab;      
    public RectTransform contentTransform;
     public ScrollRect scrollRect; 

    [Header("Input UI")]
    public InputField chatInput;
    public Button sendButton;

    void Start()
    {
        // パネル開閉
        handleButton.onClick.RemoveAllListeners();
        handleButton.onClick.AddListener(ToggleLogPanel);

        // 初期位置（閉じる）
        Vector3 pos = logPanel.localPosition;
        pos.x = closeX;
        logPanel.localPosition = pos;

        // 送信処理
        sendButton.onClick.RemoveAllListeners();
        sendButton.onClick.AddListener(OnSendMessage);
    }

    void Update()
    {
        float targetX = isOpen ? openX : closeX;
        Vector3 pos = logPanel.localPosition;
        pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * slideSpeed);
        logPanel.localPosition = pos;
    }

    public void ToggleLogPanel()
    {
        isOpen = !isOpen;
    }

    // -------------------------
    // 改行の処理
    // -------------------------
    private string InsertLineBreaks(string text, int maxCharsPerLine = 12)
    {
        if (string.IsNullOrEmpty(text)) return text;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        int count = 0;
        foreach (char c in text)
        {
            sb.Append(c);
            count++;

            if (count >= maxCharsPerLine)
            {
                sb.Append("\n");
                count = 0;
            }
        }

        return sb.ToString();
    }

    // -------------------------
    // 送信ボタンの処理
    // -------------------------
    public void OnSendMessage()
    {
        string msg = chatInput.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        // ▼ 改行処理を先に行う
        msg = InsertLineBreaks(msg, 20);

        // ▼ 自分の吹き出しを追加
        AddLogItem(msg, true);

        // 入力欄クリア
        chatInput.text = "";

        // ▼ テスト返信
        StartCoroutine(DebugAutoReply());
    }


    IEnumerator DebugAutoReply()
    {
        yield return new WaitForSeconds(0.5f);
        AddLogItem("了解です！", false);
        ScrollToBottom();
    }

    // -------------------------
    // 吹き出し生成
    // -------------------------
    public void AddLogItem(string message, bool isUser)
    {
        GameObject prefab = isUser ? logItemUserPrefab : logItemEmoPrefab;

        GameObject item = Instantiate(prefab, logContent);
        Text text = item.GetComponentInChildren<Text>();
        text.text = message;

        LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);

    }

    // -------------------------
    // スクロールを一番下に移動
    // -------------------------
    public void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;  
        Canvas.ForceUpdateCanvases();
    }
}