using DG.Tweening;
using System;
using UnityEngine;

public class FxController : MonoBehaviour
{
    public static FxController Instance { get; private set; }

    public GameObject objHandBrokenPrefb;
    public GameObject objBarrierDestroyPrefb;
    public GameObject objHandClockPrefab;
    public GameObject objStunPrefab;
    public GameObject objBreakIcePrefab;
    public GameObject objCrakedIcePrefab;
    public GameObject objHeartBreakPrefab;

    private HandHummer handHammer;
    private BarrierDestroyFx barrierDestroyFx;
    private HandClock handClock;
    private Action actionHammer;
    private Action actionClock;
    private Action actionDestroyBarrier;

    private void Start()
    {
        Instance = this;
    }

    public void ShowHeartBreack(Vector3 pos)
    {
        GameObject objHeart = PoolingController.Instance.GetGameObjectFromPool(TypeObject.FX_HEART_BREAK, objHeartBreakPrefab);
        objHeart.transform.position = pos;
        objHeart.SetActive(true);
     
    }

    public void ShowBroken(Vector3 pos, Action action)
    {
        if (handHammer == null)
        {
            handHammer = Instantiate(objHandBrokenPrefb).GetComponent<HandHummer>();
        }

        if(this.actionHammer!= null)
        {
            actionHammer.Invoke();
        }
        this.actionHammer = action;

        handHammer.gameObject.SetActive(false);
        handHammer.transform.position = pos;
        handHammer.gameObject.SetActive(true);
    }

    public void ShowDestroyBarrier(Vector3 pos, Action action)
    {
        if (barrierDestroyFx == null)
        {
            barrierDestroyFx = Instantiate(objBarrierDestroyPrefb).GetComponent<BarrierDestroyFx>();
        }
        if (this.actionDestroyBarrier != null)
        {
            actionDestroyBarrier.Invoke();
        }

        this.actionDestroyBarrier = action;

        barrierDestroyFx.gameObject.SetActive(false);
        barrierDestroyFx.transform.position = pos;
        barrierDestroyFx.gameObject.SetActive(true);
    }

    public void ShowClock(Vector3 pos, Action action)
    {
        if (handClock == null)
        {
            handClock = Instantiate(objHandClockPrefab).GetComponent<HandClock>();
        }

        this.actionClock = action;

        handClock.transform.position = pos;
        handClock.gameObject.SetActive(true);
    }

    public void DoneAnimHammer()
    {
        //handHammer.gameObject.SetActive(false);

        if (actionHammer != null)
        {
            actionHammer.Invoke();
        }

        actionHammer = null;
    }

    public void DoneAniXeng()
    {
        if(actionDestroyBarrier != null)
        {
            actionDestroyBarrier.Invoke();
        }

        actionDestroyBarrier = null;
    }

    public void DoneAnimClock()
    {
        DebugCustom.ShowDebugColorRed("Done ice");


        if (actionClock != null)
        {
            actionClock.Invoke();
        }
        else
        {
            DebugCustom.ShowDebugColorRed("None error");
        }

        this.StartDelayAction(1f, () =>
        {
            if (handClock != null)
                handClock.gameObject.SetActive(false);
        });
    }

}
