using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MyPlayerController : PlayerController
{
    Camera cm;
    protected StatInfo Stat;

    private float _moveTime = 0.5f;
    protected override void Init()
    {
        base.Init();
        cm = Camera.main;
        cm.GetComponent<CameraController>().SettingPlayer(this);
        PrevPos = transform.position;
#if UNITY_SERVER

#else
        StopCoroutine(CheckPosInfo());
        StartCoroutine(CheckPosInfo());
#endif
    }
    protected override void Update()
    {
        OnClickMouseInputEvent();
        KeyInputEvent();
        base.Update();
    }

    private void KeyInputEvent()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (State != CreatureState.Idle || State == CreatureState.Moving || State == CreatureState.Skill || State == CreatureState.Dead || State == CreatureState.Wait) return;
            StartCoroutine(CoMakeSkillPacket(3));
        }
    }
    IEnumerator CoMakeSkillPacket(int skillId)
    {
        Skill skill = null;
        if (Managers.Data.SkillDict.TryGetValue(skillId, out skill) == false) yield break;
        for (int i = 0; i < skill.skillDatas.Count; i++)
        {
            C_MeleeAttack meleeAttack = new C_MeleeAttack() { Info = new SkillInfo(), Forward = new Positions() };
            meleeAttack.Info.SkillId = skillId;
            meleeAttack.Forward = Util.Vector3ToPositions(transform.forward);
            meleeAttack.Time = i;
            Managers.Network.Send(meleeAttack);
            State = CreatureState.Wait;
            if (i == skill.skillDatas.Count - 1) yield break;
            yield return new WaitForSeconds(skill.skillDatas[i].attackTime);
        }
    }
    public void OnClickMouseInputEvent()
    {
        _moveTime += Time.deltaTime;
        if (Input.GetMouseButton(1) && _moveTime >= 0.3f)
        {
            if (State == CreatureState.Skill || State == CreatureState.Dead || State == CreatureState.Wait) return;
            Ray ray = cm.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // TODO : �̵� ��Ŷ
                //MoveTarget(hit.point);
                C_Move movePacket = new C_Move() { PosInfo = new PositionInfo() { Pos = new Positions() } };
                movePacket.PosInfo.State = CreatureState.Moving;
                Positions pos = new Positions() { PosX = hit.point.x, PosY = hit.point.y, PosZ = hit.point.z };
                movePacket.PosInfo.Pos = pos;
                Managers.Network.Send(movePacket);
                _moveTime = 0;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (State == CreatureState.Skill || State == CreatureState.Dead || State == CreatureState.Wait) return;
            // idle �������� ���� -> �ƴ϶�� ���� ��Ŷ ���´ٰ� ���� ��Ŷ ����
            if (State != CreatureState.Idle && State == CreatureState.Moving)
            {
                C_StopMove moveStopPacket = new C_StopMove() { PosInfo = new PositionInfo() };
                moveStopPacket.PosInfo.Pos = new Positions() { PosX = transform.position.x, PosY = transform.position.y, PosZ = transform.position.z };
                Vector3 rotationEuler = transform.rotation.eulerAngles;
                moveStopPacket.PosInfo.Rotate = new RotateInfo() { RotateX = rotationEuler.x, RotateY = rotationEuler.y, RotateZ = rotationEuler.z };
                Managers.Network.Send(moveStopPacket);
                return;
            }
            // �۾�
            int attackRand = Random.Range(1, 3);
            if (attackRand < 0 || attackRand > 2) 
            {
                attackNum = -1; 
                return;
            }
            else
            {
                C_MeleeAttack meleeAttack = new C_MeleeAttack() { Info = new SkillInfo(), Forward = new Positions() };
                meleeAttack.Info.SkillId = attackRand;
                meleeAttack.Forward = Util.Vector3ToPositions(transform.forward);
                meleeAttack.Time = 0;
                Managers.Network.Send(meleeAttack);
                State = CreatureState.Wait;
            }
        }
    }

    public override IEnumerator CheckPosInfo() {
        while (true)
        {
            var offset = transform.position - PrevPos;
            if (offset.sqrMagnitude > 0.01f)
            {
                C_CheckPos checkPosPacket = new C_CheckPos() { CurPosInfo = new PositionInfo() };
                checkPosPacket.CurPosInfo.Pos = new Positions() { PosX = transform.position.x, PosY = transform.position.y, PosZ = transform.position.z };
                Vector3 rotationEuler = transform.rotation.eulerAngles;
                checkPosPacket.CurPosInfo.Rotate = new RotateInfo() { RotateX = rotationEuler.x, RotateY = rotationEuler.y, RotateZ = rotationEuler.z };
                Managers.Network.Send(checkPosPacket);
                PrevPos = transform.position;
            }
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }
}