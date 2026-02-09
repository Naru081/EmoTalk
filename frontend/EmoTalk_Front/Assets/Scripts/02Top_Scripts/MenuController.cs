using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ハンバーガーメニュー制御
public class MenuController : MonoBehaviour
{
    [Header("UI")]
    public GameObject menuMask;         // メニューの背景マスク
    public RectTransform menuPanel;     // メニューの本体

    [Header("Hamburger Button")]
    public Button btnMenu;            // ハンバーガーボタン
    public Image btnMenuImage;        // メニュー展開ボタン
    public Sprite iconMenu;           // 通常のアイコン
    public Sprite iconClose;          // 展開中のアイコン
    public LogoutPopup logoutPopup;   // ログアウトボタン

    [Header("Animation")]
    public float slideDuration = 0.25f;      // スライド速度
    public float maskFadeDuration = 0.25f;   // フェード速度

    private float panelWidth;
    private Vector2 panelHiddenPos;
    private Vector2 panelShownPos;

    // メニューが開いているかどうか
    private bool isOpen = false;

    [Header("ProfileListWindow")]
    // メニュー内のプロフィールリストウィンドウ
    public ProfileListWindow listWindow;

    // ==============================
    // ハンバーガーメニュー制御
    // ==============================
    void Start()
    {
        // パネル幅
        panelWidth = menuPanel.rect.width;

        // スライド位置（左外 / 画面内）
        panelHiddenPos = new Vector2(-panelWidth, 0);
        panelShownPos  = new Vector2(0, 0);

        // 初期状態
        menuPanel.anchoredPosition = panelHiddenPos;
        menuMask.SetActive(false);
        isOpen = false;
        btnMenuImage.sprite = iconMenu;     // 最初は三本線

        // ボタンイベント
        btnMenu.onClick.AddListener(ToggleMenu);

        // 背景クリックで閉じる
        Button maskBtn = menuMask.AddComponent<Button>();
        maskBtn.transition = Selectable.Transition.None;
        maskBtn.onClick.AddListener(CloseMenu);
    }

    // ==============================
    // メニューボタン（三本線/×）が押されたとき
    // ==============================
    public void ToggleMenu()
    {
        bool willOpen = !isOpen;   // このあと「開くのか/閉じるのか」

        // これからの状態に合わせて画像を切り替え
        btnMenuImage.sprite = willOpen ? iconClose : iconMenu;

        if (willOpen)
            OpenMenu();
        else
            CloseMenu();
    }

    // ==============================
    // メニューを開く処理
    // ==============================
    public void OpenMenu()
    {
        if (isOpen) return;
        isOpen = true;

        // 念のためここでもアイコンを保証
        btnMenuImage.sprite = iconClose;

        menuMask.SetActive(true);
        listWindow.RefreshList();

        // フェードイン
        StartCoroutine(FadeMask(0f, 0.6f, maskFadeDuration));
        // スライドイン
        StartCoroutine(Slide(menuPanel, panelHiddenPos, panelShownPos, slideDuration));
    }

    // ==============================
    // メニューを閉じる処理
    // ==============================
    public void CloseMenu()
    {
        if (!isOpen) return;
        isOpen = false;

        // 閉じた状態のアイコン（三本線）に戻す
        btnMenuImage.sprite = iconMenu;

        // フェードアウト
        StartCoroutine(FadeMask(0.6f, 0f, maskFadeDuration, onComplete: () =>
        {
            menuMask.SetActive(false);
        }));

        // スライドアウト
        StartCoroutine(Slide(menuPanel, panelShownPos, panelHiddenPos, slideDuration));
    }
    // コルーチン：スライドアニメーション
    IEnumerator Slide(RectTransform target, Vector2 from, Vector2 to, float time)
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            float rate = Mathf.Clamp01(t / time);
            target.anchoredPosition = Vector2.Lerp(from, to, rate);
            yield return null;
        }
        target.anchoredPosition = to;
    }
    // コルーチン：マスクのフェードアニメーション
    IEnumerator FadeMask(float from, float to, float time, System.Action onComplete = null)
    {
        float t = 0;
        Image maskImage = menuMask.GetComponent<Image>();

        while (t < time)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / time);
            maskImage.color = new Color(0, 0, 0, a);
            yield return null;
        }

        maskImage.color = new Color(0, 0, 0, to);
        onComplete?.Invoke();
    }

    // ==============================
    // モデル選択
    // ==============================
    public void OnClickModel1()
    {
        ChangeModel(0);
    }

    public void OnClickModel2()
    {
        ChangeModel(1);
    }

    public void OnClickModel3()
    {
        ChangeModel(2);
    }

    // ==============================
    // モデル変更共通処理
    // ==============================
    private void ChangeModel(int index)
    {
        if (ModelManager.Instance != null)
        {
            ModelManager.Instance.ShowModel(index);
        }
        else
        {
            Debug.LogWarning("MenuController: ModelManager.Instance が見つかりません。");
        }

        // モデルを選んだらメニューを閉じる
        CloseMenu();
    }

    // ==============================
    // ログアウト
    // ==============================
    public void OnClickLogout()
    {
        logoutPopup.Open();
    }
}