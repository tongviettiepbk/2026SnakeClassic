using UnityEngine;

public class LLevelHard : BaseUI
{
    protected override void OnEnable()
    {
        this.StartDelayAction(timeClose, () => {
            Close();
        });
    }

    public override void Close()
    {
        if (isLoadFromResources)
        {
            UIManager.Instance.HideUI(this);
        }

        gameObject.SetActive(false);
    }

    

}
