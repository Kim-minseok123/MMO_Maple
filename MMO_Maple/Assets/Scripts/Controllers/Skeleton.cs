using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : MonsterController
{
    protected override void Init()
    {
        base.Init();
        isAttackMotion = false;
    }

    public override void OnAttack(SkillInfo info)
    {
        Skill skill = null;
        if (Managers.Data.SkillDict.TryGetValue(info.SkillId, out skill) == false) return;
        transform.LookAt(TargetObj.transform);
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;
        State = CreatureState.Skill;
        _anim.SetTrigger("Attack");
        StartCoroutine(CoAttackPacket(skill));
        State = CreatureState.Idle;
    }

    public IEnumerator CoAttackPacket(Skill skill)
    {
        yield return new WaitForSeconds(skill.skillDatas[0].attackTime);
#if UNITY_SERVER
        if(State == CreatureState.Skill)
        {
            C_MeleeAttack meleeAttack = new C_MeleeAttack() { Info = new SkillInfo(), Forward = new Positions() };
            meleeAttack.Info.SkillId = skill.id;
            meleeAttack.Forward = Util.Vector3ToPositions(transform.forward);
            meleeAttack.IsMonster = true;
            meleeAttack.Time = 0;
            meleeAttack.ObjectId = Id;
            Managers.Network.Send(meleeAttack);
        }
#endif
        yield return new WaitForSeconds(skill.cooldown - (int)skill.skillDatas[0].attackTime);
        isAttackMotion = false;
        
    }

    public override IEnumerator OnMove(Vector3 target)
    {
        if (isAttackMotion) yield break;
        _agent.ResetPath();
        if (TargetObj != null && Vector3.Distance(target, transform.position) >= 1.2f)
            _agent.SetDestination(target);
        State = CreatureState.Moving;

        while (true)
        {
            if (TargetObj == null)
            {

                if (Vector3.Distance(_agent.destination, transform.position) < 0.3f)
                {
#if UNITY_SERVER
                    C_StopMove moveStopPacket = new C_StopMove() { PosInfo = new PositionInfo() };
                    moveStopPacket.PosInfo.Pos = new Positions() { PosX = transform.position.x, PosY = transform.position.y, PosZ = transform.position.z };
                    Vector3 rotationEuler = transform.rotation.eulerAngles;
                    moveStopPacket.PosInfo.Rotate = new RotateInfo() { RotateX = rotationEuler.x, RotateY = rotationEuler.y, RotateZ = rotationEuler.z };
                    moveStopPacket.IsMonster = true;
                    moveStopPacket.ObjectId = Id;
                    Managers.Network.Send(moveStopPacket);
                    break;
#else
                    break;
#endif
                }

            }
                else
            {
                if (Vector3.Distance(_agent.destination, transform.position) < 1.2f)
                {
#if UNITY_SERVER
                    transform.LookAt(TargetObj.transform);
                    C_SkillMotion skillMotion = new C_SkillMotion() { Info = new SkillInfo() };
                    skillMotion.ObjectId = Id;
                    skillMotion.Info.SkillId = 4;
                    skillMotion.IsMonster = true;
                    Managers.Network.Send(skillMotion);
                    isAttackMotion = true;
                    break;
#else
                    isAttackMotion = true;
                    break;
#endif
                }
            }
            yield return null;
        }

        yield return null;
    }
}
