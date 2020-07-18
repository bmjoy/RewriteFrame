using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEscWarShipContentPart : UIEscSecondPart
{
    protected override List<EscChooseInfo> GetEscChooseInfos()
    {
        return new List<EscChooseInfo>
        {
            new EscChooseInfo { Name = "esc_title_1013" ,IconId = 40213},
            new EscChooseInfo { Name = "esc_title_1014" ,IconId = 40214},
            new EscChooseInfo { Name = "esc_title_1015" ,IconId = 40215},
            new EscChooseInfo { Name = "esc_title_1016" ,IconId = 40216},
            new EscChooseInfo { Name = "esc_title_1017" ,IconId = 40217},
            new EscChooseInfo { Name = "esc_title_1018" ,IconId = 40218},

        };
    }

    protected override void Open()
    {
        UIManager.Instance.ClosePanel(UIPanel.EscWarShipPanel);
        switch (OwnerView.State.GetPageIndex())
        {
            case 0:
                break;
            case 1:              
                break;
            case 2:              
                break;
            case 3:
            case 4:
            case 5:
                break;
        }
    }
    protected override UIPanel GetPanelName()
    {
        return UIPanel.EscWarShipPanel;
    }
}
