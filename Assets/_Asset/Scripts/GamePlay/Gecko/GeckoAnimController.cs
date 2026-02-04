using System.Collections.Generic;
using UnityEngine;

public class GeckoAnimController : MonoBehaviour
{
    public List<GameObject> objEysStun;

    private Animator aniController;

    private string stun = "Stun";

    //public animationControl
    public void Start()
    {
        aniController = GetComponent<Animator>();
        SetIlde();
    }

    public void OnIdleDone()
    {
        int r = Random.Range(1, 5); // 1 đến 4
        aniController.SetTrigger("Idle" + r);
    }

    public void SetStun()
    {
        DebugCustom.ShowDebugColorRed("Stun");

        aniController.SetTrigger(stun);
        for (int i = 0; i < objEysStun.Count; i++)
        {
            objEysStun[i].SetActive(true);
        }
    }

    public void SetIlde()
    {
        OnIdleDone();

        for (int i = 0; i < objEysStun.Count; i++)
        {
            objEysStun[i].SetActive(false);
        }
    }
}
