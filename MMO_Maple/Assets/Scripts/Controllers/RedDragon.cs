using DG.Tweening;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedDragon : MonsterController
{
    UI_BossHp_Popup hpbarUI;
    protected override void Init()
    {
        hpbarUI = Managers.Resource.Instantiate("UI/Popup/UI_BossHp_Popup").GetComponent<UI_BossHp_Popup>();
        hpbarUI.Setting(gameObject);
        hpbarUI.ChangeHp(MaxHp);
        _anim = GetComponent<Animator>();
        _anim.SetInteger("ActionNum", -1);
    }
    public override void ChangeHp(int hp, bool isHeal, int damage)
    {
        if (isHeal)
        {
        }
        else
        {
            if (damage <= 0)
            {
                GameObject damageInfo = Managers.Resource.Instantiate("Effect/DamageInfo");
                damageInfo.transform.position = transform.position;
                damageInfo.GetComponent<UI_DamageInfo_Item>().Setting(damage);
            }
            else
            {
               
                // ü�¹� �� �۾�
                Hp = hp;
                var value = Mathf.Max(0f, (float)Hp / MaxHp);
                if (value == 0f)
                {
                    // ���� ó��
                }
                else
                {
                    hpbarUI.ChangeHp(value);
                }

                GameObject damageInfo = Managers.Resource.Instantiate("Effect/DamageInfo");
                damageInfo.transform.position = transform.position + new Vector3(0, 3f, 0);
                damageInfo.GetComponent<UI_DamageInfo_Item>().Setting(damage);
            }
        }
    }
    public override void OnAttack(SkillInfo info)
    {
        int actionNum = info.SkillId;

        StartCoroutine(CoChagneAnimNum(actionNum));
    }
    public IEnumerator CoChagneAnimNum(int actionNum)
    {
        _anim.SetInteger("ActionNum", actionNum);
        yield return new WaitForSeconds(0.5f);
        _anim.SetInteger("ActionNum", -1);
    }
}
