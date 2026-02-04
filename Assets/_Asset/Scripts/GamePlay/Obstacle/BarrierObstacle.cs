using Newtonsoft.Json;
using UnityEngine;

public class BarrierObstacle : Obstacle
{
    public GameObject objVisualBarrier;

    public Animator animator;
    public int indexInListObstacle;

    private string idleGreen = "IdleGreen";
    private string idleRed = "IdleRed";
    private string greenToRed = "GreenToRed";
    private string redToGreen = "RedToGreen";

    private BarrierDataInMap dataInMap;
    private bool isOpen = false;
    private bool isCheckShowVisual = false;

    private Gecko geckoFollowCheck = null;

    private float timeCount = 0;
    private float timeDelayMaxShow = 0.5f;

    public void Init(BarrierDataInMap dataTemp, MapController mapController,int indexInListTemp = -99)
    {
        this.mapController = mapController;
        this.dataInMap = dataTemp;
        this.indexInMap = dataTemp.indexInMap;
        this.value = dataTemp.isOpen;
        geckoFollowCheck = null;
        this.indexInListObstacle = indexInListTemp;

        if (dataTemp.isOpen == 1)
        {
            isOpen = true;
        }
        else
        {
            isOpen = false;
        }

        if (isOpen)
        {
            SetAnim(idleGreen);
        }
        else
        {
            SetAnim(idleRed);
        }

        ShowVisual();
    }

    private void OnEnable()
    {
        SimpleEventManager.Instance.Register(EventIDSimple.moveSuccess, OnMoveSuccess);
    }

    private void OnDisable()
    {
        SimpleEventManager.Instance.Unregister(EventIDSimple.moveSuccess, OnMoveSuccess);
    }

    private void Update()
    {
        CheckShowVisualWithGecko();
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    private void OnMoveSuccess(object param)
    {
        //DebugCustom.ShowDebugColorRed("Change state barrier");

        isOpen = !isOpen;
        isCheckShowVisual = false;

        Gecko gecko = (Gecko)param;

        //ShowVisual();
        //return;

        if (gecko != null)
        {
            var listIndexNodeMove = gecko.GetListNodeMoveData();

            //DebugCustom.ShowDebugColor("List Move:" + JsonConvert.SerializeObject(listIndexNodeMove), dataInMap.indexInMap);

            for (int i = 0; i < listIndexNodeMove.Count; i++)
            {
                if (listIndexNodeMove[i] == this.dataInMap.indexInMap)
                {
                    // gecko có đi qua nên cần show sau khi đi qua.
                    isCheckShowVisual = true;
                    geckoFollowCheck = gecko;

                    //DebugCustom.ShowDebugColorRed("!!!!!!!!!!!!!!!Show light traffic delay");
                    break;
                }
            }

            if (isCheckShowVisual == false)
            {
                ShowVisual();
                timeCount = 0;
            }
            else
            {

            }

        }
        else
        {
            ShowVisual();
        }
    }

    private void ShowVisual()
    {
        if (isOpen)
        {
            // Visual open barrier
            //objVisualBarrier.gameObject.SetActive(false);
            SetAnim(redToGreen);
        }
        else
        {
            // Visual close barrier
            //objVisualBarrier.gameObject.SetActive(true);
            SetAnim(greenToRed);
        }
    }

    private void CheckShowVisualWithGecko()
    {
        if (!isCheckShowVisual)
            return;

        if (geckoFollowCheck == null)
        {
            isCheckShowVisual = false;
            ShowVisual();
            return;
        }

        if (Vector3.Distance(geckoFollowCheck.GetTransHead().position, this.transform.position) < 1)
        {
            DebugCustom.ShowDebugColorRed("Show light traffic");
            isCheckShowVisual = false;
            ShowVisual();
            return;
        }

        timeCount += Time.deltaTime;
        if(timeCount > timeDelayMaxShow)
        {
            isCheckShowVisual = false;
            ShowVisual();
        }

    }

    private void SetAnim(string nameAnim)
    {
        this.animator.SetTrigger(nameAnim);
    }
}
