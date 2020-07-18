using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEscRoleContentPart : UIEscSecondPart
{
    protected override List<EscChooseInfo> GetEscChooseInfos()
    {
        return new List<EscChooseInfo>
        {
            new EscChooseInfo { Name = "esc_title_1008" ,IconId = 40207},
            new EscChooseInfo { Name = "esc_title_1009" ,IconId = 40208},
            new EscChooseInfo { Name = "esc_title_1010" ,IconId = 40209},
            new EscChooseInfo { Name = "esc_title_1011" ,IconId = 40210},
            new EscChooseInfo { Name = "esc_title_1012" ,IconId = 40211},
            new EscChooseInfo { Name = "esc_title_1031" ,IconId = 40212},

        };
    }
    protected override void OnViewPartLoaded()
    {
        base.OnViewPartLoaded();
        for (int i = 0;i< m_Toggles.Length;i++)
        {
            TMP_Text m_UnOpen = m_Toggles[i].transform.Find("Content/Content/Label").GetComponent<TMP_Text>();
            m_UnOpen.gameObject.SetActive(true);
        }
    }

    protected override void SetHotKeyEnable(int index)
    {
        State.GetAction(UIAction.Common_Confirm).Enabled = false;
    }
    protected override void ToggleOnClick(bool select, Toggle toggle, int index)
    {
        base.ToggleOnClick(select, toggle, index);
        if (select)
        {
            m_Toggles[index].transform.Find("Content").GetComponent<Image>().raycastTarget = false;
        }
    }

    protected override void Open()
    {

        UIManager.Instance.ClosePanel(UIPanel.EscRolePanel);
        switch (OwnerView.State.GetPageIndex())
        {
            case 0:
                break;
            case 1:
                //UIManager.Instance.OpenPanel(UIPanel.TalentSciencePanel);
                break;
            case 2:
                //UIManager.Instance.OpenPanel(UIPanel.TalentPowerPanel);
                break;
            case 3:
            case 4:
            case 5:
                break;
        }
    }

    protected override UIPanel GetPanelName()
    {
        return UIPanel.EscRolePanel;
    }
}
