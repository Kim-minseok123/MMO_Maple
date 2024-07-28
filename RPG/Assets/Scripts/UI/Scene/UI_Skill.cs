using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Skill : UI_Base
{
    enum Buttons
    {
        ExitButton
    }
    enum GameObjects
    {
        Content
    }
    enum Texts
    {
        SkillPointText,
        SkillTitleText,
    }
    enum Images
    {
        ClassLogoImage
    }
    bool _init = false;
    public List<UI_SkillInfo> Skills { get; } = new List<UI_SkillInfo>();
    public GameObject grid;

    public Sprite Beginner;
    public Sprite Warrior;
    public Sprite Archer;
    public override void Init()
    {
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetButton((int)Buttons.ExitButton).gameObject.BindEvent((e) => { 
            var ui = Managers.UI.SceneUI as UI_GameScene; ui.CloseUI("UI_Skill"); });

        _init = true;
        RefreshUI();
    }
    public void RefreshUI()
    {
        if (_init == false)
            return;
        Skills.Clear();
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        MyPlayerController myPlayer = Managers.Object.MyPlayer;
        if (myPlayer == null) return;
        

        if (myPlayer.ClassType == (int)ClassTypes.Beginner)
        {
            for (int i = 0; i < Managers.Data.BeginnerSkillData.Count; i++)
            {
                GameObject go = Managers.Resource.Instantiate("UI/SubItem/UI_SkillInfo", grid.transform);
                UI_SkillInfo skillInfo = go.GetOrAddComponent<UI_SkillInfo>();
                skillInfo.Setting(Managers.Data.BeginnerSkillData[i]);
                skillInfo.RefreshUI();
                Skills.Add(skillInfo);
            }
        }
        GetText((int)Texts.SkillPointText).text = myPlayer.Stat.SkillPoint.ToString();
        switch (myPlayer.ClassType)
        {
            case (int)ClassTypes.Beginner:
                GetText((int)Texts.SkillTitleText).text = "�ʺ��� ���� ���̵�";
                GetImage((int)Images.ClassLogoImage).sprite = Beginner;
                break;
            case (int)ClassTypes.Warrior:
                GetText((int)Texts.SkillTitleText).text = "���� �Թ���";
                GetImage((int)Images.ClassLogoImage).sprite = Warrior;
                break;
            case (int)ClassTypes.Archer:
                GetText((int)Texts.SkillTitleText).text = "�ü� �Թ���";
                GetImage((int)Images.ClassLogoImage).sprite = Archer;
                break;
        }
    }
    public override void InfoRemove()
    {
        foreach (var skill in Skills)
        {
            skill.InfoRemoveSkill();
        }
    }
}
