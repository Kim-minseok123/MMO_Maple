using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Stat : UI_Base
{
    MyPlayerController myPlayer;
    bool isInit = false;
    enum Texts
    {
        PlayerStatNameText,
        AttackText,
        RemainPointText,
        StrText,
        DexText,
        LukText,
        IntText,   
        HpText,
        MpText,
    }
    enum Buttons
    {
        ExitButton,
        StrUpButton,
        DexUpButton,
        LukUpButton,
        IntUpButton
    }
    public override void Init()
    {
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.ExitButton).gameObject.BindEvent((e) => { var ui = Managers.UI.SceneUI as UI_GameScene; ui.CloseUI("UI_Stat"); });

        GetButton((int)Buttons.StrUpButton).gameObject.BindEvent((e) => { MakeChagneStatPacket("Str"); });
        GetButton((int)Buttons.DexUpButton).gameObject.BindEvent((e) => { MakeChagneStatPacket("Dex"); });
        GetButton((int)Buttons.LukUpButton).gameObject.BindEvent((e) => { MakeChagneStatPacket("Luk"); });
        GetButton((int)Buttons.IntUpButton).gameObject.BindEvent((e) => { MakeChagneStatPacket("Int"); });

        myPlayer = Managers.Object.MyPlayer;

        isInit = true;

        RefreshUI();
    }
    void MakeChagneStatPacket(string statString)
    {
        if (myPlayer.Stat.StatPoint <= 0) return;
        C_ChangeStat changeStatPacket = new C_ChangeStat();
        changeStatPacket.ChangeStat = statString;
        Managers.Network.Send(changeStatPacket);
    }
    public void RefreshUI()
    {
        if (isInit == false) return;
        GetText((int)Texts.PlayerStatNameText).text = $"�г��� : <color=#F3E3AE>{myPlayer.objectInfo.Name}</color>";
        GetText((int)Texts.HpText).text = $"HP\t: <color=#F3E3AE>{myPlayer.Hp}</color> / <color=#F3E3AE>{myPlayer.MaxHp}</color>";
        GetText((int)Texts.MpText).text = $"MP\t: <color=#F3E3AE>{myPlayer.Mp}</color> / <color=#F3E3AE>{myPlayer.MaxMp}</color>";
        if(myPlayer.BuffDamage > 0)
            GetText((int)Texts.AttackText).text = $"���ݷ�\t: <color=red>{myPlayer.MinAttack}</color> ~ <color=red>{myPlayer.MaxAttack}</color>";
        else
            GetText((int)Texts.AttackText).text = $"���ݷ�\t: <color=#F3E3AE>{myPlayer.MinAttack}</color> ~ <color=#F3E3AE>{myPlayer.MaxAttack}</color>";
        GetText((int)Texts.RemainPointText).text = $"���� ���� ����Ʈ : <color=#F3E3AE>{myPlayer.Stat.StatPoint}</color>";
        GetText((int)Texts.StrText).text = $"��\t: <color=#F3E3AE>{myPlayer.Stat.Str}</color>";
        GetText((int)Texts.DexText).text = $"��ø\t: <color=#F3E3AE>{myPlayer.Stat.Dex}</color>";
        GetText((int)Texts.LukText).text = $"���\t: <color=#F3E3AE>{myPlayer.Stat.Luk}</color>";
        GetText((int)Texts.IntText).text = $"����\t: <color=#F3E3AE>{myPlayer.Stat.Int}</color>";

    }
}
