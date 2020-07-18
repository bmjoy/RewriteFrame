using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEscSettingContentPart : UIEscSecondPart
{
    protected override List<EscChooseInfo> GetEscChooseInfos()
    {
        return new List<EscChooseInfo>
        {
            new EscChooseInfo { Name = "esc_title_1019" ,IconId = 40225},
            new EscChooseInfo { Name = "esc_title_1020" ,IconId = 40226},
            new EscChooseInfo { Name = "esc_title_1021" ,IconId = 40227},
            new EscChooseInfo { Name = "esc_title_1022" ,IconId = 40228},
            new EscChooseInfo { Name = "esc_title_1023" ,IconId = 40229},
            new EscChooseInfo { Name = "esc_title_1024" ,IconId = 40230},

        };
    }

    protected override void Open()
    {
        UIManager.Instance.ClosePanel(UIPanel.EscSettingPanel);
        switch (OwnerView.State.GetPageIndex())
        {
            case 0:
            case 1:      
            case 2:
            case 3:
            case 4:
            case 5:
                break;
        }
    }

    protected override UIPanel GetPanelName()
    {
        return UIPanel.EscSettingPanel;
    }
}
