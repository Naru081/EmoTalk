using UnityEngine;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework.Expression;
using System.Collections;

// Live2Dモデルの表情を制御するスクリプト
public class Live2DExpressionController : MonoBehaviour
{
    // Live2Dモデルと表情コントローラーの参照
    private CubismExpressionController expressionController;
    private CubismModel model;

    // 現在実行中のコールチン参照
    private Coroutine currentRoutine;

    // Animatorへの参照
    private Animator animator;

    // ==============================
    // 初期化
    // ==============================
    void Awake()
    {
        // 現在のオブジェクトから表情コントローラーを取得
        expressionController = GetComponent<CubismExpressionController>()
                             ?? GetComponentInParent<CubismExpressionController>();

        // 現在のオブジェクトからLive2Dモデルを取得
        model = GetComponent<CubismModel>()
              ?? GetComponentInParent<CubismModel>();

        // Animatorコンポーネントを取得
        animator = GetComponent<Animator>();
    }

    // ==============================
    // 表情を初期値に戻す
    // ==============================
    public void ReturnToNatural()
    {
        // 全ての表情変更はこれを通す
        StartCoroutine(ReturnToNaturalRoutine());
    }

    // ==============================
    // 表情を初期値に戻すコルーチン
    // ==============================
    private IEnumerator ReturnToNaturalRoutine()
    {
        // 現在もの表情を強制解除
        expressionController.CurrentExpressionIndex = -1;

        // ２フレーム待機
        // CUMISM SDKの内部バッファをクリアするため
        yield return null;
        yield return null;

        // モデルのデフォルト値を全パラメータに強制適用
        foreach (var p in model.Parameters)
        {
            p.Value = p.DefaultValue;
        }

        // 特定のパラメータだけには個別に'0'を設定して確実に初期化する
        foreach (var p in model.Parameters)
        {
            if (p.Id == "e_Param54") p.Value = 0f;  // 特定のパラメータ
        }

        // 1フレーム待機
        // パラメータ変更を反映させるため
        yield return null;

        // 感情タグを適用するためのインデックスを取得
        int naturalIndex = GetIndex("natural"); // 取得するインデックス

        // 1フレームでの競合を避けるため・SDKの更新タイミングを考慮して
        // 複数回代入して確実に　"natural"を適用
        expressionController.CurrentExpressionIndex = naturalIndex;
        expressionController.CurrentExpressionIndex = naturalIndex;
        expressionController.CurrentExpressionIndex = naturalIndex;
        expressionController.CurrentExpressionIndex = naturalIndex;

        Debug.Log("naturalを適用しました。");
    }

    // ==============================
    // 表情変更を変更させる
    // ==============================
    public void SetExpression(string emotion)
    {
        // 既存の処理を停止させ、新しい表情設定コルーチンを開始する
        StartSingleRoutine(SetExpressionRoutine(emotion));
    }

    // ==============================
    // 実行中のコルーチンを１つに管理・制御する
    // ==============================
    private void StartSingleRoutine(IEnumerator routine)    // 実行したいコルーチン
    {
        // 既に別の表情切り替えが動いている場合は、重複を避けるため停止
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        // 新しいコルーチンを変数に保持しながら開始
        currentRoutine = StartCoroutine(routine);
    }

    // ==============================
    // 表情を設定するメインコルーチン
    // ==============================
    private IEnumerator SetExpressionRoutine(string emotion)
    {
        // 現在適用されているExpressionを一度解除してリセットする
        expressionController.CurrentExpressionIndex = -1;
        yield return null;

        // モデルの全てのパラメータをデフォルト値に強制リセット
        foreach (var parameter in model.Parameters)
        {
            parameter.Value = parameter.DefaultValue;
        }

        // 書き換えを確定させるため１フレーム待機
        yield return null;

        // 指定された感情に対応するインデックスを取得
        int targetIndex = GetIndex(emotion);
        // 指定した表情を適用
        expressionController.CurrentExpressionIndex = targetIndex;

        Debug.Log($"表情をリセットし、新しく適用しました: {emotion} (Index: {targetIndex})");

        // コルーチン参照クリア
        currentRoutine = null;
    }

    // 感情の文字列から、Expressionのインデックス番号を返す
    int GetIndex(string emotion)    // 判定対象の感情の名前
    {
        switch (emotion)
        {
            case "happy": return 1;
            case "angry": return 2;
            case "sad": return 3;
            case "natural":     //見つからない場合はnatural(0)を返す
            default: return 0;
        }
    }
}
