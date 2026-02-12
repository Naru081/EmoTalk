using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using TMPro;

// TOP画面のUI制御を行うクラス
public class TopController : MonoBehaviour
{
    [Header("Log Panel")]
    public RectTransform logPanel;  // チャットログパネル
    public Button handleButton;     // ハンドルボタン

    // パネル位置設定
    [Header("Positions")]
    public float openX = 0f;        // 開いたときのX位置
    public float closeX = -1028f;   // 閉じたときのX位置
    private bool isOpen = false;    // パネル開閉状態
    private float slideSpeed = 10f; // スライド速度

    // スワイプ用の変数を追加
    private bool isDragging = false;    // ドラッグ中フラグ
    private Vector2 dragStartPos;       // ドラッグ開始位置
    private float panelStartX;          // パネルの開始X位置
    [Range(0.1f, 0.9f)]
    public float openThreshold = 0.5f;   // 開き具合が何割以上なら「開く」とみなすか

    // 追加：handle内判定用
    private Canvas uiCanvas;
    private RectTransform handleRect;


    // チャットログ追加
    [Header("Chat Log")]
    public Transform logContent;        // チャットログコンテンツの親
    public GameObject logItemUserPrefab;    // ユーザ吹き出しPrefab
    public GameObject logItemEmoPrefab;     // AI吹き出しPrefab
    public RectTransform contentTransform;
    public ScrollRect scrollRect;

    // チャット入力UI
    [Header("Input UI")]
    public InputField chatInput;    // チャット入力欄
    public Button sendButton;       // 送信ボタン

    // 会話のネーム表示用
    [Header("Chat Log Name")]
    public GameObject fnameUserPrefab;   // ユーザ名Prefab
    public GameObject fnameAiPrefab;    // AI名Prefab

    [Header("Chat Log Spacing")]
    [Tooltip("同じ話者が連続する時の間隔")]
    public float sameSpeakerGap = 12f;      // 同じ話者が連続する時の間隔

    [Tooltip("話者が切り替わる時の間隔")]
    public float changeSpeakerGap = 28f;    // 話者が切り替わる時の間隔


    // 直前の話者情報
    private bool hasLastSpeaker = false;    // 最初の1個目かどうか
    private bool lastSpeakerIsUser = false; // 直前の話者がユーザかどうか



    // 音声再生用
    private Base64WavPlayer wavPlayer;

    // 音声再生中フラグ
    private bool isVoicePlaying = false;

    // 入力受付禁止ポップアップ
    [Header("Popups")]
    public PopupManager maskSendPopup;

    [Header("Log Wrap")]
    public float forceWrapWidth; // 強制改行幅（TMP用）　

    // ポップアップを次フレームで開閉
    IEnumerator OpenNextFrame(PopupManager popup)
    {
        yield return null; // 次のフレームまで待機
        popup.Open();
    }

    // ポップアップを次フレームで閉じる
    IEnumerator CloseNextFrame(PopupManager popup)
    {
        yield return null; // 次のフレームまで待機
        popup.Close();
    }

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
    // public string serverUrl = "http://ernestine-geoidal-gaynelle.ngrok-free.dev/backend/PHP_message/control_message.php";

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

        // handleのRectとCanvas参照を取る
        handleRect = handleButton != null ? handleButton.GetComponent<RectTransform>() : null;
        uiCanvas = handleButton != null ? handleButton.GetComponentInParent<Canvas>() : null;
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

        // AIに送信と同時に入力受付を禁止にする
        maskSendPopup.Open();
        StartCoroutine(OpenNextFrame(maskSendPopup));

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

        // AIに送信と同時に入力受付を禁止にする
        maskSendPopup.Open();
        StartCoroutine(OpenNextFrame(maskSendPopup));

        // ChatGPT APIに送信
        StartCoroutine(SendMessageToAI(rawText));
    }

    // ==============================
    // ChatGPTへメッセージ送信処理
    // ==============================
    private IEnumerator SendMessageToAI(string messageContent)
    {
        // 選択中のプロファイルIDを取得
        int profileId = ProfileManager.Instance != null ? ProfileManager.Instance.GetSelectedProfileId() : -1;
        if (profileId <= 0)
        {
            Debug.LogError("プロファイルが選択されていません。");
            AddLogItem("プロファイルが選択されていません。", false);
            ScrollToBottom();
            yield break;
        }

        // APIリクエスト作成
        var req = new MessageRequest
        {
            prof_id = profileId,
            message_content = messageContent
        };
        // API送信
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

                var model = ModelManager.Instance.CurrentModel;

                string model_voice = res.model_voice;
                Debug.Log("モデル音声: " + model_voice);

                // CoeiroInkに送信
                StartCoroutine(RequestCoeiroInk(model_voice, responseText_hiragana, emotion));
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
    private IEnumerator RequestCoeiroInk(string model_voice, string responseText_hiragana, string emotion)
    {
        // CoeiroInk API送信
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

                var exp = model.GetComponent<Live2DExpressionController>();
                Live2DExpressionController expLocal = exp; // ローカルコピー

                player.OnVoiceFinished = () =>
                {
                    Debug.Log("表情を元に戻す");
                    isVoicePlaying = false;
                    exp.ReturnToNatural();

                    maskSendPopup.Open();
                    StartCoroutine(CloseNextFrame(maskSendPopup));
                };
                // 音声再生開始とともに表情を切り替え

                if (exp != null && !isVoicePlaying)
                {
                    exp.SetExpression(emotion); // 感情タグに基づいて表情変更
                }

                isVoicePlaying = true;
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
        // 直前の話者との関係で、先頭にスペースを入れる（見やすさ）
        float gap = sameSpeakerGap;

        if (hasLastSpeaker && lastSpeakerIsUser != isUser)
        {
            gap = changeSpeakerGap; // 話者が切り替わったら広め
        }

        if (hasLastSpeaker) // 最初の1個目は不要なら入れない
        {
            CreateSpacer(gap);
        }

        // 名前（Fname_user / Fname_ai）を生成
        GameObject namePrefab = isUser ? fnameUserPrefab : fnameAiPrefab;
        if (namePrefab != null)
        {
            Instantiate(namePrefab, logContent);
        }
        else
        {
            Debug.LogWarning("Fname prefab が未設定です（fnameUserPrefab / fnameAiPrefab）。");
        }

        // 吹き出し（Logitem_User / Logitem_Emo）を生成
        GameObject prefab = isUser ? logItemUserPrefab : logItemEmoPrefab;
        GameObject item = Instantiate(prefab, logContent);

        TMP_Text tmpText = item.GetComponentInChildren<TMP_Text>(true);
        if (tmpText == null)
        {
            Debug.LogError("ログアイテムPrefab内に TMP_Text が見つかりません。");
            return;
        }

        // テキストを入れる
        tmpText.text = message;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();

        float maxWidth = forceWrapWidth;
        Debug.Log($"[LogWrap] maxWidth(forced)={maxWidth}");
        tmpText.text = ApplyAutoLineBreakTMP(tmpText, message, maxWidth);

        LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);

        // 状態更新
        hasLastSpeaker = true;
        lastSpeakerIsUser = isUser;
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
    // ドラッグ位置の判定
    // =====================================
    private bool IsPointerOnHandle(Vector2 screenPos)
    {
        if (handleRect == null) return false;

        Camera cam = null;
        if (uiCanvas != null && uiCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = uiCanvas.worldCamera;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(handleRect, screenPos, cam);
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
            Vector2 pos = Input.mousePosition;

            // ★追加：handle内で押してないならドラッグ開始しない
            if (!IsPointerOnHandle(pos)) return;

            isDragging = true;
            dragStartPos = pos;
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
                // ★追加：handle内で触ってないならドラッグ開始しない
                if (!IsPointerOnHandle(t.position)) return;

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
        float targetX = isOpen ? openX : closeX;
        logPanel.localPosition = new Vector3(targetX, logPanel.localPosition.y, logPanel.localPosition.z);
    }

    // ==============================
    // TextMeshPro用自動改行処理
    // ==============================
    private string ApplyAutoLineBreakTMP(TMP_Text tmp, string src, float maxWidth)
    {
        if (string.IsNullOrEmpty(src)) return src;
        if (maxWidth <= 0.01f) return src; // 幅が取れないならそのまま

        // 既存の改行は一旦統一（ブログでも Parse 時に \n を外していたのと同じ意図） :contentReference[oaicite:1]{index=1}
        src = src.Replace("\r\n", "\n");

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        System.Text.StringBuilder line = new System.Text.StringBuilder();

        // 文字単位で積む（MeCabなしの簡易版：依存を増やさない）
        for (int i = 0; i < src.Length; i++)
        {
            char c = src[i];

            // 元々の改行は維持
            if (c == '\n')
            {
                builder.Append(line.ToString());
                builder.Append('\n');
                line.Clear();
                continue;
            }

            // 次の1文字を足した時に横幅に収まるかを計測
            string candidate = line.ToString() + c;
            Vector2 preferred = tmp.GetPreferredValues(candidate, maxWidth, 0f);

            if (preferred.x <= maxWidth || line.Length == 0)
            {
                line.Append(c);
            }
            else
            {
                // 収まらないので改行
                builder.Append(line.ToString());
                builder.Append('\n');
                line.Clear();
                line.Append(c);
            }
        }

        builder.Append(line.ToString());
        return builder.ToString();
    }

    // ==============================
    // ログ間スペーサー生成
    // ==============================
    private void CreateSpacer(float height)
    {
        GameObject spacer = new GameObject("LogSpacer", typeof(RectTransform), typeof(LayoutElement));
        spacer.transform.SetParent(logContent, false);

        LayoutElement le = spacer.GetComponent<LayoutElement>();
        le.minHeight = height;
        le.preferredHeight = height;
        le.flexibleHeight = 0f;
    }
}