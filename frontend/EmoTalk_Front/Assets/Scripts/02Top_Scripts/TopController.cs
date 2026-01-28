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

    private Base64WavPlayer wavPlayer;

    void Awake()
    {
        wavPlayer = FindObjectOfType<Base64WavPlayer>();
    }

    // ==============================
    // PHP送信用
    // ==============================
    [Header("Server (PHP)")]
    [Tooltip("例: http://localhost/control_message.php  /  実機なら http://PCのIP/control_message.php")]
    public string serverUrl = "http://172.20.10.6/backend/PHP_message/control_message.php";

    // ngrok http 80で起動したURLを指定すること
    //public string serverUrl = "http://ernestine-geoidal-gaynelle.ngrok-free.dev/backend/PHP_message/control_message.php";

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
    // 送信ボタンの処理
    // ==============================
    public void OnSendMessage()
    {
        string msg = chatInput.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;
        string rawMsg = msg;

        // ▼ 自分の吹き出しを追加
        AddLogItem(msg, true);

        // 入力欄クリア
        chatInput.text = "";

        // AIに送信
        StartCoroutine(SendMessageToAI(rawMsg));
    }

    // ==============================
    // 音声入力からの送信処理
    // ==============================
    public void SendMessageFromVoice(string rawText)
    {
        if (string.IsNullOrEmpty(rawText)) return;

        // 表示用改行
        //string displayText = InsertLineBreaks(rawText, 20);

        // ▼ ユーザの吹き出しを追加
        AddLogItem(rawText, true);
        ScrollToBottom();

        // ChatGPT APIに送信
        StartCoroutine(SendMessageToAI(rawText));
    }

    // ==============================
    // ChatGPTへメッセージ送信処理
    // ==============================
    private IEnumerator SendMessageToAI(string messageContent)
    {
        int profileId = ProfileManager.Instance != null ? ProfileManager.Instance.GetSelectedProfileId() : -1;
        if (profileId <= 0)
        {
            Debug.LogError("プロファイルが選択されていません。");
            AddLogItem("プロファイルが選択されていません。", false);
            ScrollToBottom();
            yield break;
        }

        var req = new MessageRequest
        {
            prof_id = profileId,
            message_content = messageContent
        };

        yield return ApiConnect.Post<MessageRequest, MessageResponse>(
            "PHP_api/chatgpt_api.php",
            req,
            (res) =>
            {
                if (!res.success)
                {
                    Debug.LogError("メッセージ送信失敗: " + res.message);
                    AddLogItem("メッセージ送信失敗: " + res.message, false);
                    ScrollToBottom();
                    return;
                }

                string responseText = string.IsNullOrEmpty(res.response_text)
                    ? "返答が取得できませんでした。"
                    : res.response_text;

                AddLogItem(responseText, false);
                ScrollToBottom();

                string responseText_hiragana = string.IsNullOrEmpty(res.response_text_hiragana)
                    ? "返答が取得できませんでした"
                    : res.response_text_hiragana;

                string emotion = res.emotion;
                Debug.Log("感情タグ: " + emotion);

                string model_voice = res.model_voice;
                 Debug.Log("モデル音声: " + model_voice);

                // CoeiroInkに送信
                StartCoroutine(RequestCoeiroInk(model_voice, responseText_hiragana));
            },
            error =>
            {
                Debug.LogError("メッセージ送信エラー: " + error);
                AddLogItem("通信エラーが発生しました。", false);
                ScrollToBottom();
            }
        );
    }

    // ==============================
    // CoeiroInkへ音声生成リクエスト及び再生処理
    // ==============================
    private IEnumerator RequestCoeiroInk(string model_voice, string responseText_hiragana)
    {
        yield return ApiConnect.Post<CoeiroInkRequest, CoeiroInkResponse>(
            "PHP_api/coeiroink_api.php",
            new CoeiroInkRequest
            {
                model_voice = model_voice,
                responseText_hiragana = responseText_hiragana
            },
            (res) =>
            {
                if (!res.success)
                {
                    Debug.LogError("CoeiroInkリクエスト失敗: " + res.message);
                    //Debug.Log("model_voice: " + model_voice);
                    return;
                }
                // 音声データのbase64文字列を取得
                string voiceBase64 = res.voice_wav_base64;
                Debug.Log("CoeiroInk音声データ取得成功" + res.success);
                // CoeiroInkからのキャラクターボイス再生
                var model = ModelManager.Instance != null ? ModelManager.Instance.CurrentModel : null;
                if (model == null)
                {
                    Debug.LogError("モデルがまだ生成されていません（ModelManager.CurrentModel が null）");
                    return;
                }

                var player = model.GetComponent<Base64WavPlayer>();
                if (player == null)
                {
                    Debug.LogError("モデルPrefabのルートに Base64WavPlayer が付いていません");
                    return;
                }
                player.PlayFromBase64(voiceBase64);
            },
            error =>
            {
                Debug.LogError("CoeiroInkリクエストエラー: " + error);
            }
        );
    }

    // ==============================
    // テスト返信（デバッグ用）※元のまま残す
    // ==============================
    // IEnumerator DebugAutoReply()
    // {
    //     yield return new WaitForSeconds(0.5f);
    //     AddLogItem("了解です！", false);
    //     ScrollToBottom();
    // }

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