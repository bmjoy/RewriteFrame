using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEscPermitContentPart : UIEscSecondPart
{
    protected override List<EscChooseInfo> GetEscChooseInfos()
    {
        return new List<EscChooseInfo>
        {
            new EscChooseInfo { Name = "esc_title_1025" ,IconId = 40219},
            new EscChooseInfo { Name = "esc_title_1026" ,IconId = 40220},
            new EscChooseInfo { Name = "esc_title_1027" ,IconId = 40221},
            new EscChooseInfo { Name = "esc_title_1028" ,IconId = 40222},
            new EscChooseInfo { Name = "esc_title_1029" ,IconId = 40223},
            new EscChooseInfo { Name = "esc_title_1030" ,IconId = 40224},

        };
    }

    protected override void Open()
    {
        UIManager.Instance.ClosePanel(UIPanel.EscPermitPanel);
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
        return UIPanel.EscPermitPanel;
    }
}
