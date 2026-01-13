using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class TopController : MonoBehaviour
{
    // 会話ログパネル
    [Header("Log Panel")]
    public RectTransform logPanel;
    public Button handleButton;

    // パネル位置設定
    [Header("Positions")]
    public float openX = 0f;
    public float closeX = -1028f;
    private bool isOpen = false;
    private float slideSpeed = 10f;

    // スワイプ用の変数を追加
    private bool isDragging = false;
    private Vector2 dragStartPos;
    private float panelStartX;
    [Range(0.1f, 0.9f)]
    public float openThreshold = 0.5f;   // 開き具合が何割以上なら「開く」とみなすか

    // チャットログ追加
    [Header("Chat Log")]
    public Transform logContent;
    public GameObject logItemUserPrefab;
    public GameObject logItemEmoPrefab;
    public RectTransform contentTransform;
    public ScrollRect scrollRect;

    // チャット入力UI
    [Header("Input UI")]
    public InputField chatInput;
    public Button sendButton;

    // ==============================
    // ★ ここだけ追加：PHP送信用
    // ==============================
    [Header("Server (PHP)")]
    [Tooltip("例: http://localhost/control_message.php  /  実機なら http://PCのIP/control_message.php")]
    public string serverUrl = "http://172.20.10.6/backend/PHP_message/control_message.php";

    [Tooltip("通信失敗時に従来のテスト返信を出す（デバッグ用）")]
    public bool fallbackToDebugReply = true;

    // ==============================
    // TOP画面UI制御
    // ==============================
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

    // ==============================
    // 毎フレーム更新
    // ==============================
    void Update()
    {
        // ---- ドラッグしていないときだけ、Lerp でスムーズに目標位置へ ----
        if (!isDragging)
        {
            float targetX = isOpen ? openX : closeX;
            Vector3 pos = logPanel.localPosition;
            pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * slideSpeed);
            logPanel.localPosition = pos;
        }

        // ---- iPhone コントロールセンター風のドラッグ処理 ----
        DetectDrag();
    }

    // ==============================
    // 会話ログパネル開閉処理
    // ==============================
    public void ToggleLogPanel()
    {
        if (isOpen)
        {
            // スワイプ中断
            isDragging = false;

            // アニメーションに任せて閉じる
            isOpen = false;
        }
    }

    // ==============================
    // 改行の処理
    // ==============================
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

    // ==============================
    // 送信ボタンの処理
    // ==============================
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

        // ▼ ここだけ変更：PHPへ送信
        StartCoroutine(SendToPhpAndReceiveReply(msg));
    }

    // ==============================
    // ★ 追加：PHPへ送って返信を表示
    // ==============================
    private IEnumerator SendToPhpAndReceiveReply(string userMessage)
    {
        if (string.IsNullOrEmpty(serverUrl))
        {
            Debug.LogWarning("serverUrl is empty. Fallback reply.");
            if (fallbackToDebugReply) yield return DebugAutoReply();
            yield break;
        }

        // prof_idを取得
        int prof_id = UserData.GetUserCurrentProfId();

        // control_message.php に投げるPOST（必要に応じて項目を増やしてOK）
        WWWForm form = new WWWForm();
        form.AddField("message_content", userMessage);
        form.AddField("prof_id", prof_id);

        using (UnityWebRequest req = UnityWebRequest.Post(serverUrl, form))
        {
            // タイムアウト（任意）
            req.timeout = 20;

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("PHP request failed: " + req.error);

                // 通信失敗でもデモが止まらないように
                if (fallbackToDebugReply)
                    yield return DebugAutoReply();

                yield break;
            }

            // 返信本文（PHP側がJSONなら、ここでJSON解析に変える）
            string responseText = req.downloadHandler.text;

            // 空ならフォールバック
            if (string.IsNullOrWhiteSpace(responseText))
            {
                Debug.LogWarning("PHP response is empty.");
                if (fallbackToDebugReply) yield return DebugAutoReply();
                yield break;
            }

            // ▼ 相手（Emo）の吹き出し追加
            AddLogItem(responseText, false);
            ScrollToBottom();
        }
    }

    // ==============================
    // テスト返信（デバッグ用）※元のまま残す
    // ==============================
    IEnumerator DebugAutoReply()
    {
        yield return new WaitForSeconds(0.5f);
        AddLogItem("了解です！", false);
        ScrollToBottom();
    }

    // ==============================
    // 吹き出し生成
    // ==============================
    public void AddLogItem(string message, bool isUser)
    {
        GameObject prefab = isUser ? logItemUserPrefab : logItemEmoPrefab;

        GameObject item = Instantiate(prefab, logContent);
        Text text = item.GetComponentInChildren<Text>();
        text.text = message;

        LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
    }

    // ==============================
    // 会話更新時スクロールを一番下に移動
    // ==============================
    public void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    // =====================================
    // iPhone風スワイプ開閉処理（元のまま）
    // =====================================
    void DetectDrag()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // --- マウス操作（エディタ確認用） ---
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragStartPos = Input.mousePosition;
            panelStartX = logPanel.localPosition.x;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            float diffX = Input.mousePosition.x - dragStartPos.x;
            float newX = Mathf.Clamp(panelStartX + diffX, closeX, openX);
            logPanel.localPosition = new Vector3(newX, logPanel.localPosition.y, logPanel.localPosition.z);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag();
        }
#else
        // --- タッチ操作（スマホ実機） ---
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                isDragging = true;
                dragStartPos = t.position;
                panelStartX = logPanel.localPosition.x;
            }
            else if (t.phase == TouchPhase.Moved && isDragging)
            {
                float diffX = t.position.x - dragStartPos.x;
                float newX = Mathf.Clamp(panelStartX + diffX, closeX, openX);
                logPanel.localPosition = new Vector3(newX, logPanel.localPosition.y, logPanel.localPosition.z);
            }
            else if ((t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) && isDragging)
            {
                EndDrag();
            }
        }
#endif
    }

    // ==============================
    // ドラッグ終了時の処理
    // ==============================
    void EndDrag()
    {
        isDragging = false;

        float currentX = logPanel.localPosition.x;
        float width = openX - closeX;

        // closeX〜openX を 0〜1 に正規化
        float ratio = (currentX - closeX) / width;

        // 指を離した位置が「一定以上右に出ていれば」開く
        isOpen = (ratio >= openThreshold);
    }
}