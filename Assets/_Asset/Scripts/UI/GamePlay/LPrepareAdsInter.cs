using System;
using UnityEngine;

public class LPrepareAdsInter : BaseUI
{
    public void PrepareShowInter()
    {
        this.StartDelayAction(GameConfig.timeWaitPopupShowInter, () =>
        {
            Close();
        });
    }
}
