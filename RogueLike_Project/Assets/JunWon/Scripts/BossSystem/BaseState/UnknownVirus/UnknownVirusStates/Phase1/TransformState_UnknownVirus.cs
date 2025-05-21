using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnknownVirusBoss;

public class TransformState_UnknownVirus : BaseState_UnknownVirus
{
    private float startTime = 0f;
    private float transformationTime = 1.0f;
    private BossForm targetForm;

    bool isTransforming = false;

    public TransformState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
        owner.SetTransformState(this);
    }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Transform State ����");
        startTime = Time.time;
        isTransforming = true;

        // ���� �� ���� (���� Basic�̸� �ٸ� ������, �ƴϸ� Basic����)
        if (owner.CurrentForm == BossForm.Basic)
            targetForm = DecideNextForm();

        // ���� ��û - ����Ʈ Ȱ��ȭ ��
        owner.RequestFormChange(targetForm);

        Debug.Log($"[TransformState] {targetForm} ������ ���� ����");
    }

    public override void Update()
    {
        // ���� �Ϸ�
        if (isTransforming && Time.time - startTime >= transformationTime)
        {
            CompleteTransformation();
        }
    }

    private void CompleteTransformation()
    {
        // �� ����
        owner.ApplyForm(targetForm);

        // ���� �Ϸ� ����
        isTransforming = false;

        Debug.Log($"[TransformState] {targetForm} ������ ���� �Ϸ�");
    }
    public override void Exit()
    {
        targetForm = BossForm.Basic;
        CompleteTransformation();
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
            return BossForm.Basic;

        return availableForms[UnityEngine.Random.Range(0, availableForms.Count)];
    }
}