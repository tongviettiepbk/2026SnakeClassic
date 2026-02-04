using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BrickObstacle : Obstacle
{
    public BrickDataInMap dataInMap { get; private set; }

    [Space(20)]
    public Animator animatorBrick;
    public string strAnimIdle = "Idle";
    public string strAnimHit = "Hit";
    public string strAnimDie = "Die";

    [Space(20)]
    public float timeDelayStartShowAnim = 0.5f;
    public float timeDestroyObject = 1f;
    public float timeShowBreakAll = 0.5f;

    public TMP_Text hpText;
    public GameObject objCube;
    public List<Material> listMaterial;

    public int indexInListObstacle;
    private int hp;
    private int hpMax;

    private bool isDied = false;

    private void OnEnable()
    {
        //DebugCustom.ShowDebugColor("Event move");

        SimpleEventManager.Instance.Register(EventIDSimple.moveSuccess, OnMoveSuccess);

        PlayAnim(strAnimIdle);

    }

    public void Init(BrickDataInMap dataTemp, MapController mapController,int indexInList = -99)
    {
        this.mapController = mapController;
        this.dataInMap = dataTemp;
        this.hp = dataTemp.hp;
        this.hpMax = dataTemp.hp;

        this.indexInMap = dataTemp.indexInMap;
        this.
        isDied = false;
        this.indexInListObstacle = indexInList;
        this.hpText.text = hp.ToString();
    }

    private void OnMoveSuccess(object param)
    {
        if (isDied) return;

        hp--;
        this.hpText.text = hp.ToString();

        GameObject objTemp = null;
        if (hp == 0)
        {
            hpText.gameObject.SetActive(false);
            isDied = true;
            mapController.RemoveBrickObstacle(this);

            RemoveObstacle();

           
        }
        else
        {
            PlayAnim(strAnimHit);
            AudioManager.Instance.PlayLimitedSfx(AudioManager.Instance.sfxBreakCrack, 3);
            SetMaterial();
        }

        objTemp = PoolingController.Instance.GetGameObjectFromPool(TypeObject.FX_ICE_CRACKED, FxController.Instance.objCrakedIcePrefab);
        this.transform.eulerAngles = Vector3.zero;
        objTemp.transform.position = this.transform.position;
        this.transform.localEulerAngles = Vector3.zero;
        objTemp.gameObject.SetActive(true);
    }

    private void SetMaterial()
    {
        if (hp < 5)
        {
            objCube.GetComponent<Renderer>().material = listMaterial[1];
        }
        else
        {

        }
    }

    public override void RemoveObstacle()
    {
        
        this.StartDelayAction(timeDelayStartShowAnim, () =>
        {
            PlayAnim(strAnimDie);

            this.StartDelayAction(timeShowBreakAll,() => {

                AudioManager.Instance.PlayBreakBrick();

                var objTemp = PoolingController.Instance.GetGameObjectFromPool(TypeObject.FX_ICE_BREAK, FxController.Instance.objBreakIcePrefab);

                this.transform.eulerAngles = Vector3.zero;
                objTemp.transform.position = this.transform.position;
                this.transform.localEulerAngles = Vector3.zero;



                objTemp.gameObject.SetActive(true);

            });
            
        });


        this.StartDelayAction(timeDestroyObject, () =>
        {
            Destroy(this.gameObject);
        });

    }

    private void OnDestroy()
    {
        SimpleEventManager.Instance.Unregister(EventIDSimple.moveSuccess, OnMoveSuccess);
    }

    private void PlayAnim(string animStr)
    {
        animatorBrick.SetTrigger(animStr);
    }
}
