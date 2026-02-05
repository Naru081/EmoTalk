using UnityEngine;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework.Expression;
using System.Collections;

public class Live2DExpressionController : MonoBehaviour
{
    private CubismExpressionController expressionController;
    private CubismModel model;

    private Coroutine currentRoutine; // �� �ǉ��F�������Ă���R���[�`�����L�^
    private Animator animator;

    void Awake()
    {
        expressionController = GetComponent<CubismExpressionController>()
                             ?? GetComponentInParent<CubismExpressionController>();

        model = GetComponent<CubismModel>()
              ?? GetComponentInParent<CubismModel>();

        animator = GetComponent<Animator>();
    }

    public void ReturnToNatural()
    {
        StartCoroutine(ReturnToNaturalRoutine());
    }

    private IEnumerator ReturnToNaturalRoutine()
    {
        // �@ ���݂̕\��������I��
        expressionController.CurrentExpressionIndex = -1;

        // �A 2�t���[���ҋ@�iSDK�̓����o�b�t�@���N���A���邽�߁j
        yield return null;
        yield return null;

        // �B ���f���́u���݂̒l�v�ł͂Ȃ��u�f�t�H���g�l�v��S�p�����[�^�ɋ����K�p
        foreach (var p in model.Parameters)
        {
            p.Value = p.DefaultValue;
        }

        // �C e_Param54 �����͔O����� 0 �ɒ@������
        foreach (var p in model.Parameters)
        {
            if (p.Id == "e_Param54") p.Value = 0f;
        }

        yield return null;

        // �D �Ō�� natural ��K�p
        int naturalIndex = GetIndex("natural");
        expressionController.CurrentExpressionIndex = naturalIndex;
        expressionController.CurrentExpressionIndex = naturalIndex;

        expressionController.CurrentExpressionIndex = naturalIndex;
        expressionController.CurrentExpressionIndex = naturalIndex;



        Debug.Log("����ȏ�Ȃ����x���ŏ��������� natural ��K�p���܂����B");
    }
    // ���ׂĂ̕\��ύX�͂����ʂ�
    public void SetExpression(string emotion)
    {
        StartSingleRoutine(SetExpressionRoutine(emotion));
    }

    // �R���[�`����1�ɐ���
    private void StartSingleRoutine(IEnumerator routine)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(routine);
    }

    private IEnumerator SetExpressionRoutine(string emotion)
    {
        // �@ ���݂�Expression������
        expressionController.CurrentExpressionIndex = -1;
        yield return null;

        // �A �y�d�v�z���f���̑S�p�����[�^���u�f�t�H���g�l�v�ɋ������Z�b�g
        // ����ɂ��A�O�̕\��iHappy���j�̎c�[�����S�ɏ������܂�
        foreach (var parameter in model.Parameters)
        {
            parameter.Value = parameter.DefaultValue;
        }

        // �B �O�̂���1�t���[���ҋ@
        yield return null;

        // �C �V���������K�p
        int targetIndex = GetIndex(emotion);
        expressionController.CurrentExpressionIndex = targetIndex;

        Debug.Log($"�������Z�b�g���A�V�����K�p���܂���: {emotion} (Index: {targetIndex})");

        currentRoutine = null;
    }

    int GetIndex(string emotion)
    {
        switch (emotion)
        {
            case "happy": return 1;
            case "angry": return 2;
            case "sad": return 3;
            case "natural":
            default: return 0;
        }
    }
}
