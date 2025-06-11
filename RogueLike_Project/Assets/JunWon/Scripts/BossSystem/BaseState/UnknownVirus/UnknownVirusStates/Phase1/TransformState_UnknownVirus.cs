using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnknownVirusBoss;

public class TransformState_UnknownVirus : BaseState_UnknownVirus
{
    private float startTime = 0f;
    private float transformationTime = 2.5f;
    private BossForm targetForm;
    private bool isTransforming = false;
    private bool hasTransformed = false;

    public TransformState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
        owner.SetTransformState(this);
    }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Transform State ����");
        startTime = Time.time;
        isTransforming = true;
        hasTransformed = false;

        if (owner.AbilityManager.UseAbility("Transform"))
        {
            owner.TRANSFORMDIRECTOR.SetTransformPattern(CubeTransformationDirector.TransformPattern.Implosion);
            owner.TRANSFORMDIRECTOR.SetTransformDuration(transformationTime);
            owner.TRANSFORMDIRECTOR.StartCubeTransformation();

            owner.ResetFormTimer();
            // �׻� Basic�� �ƴ� �ٸ� ������ ����
            targetForm = DecideNextForm();

            // ���� ��û - ����Ʈ Ȱ��ȭ ��
            owner.RequestFormChange(targetForm);

            Debug.Log($"[TransformState] {owner.CurrentForm} �� {targetForm} ������ ���� ����");
        }
    }

    public override void Update()
    {
        // ���� �ִϸ��̼� �Ϸ� üũ
        if (isTransforming && !hasTransformed && Time.time - startTime >= transformationTime)
        {
            CompleteTransformation();
        }
    }

    private void CompleteTransformation()
    {
        if (targetForm == BossForm.Basic) return;
        // �� ���� (���⼭ formTimer�� ������)
        owner.ApplyForm(targetForm);

        // ���� �Ϸ� ����
        isTransforming = false;
        hasTransformed = true;

        Debug.Log($"[TransformState] {targetForm} ������ ���� �Ϸ�");
        Debug.Log($"[TransformState] formTimer: {owner.GetFormTimer()}, ���ӽð�: {owner.GetStayDuration()}��");
    }

    public override void Exit()
    {
        if (targetForm == BossForm.Basic) return;

        // Transform State���� ���� ���� �׻� Basic���� ���ư�
        owner.ApplyForm(BossForm.Basic);
        owner.TRANSFORMDIRECTOR.RevertToOriginal();
        Debug.Log($"[TransformState] Exit - {owner.CurrentForm}���� Basic���� ����");
    }

    private BossForm DecideNextForm()
    {
        List<BossForm> availableForms = new List<BossForm>();

        if (owner.Worm != null)
            availableForms.Add(BossForm.Worm);
        if (owner.Troy != null)
            availableForms.Add(BossForm.Trojan);
        if (owner.Ransomware != null)
            availableForms.Add(BossForm.Ransomware);

        if (availableForms.Count == 0)
        {
            Debug.LogWarning("[TransformState] ��� ������ ���� ���� ���� - Basic ����");
            return BossForm.Basic;
        }

        return availableForms[UnityEngine.Random.Range(0, availableForms.Count)];
    }
}