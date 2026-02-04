using UnityEngine;

public class MiniGameWin : MonoBehaviour
{
    public LWin uiWinController;
    public Animator Animator;

    public void ChangeMuiltiGold(int muiltiTem)
    {
        uiWinController.UpdateMuiltiGold(muiltiTem);
    }

    public void StopGame()
    {
        DebugCustom.ShowDebugColorRed("Stop minigame");
        //Animator.enabled = false;
        Animator.speed = 0;
    }
}
