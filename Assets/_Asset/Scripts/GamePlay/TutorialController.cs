using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public enum IDTutorial
{
    MOVE = 0,
    ZOOM_OUT_IN = 1,
    BOOSTER_ESCAPSE = 2,
    BOOSTER_STOP_TIME = 3,
    BOOSTER_DESTROY_BRICK = 4,
    BOOSTER_DESTROY_BARRIER = 5,
    TUT_FEATURE_BRICK = 6,
    TUT_FEATURE_HOLE_EXIT = 7,
    TUT_FEATURE_STOP = 8,
    TUT_FEATURE_BARRIER = 9,
    TUT_BOOSTER_FIND = 10,

    SHOW_LINE_GECKO = 11,
    NONE = -1
}

public class TutorialController : MonoBehaviour
{
    private MapController mapController;

    public static TutorialController Instance;

    public GameObject objPrefabTutMove;
    public GameObject objPrefabTutZoomout;

    [HideInInspector]
    public IDTutorial idTutCurrent = IDTutorial.NONE;
    [HideInInspector]
    public int StepCurrent = 0;

    private string keyCompletedTutorialStep = "CompletedTutorialStep";

    private Dictionary<int, int> dictSaveCompleteTut = new Dictionary<int, int>();

    private GameObject objTutHandClick = null;

    private float timeDelayShowPopupBooster = 0.5f;

    private LUiTutNotice uiNotice;

    private int bonusBoosterTut = 3;
    public Gecko geckoTargetTut = null;
    public BrickObstacle brickTarget = null;
    public BarrierObstacle barrierTarget = null;

    private void Start()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        SimpleEventManager.Instance.Register(EventIDSimple.moveSuccess, OnMoveSuccess);
    }

    private void OnDisable()
    {
        SimpleEventManager.Instance.Unregister(EventIDSimple.moveSuccess, OnMoveSuccess);
    }

    public void CheckTutorial(MapController mapController)
    {
        this.mapController = mapController;
        int levelTemp = mapController.levelCurrent;

        string dataGet = PlayerPrefs.GetString(keyCompletedTutorialStep, "");
        dictSaveCompleteTut = JsonConvert.DeserializeObject<Dictionary<int, int>>(dataGet);

        if (dictSaveCompleteTut == null)
        {
            dictSaveCompleteTut = new Dictionary<int, int>();
        }

        idTutCurrent = IDTutorial.NONE;
        switch (mapController.levelCurrent)
        {
            case GameConfig.lvlTutMove:

                if (!IsCompletedTut(levelTemp))
                {
                    StepCurrent = 0;
                    idTutCurrent = IDTutorial.MOVE;

                    StartTutMove();
                }
                break;
            case GameConfig.lvlTutZoomOutIn:
                if (!IsCompletedTut(levelTemp))
                {
                    StepCurrent = 0;
                    idTutCurrent = IDTutorial.ZOOM_OUT_IN;

                    StartTutZoomOut();
                }
                break;
            case GameConfig.lvlTutShowLineGecko:
                if (!IsCompletedTut(levelTemp))
                {
                    StepCurrent = 0;
                    idTutCurrent = IDTutorial.SHOW_LINE_GECKO;

                    StartTutShowLineGeko();
                }
                break;

            case GameConfig.lvlUnlockEscapseGecko:


                if (!IsCompletedTut(levelTemp))
                {
                    StepCurrent = 0;
                    idTutCurrent = IDTutorial.BOOSTER_ESCAPSE;

                    StartTutBoosterEscape();
                }
                break;
            case GameConfig.lvlUnlockFindGeckoCanMove:


                if (!IsCompletedTut(levelTemp))
                {
                    StepCurrent = 0;
                    idTutCurrent = IDTutorial.TUT_BOOSTER_FIND;

                    StartTutBoosterFind();
                }
                break;
            case GameConfig.lvlUnlockStopTime:


                if (!IsCompletedTut(levelTemp))
                {
                    StepCurrent = 0;
                    idTutCurrent = IDTutorial.BOOSTER_STOP_TIME;

                    StartTutBoosterTimeStop();
                }
                break;
            case GameConfig.lvlUnlockDestroyIceHamer:


                if (!IsCompletedTut(levelTemp))
                {
                    StepCurrent = 0;
                    idTutCurrent = IDTutorial.BOOSTER_DESTROY_BRICK;
                    StartTutBoosterBrickDestroy();
                }
                break;
            case GameConfig.lvlUnlockDestroyBarrier:

                if (!IsCompletedTut(levelTemp))
                {
                    StepCurrent = 0;
                    idTutCurrent = IDTutorial.BOOSTER_DESTROY_BARRIER;

                    StartTutBoosterBarrierDestroy();
                }
                break;
            case GameConfig.lvlTutFeatureBrick:
                if (!IsCompletedTut(levelTemp))
                {
                    idTutCurrent = IDTutorial.TUT_FEATURE_BRICK;

                    StepCurrent = 0;
                    this.StartDelayAction(timeDelayShowPopupBooster, () =>
                    {
                        LUiUnlockFeature ui = UIManager.Instance.LoadUI(UIKey.TUTORIAL_FEATURE, isPauseMusic: true) as LUiUnlockFeature;
                        ui.SetTypeBooster(IDTutorial.TUT_FEATURE_BRICK);
                    });

                }
                break;
            case GameConfig.lvlTutFeatureHoleExit:
                if (!IsCompletedTut(levelTemp))
                {
                    idTutCurrent = IDTutorial.TUT_FEATURE_HOLE_EXIT;

                    StepCurrent = 0;
                    this.StartDelayAction(timeDelayShowPopupBooster, () =>
                    {
                        LUiUnlockFeature ui = UIManager.Instance.LoadUI(UIKey.TUTORIAL_FEATURE, isPauseMusic: true) as LUiUnlockFeature;
                        ui.SetTypeBooster(IDTutorial.TUT_FEATURE_HOLE_EXIT);
                    });
                }
                break;
            case GameConfig.lvlTutFeatureStop:
                if (!IsCompletedTut(levelTemp))
                {
                    idTutCurrent = IDTutorial.TUT_FEATURE_STOP;

                    StepCurrent = 0;
                    this.StartDelayAction(timeDelayShowPopupBooster, () =>
                    {
                        LUiUnlockFeature ui = UIManager.Instance.LoadUI(UIKey.TUTORIAL_FEATURE, isPauseMusic: true) as LUiUnlockFeature;
                        ui.SetTypeBooster(IDTutorial.TUT_FEATURE_STOP);
                    });
                }
                break;
            case GameConfig.lvlTutFeatureBarrier:
                if (!IsCompletedTut(levelTemp))
                {
                    idTutCurrent = IDTutorial.TUT_FEATURE_BARRIER;

                    StepCurrent = 0;
                    this.StartDelayAction(timeDelayShowPopupBooster, () =>
                    {
                        LUiUnlockFeature ui = UIManager.Instance.LoadUI(UIKey.TUTORIAL_FEATURE, isPauseMusic: true) as LUiUnlockFeature;
                        ui.SetTypeBooster(IDTutorial.TUT_FEATURE_BARRIER);
                    });
                }
                break;
            default:
                break;
        }

        if (idTutCurrent != IDTutorial.NONE)
        {
            mapController.SetTut(idTutCurrent);
        }
    }

    #region Tut Move 1

    private void StartTutMove()
    {
        var listNode = mapController.listGecko[0].dataGeckoInMap.listNode;
        int indexTut = (int)(listNode.Count / 2);
        Vector3 posSpawn = mapController.mapHelper.GetWorldPositionFromIndex(listNode[indexTut]);

        objTutHandClick = Instantiate(objPrefabTutMove);
        objTutHandClick.transform.position = posSpawn;

        mapController.mainCamera.isLockCaremaByTut = true;

        uiNotice = UIManager.Instance.LoadUI(UIKey.TUTORIAL_NOTICE, isPauseMusic: true) as LUiTutNotice;
        uiNotice.OpenTutMove();
    }

    private void OnMoveSuccess(object obj)
    {
        if (idTutCurrent == IDTutorial.MOVE)
        {
            NextStep();
        }

        if (idTutCurrent == IDTutorial.BOOSTER_ESCAPSE)
        {
            NextStep();
        }
    }

    #endregion

    #region tut ZoomOut

    private void StartTutZoomOut()
    {
        float timeZoomIn = 1f;
        int rows = mapController.mapHelper.row;
        int cols = mapController.mapHelper.column;

        int rowCenter = rows / 2;
        int columnCenter = cols / 2;

        mapController.SetTut(idTutCurrent);

        DebugCustom.ShowDebugColor("Center:" + rowCenter, columnCenter);

        int indexCenterMap = rowCenter * cols + columnCenter;

        this.StartDelayAction(mapController.mainCamera.timePlayCameraStart + 0.2f, () =>
        {
            mapController.mainCamera.PlayCameraZoomoutTut(timeZoomIn);

            this.StartDelayAction(timeZoomIn, () =>
            {
                //Spawn Tut
                Vector3 posSpawn = mapController.mapHelper.GetWorldPositionFromIndex(indexCenterMap);

                objTutHandClick = Instantiate(objPrefabTutZoomout);
                objTutHandClick.transform.position = posSpawn;

                uiNotice = UIManager.Instance.LoadUI(UIKey.TUTORIAL_NOTICE, isPauseMusic: true) as LUiTutNotice;
                uiNotice.OpenTutZoomOut();
            });
        });

    }

    #endregion

    #region Show Line Gecko

    private void StartTutShowLineGeko()
    {
        StepCurrent = 0;
        mapController.SetTut(idTutCurrent);

        this.StartDelayAction(timeDelayShowPopupBooster, () =>
        {
            LUiUnlockBooster ui = UIManager.Instance.LoadUI(UIKey.TUTORIAL_BOOSTER, isPauseMusic: true) as LUiUnlockBooster;
            ui.SetTypeBooster(Booster.SHOW_LINE);
        });
    }

    #endregion

    #region tut Booster Escape

    private void StartTutBoosterEscape()
    {
        ItemUtils.Receive(ItemType.ESCAPE_GECKO, bonusBoosterTut, false, false);
        LGamePlay.Instance.ReLoadFooter();
        StepCurrent = 0;

        mapController.SetTut(idTutCurrent);
        this.StartDelayAction(timeDelayShowPopupBooster, () =>
        {
            LUiUnlockBooster ui = UIManager.Instance.LoadUI(UIKey.TUTORIAL_BOOSTER, isPauseMusic: true) as LUiUnlockBooster;
            ui.SetTypeBooster(Booster.ESCAPES);
        });

        mapController.mainCamera.isLockCaremaByTut = true;

        for (int i = 0; i < LGamePlay.Instance.itemBoosters.Count; i++)
        {
            var dataTemp = LGamePlay.Instance.itemBoosters[i];
            if (dataTemp.typeBooster == Booster.ESCAPES)
            {
                dataTemp.SetLock(true);
            }
        }
    }

    private void Step2BoosterEscapse()
    {
        int indexGecko = 2;
        int dotGecko = 1;

        float startCamera = mapController.mainCamera.mainCamera.orthographicSize;
        float endCamera = 9;
        float timeActionTemp = 1f;
        mapController.mainCamera.PlayCameraZoom(startCamera, endCamera, timeActionTemp);

        this.geckoTargetTut = mapController.listGecko[indexGecko];
        var listNode = geckoTargetTut.dataGeckoInMap.listNode;
        int indexTut = dotGecko;
        Vector3 posSpawn = mapController.mapHelper.GetWorldPositionFromIndex(listNode[indexTut]);
        objTutHandClick = Instantiate(objPrefabTutMove);
        objTutHandClick.transform.position = posSpawn;
    }

    private void DoneEscapseBooster()
    {
        DebugCustom.ShowDebugColorRed("Done Escapse booster");
        mapController.SetOffTut();
        mapController.mainCamera.isLockCaremaByTut = false;

        if (objTutHandClick != null)
        {
            Destroy(objTutHandClick);
        }

        SaveTutCompelete(mapController.levelCurrent);

        float startCamera = mapController.mainCamera.mainCamera.orthographicSize;
        float endCamera = mapController.mainCamera.defaultZoom;
        float timeActionTemp = 1f;
        mapController.mainCamera.PlayCameraZoom(startCamera, endCamera, timeActionTemp);
    }

    #endregion

    #region Tut Booster Find
    private void StartTutBoosterFind()
    {
        ItemUtils.Receive(ItemType.FIND_GECKO_MOVE, bonusBoosterTut, false, false);
        LGamePlay.Instance.ReLoadFooter();
        StepCurrent = 0;

        mapController.SetTut(idTutCurrent);
        this.StartDelayAction(timeDelayShowPopupBooster, () =>
        {
            LUiUnlockBooster ui = UIManager.Instance.LoadUI(UIKey.TUTORIAL_BOOSTER, isPauseMusic: true) as LUiUnlockBooster;
            ui.SetTypeBooster(Booster.FIND_GECKO_MOVE);
        });

        mapController.mainCamera.isLockCaremaByTut = true;

        for (int i = 0; i < LGamePlay.Instance.itemBoosters.Count; i++)
        {
            var dataTemp = LGamePlay.Instance.itemBoosters[i];
            if (dataTemp.typeBooster == Booster.FIND_GECKO_MOVE)
            {
                dataTemp.SetLock(true);
            }
        }
    }

    #endregion

    #region Tut Time Stop

    private void StartTutBoosterTimeStop()
    {
        ItemUtils.Receive(ItemType.TIME_STOP_ICE, bonusBoosterTut, false, false);
        LGamePlay.Instance.ReLoadFooter();

        StepCurrent = 0;
        mapController.SetTut(idTutCurrent);
        this.StartDelayAction(timeDelayShowPopupBooster, () =>
        {
            LUiUnlockBooster ui = UIManager.Instance.LoadUI(UIKey.TUTORIAL_BOOSTER, isPauseMusic: true) as LUiUnlockBooster;
            ui.SetTypeBooster(Booster.TIMESTOP);
        });

        for (int i = 0; i < LGamePlay.Instance.itemBoosters.Count; i++)
        {
            var dataTemp = LGamePlay.Instance.itemBoosters[i];
            if (dataTemp.typeBooster == Booster.TIMESTOP)
            {
                dataTemp.SetLock(true);
            }
        }

    }


    #endregion

    #region Tut Brick Destroy

    private void StartTutBoosterBrickDestroy()
    {
        ItemUtils.Receive(ItemType.BRICK_BROKEN, bonusBoosterTut, false, false);
        LGamePlay.Instance.ReLoadFooter();

        StepCurrent = 0;
        mapController.SetTut(idTutCurrent);
        this.StartDelayAction(timeDelayShowPopupBooster, () =>
        {
            LUiUnlockBooster ui = UIManager.Instance.LoadUI(UIKey.TUTORIAL_BOOSTER, isPauseMusic: true) as LUiUnlockBooster;
            ui.SetTypeBooster(Booster.BRICK_DESTROY);
        });

        mapController.mainCamera.isLockCaremaByTut = true;

        for (int i = 0; i < LGamePlay.Instance.itemBoosters.Count; i++)
        {
            var dataTemp = LGamePlay.Instance.itemBoosters[i];
            if (dataTemp.typeBooster == Booster.BRICK_DESTROY)
            {
                dataTemp.SetLock(true);
            }
        }
    }

    private void Step2BoosterBrickDestroy()
    {
        int indexBrick = 19;

        float startCamera = mapController.mainCamera.mainCamera.orthographicSize;
        float endCamera = 9;
        float timeActionTemp = 1f;
        mapController.mainCamera.PlayCameraZoom(startCamera, endCamera, timeActionTemp);

        this.brickTarget = mapController.listBrickObstacle[indexBrick];

        Vector3 posSpawn = mapController.mapHelper.GetWorldPositionFromIndex(this.brickTarget.indexInMap);
        objTutHandClick = Instantiate(objPrefabTutMove);
        objTutHandClick.transform.position = posSpawn;

        for (int i = 0; i < LGamePlay.Instance.itemBoosters.Count; i++)
        {
            var dataTemp = LGamePlay.Instance.itemBoosters[i];
            if (dataTemp.typeBooster == Booster.TIMESTOP)
            {
                dataTemp.SetLock(false);
            }
        }
    }

    private void DoneBrickDestroyBooster()
    {
        DebugCustom.ShowDebugColorRed("Done DoneBrickDestroyBooster");

        mapController.SetOffTut();
        mapController.mainCamera.isLockCaremaByTut = false;

        if (objTutHandClick != null)
        {
            Destroy(objTutHandClick);
        }

        SaveTutCompelete(mapController.levelCurrent);

        float startCamera = mapController.mainCamera.mainCamera.orthographicSize;
        float endCamera = mapController.mainCamera.defaultZoom;
        float timeActionTemp = 1f;
        mapController.mainCamera.PlayCameraZoom(startCamera, endCamera, timeActionTemp);
    }

    #endregion

    #region Tut Barrier Destroy

    private void StartTutBoosterBarrierDestroy()
    {
        ItemUtils.Receive(ItemType.BARRIER, bonusBoosterTut, false, false);
        LGamePlay.Instance.ReLoadFooter();

        StepCurrent = 0;
        mapController.SetTut(idTutCurrent);
        this.StartDelayAction(timeDelayShowPopupBooster, () =>
        {
            LUiUnlockBooster ui = UIManager.Instance.LoadUI(UIKey.TUTORIAL_BOOSTER, isPauseMusic: true) as LUiUnlockBooster;
            ui.SetTypeBooster(Booster.BARRIER_DESTROY);
        });

        mapController.mainCamera.isLockCaremaByTut = true;
        LGamePlay.Instance.itemBoosters[2].SetLock(true);

        for (int i = 0; i < LGamePlay.Instance.itemBoosters.Count; i++)
        {
            var dataTemp = LGamePlay.Instance.itemBoosters[i];
            if (dataTemp.typeBooster == Booster.BARRIER_DESTROY)
            {
                dataTemp.SetLock(true);
            }
        }
    }

    private void Step2BoosterBarrierDestroy()
    {
        int indexBarrier = 6;

        float startCamera = mapController.mainCamera.mainCamera.orthographicSize;
        float endCamera = 8;
        float timeActionTemp = 1f;
        mapController.mainCamera.PlayCameraZoom(startCamera, endCamera, timeActionTemp);

        this.barrierTarget = mapController.listBarrierObstacle[indexBarrier];

        Vector3 posSpawn = mapController.mapHelper.GetWorldPositionFromIndex(this.barrierTarget.indexInMap);
        objTutHandClick = Instantiate(objPrefabTutMove);
        objTutHandClick.transform.position = posSpawn;
    }

    private void DoneBarrierDestroyBooster()
    {
        DebugCustom.ShowDebugColorRed("Done DoneBarrierDestroyBooster");

        mapController.SetOffTut();
        mapController.mainCamera.isLockCaremaByTut = false;

        if (objTutHandClick != null)
        {
            Destroy(objTutHandClick);
        }

        SaveTutCompelete(mapController.levelCurrent);

        float startCamera = mapController.mainCamera.mainCamera.orthographicSize;
        float endCamera = mapController.mainCamera.defaultZoom;
        float timeActionTemp = 1f;
        mapController.mainCamera.PlayCameraZoom(startCamera, endCamera, timeActionTemp);

    }

    #endregion

    public void NextStep()
    {
        switch (idTutCurrent)
        {
            case IDTutorial.MOVE:
                if (IsCompletedTut(mapController.levelCurrent) == false)
                {
                    DebugCustom.ShowDebugColorRed("Done tut move");
                    SaveTutCompelete(mapController.levelCurrent);
                    if (objTutHandClick != null)
                    {
                        Destroy(objTutHandClick);
                    }

                    mapController.mainCamera.isLockCaremaByTut = false;

                    if (uiNotice != null)
                    {
                        uiNotice.Close();
                    }
                }
                break;

            case IDTutorial.ZOOM_OUT_IN:

                if (IsCompletedTut(mapController.levelCurrent) == false)
                {
                    DebugCustom.ShowDebugColorRed("Done tut move");
                    SaveTutCompelete(mapController.levelCurrent);
                    if (objTutHandClick != null)
                    {
                        Destroy(objTutHandClick);
                    }

                    if (uiNotice != null)
                    {
                        uiNotice.Close();
                    }
                }

                mapController.SetOffTut();

                break;
            case IDTutorial.SHOW_LINE_GECKO:

                if (StepCurrent == 0)
                {
                    // Show Chose Booste Show Line
                    DebugCustom.ShowDebugColorRed("Next tut ShowLine choose booster StepCurrent 0");
                    LGamePlay.Instance.ActiveHandBoosterTut(Booster.SHOW_LINE);
                }

                break;
            case IDTutorial.BOOSTER_ESCAPSE:
                if (StepCurrent == 0)
                {
                    // Show Chose Booster Escapse
                    DebugCustom.ShowDebugColorRed("Next tut escapse choose booster StepCurrent 0");
                    LGamePlay.Instance.ActiveHandBoosterTut(Booster.ESCAPES);
                }

                if (StepCurrent == 1)
                {
                    // chose escapse
                    DebugCustom.ShowDebugColorRed(" escapse choose StepCurrent 1");
                    Step2BoosterEscapse();

                }

                if (StepCurrent == 2)
                {
                    // Done Escapse
                    DoneEscapseBooster();
                }

                StepCurrent++;
                break;
            case IDTutorial.BOOSTER_STOP_TIME:
                DebugCustom.ShowDebugColorRed("Next tut StopTime choose booster");
                LGamePlay.Instance.ActiveHandBoosterTut(Booster.TIMESTOP);

                break;
            case IDTutorial.TUT_BOOSTER_FIND:
                DebugCustom.ShowDebugColorRed("Next tut find choose booster");
                LGamePlay.Instance.ActiveHandBoosterTut(Booster.FIND_GECKO_MOVE);

                break;

            case IDTutorial.BOOSTER_DESTROY_BRICK:

                if (StepCurrent == 0)
                {
                    LGamePlay.Instance.ActiveHandBoosterTut(Booster.BRICK_DESTROY);
                }

                if (StepCurrent == 1)
                {
                    // chose Brick
                    Step2BoosterBrickDestroy();
                }

                if (StepCurrent == 2)
                {
                    // chose Brick
                    DoneBrickDestroyBooster();
                }

                StepCurrent++;
                break;
            case IDTutorial.BOOSTER_DESTROY_BARRIER:

                if (StepCurrent == 0)
                {
                    LGamePlay.Instance.ActiveHandBoosterTut(Booster.BARRIER_DESTROY);
                }

                if (StepCurrent == 1)
                {
                    // chose Barrier
                    Step2BoosterBarrierDestroy();
                }

                if (StepCurrent == 2)
                {
                    // chose Barrier
                    DoneBarrierDestroyBooster();
                }

                StepCurrent++;

                break;
            case IDTutorial.TUT_FEATURE_BARRIER:
            case IDTutorial.TUT_FEATURE_BRICK:
            case IDTutorial.TUT_FEATURE_HOLE_EXIT:
            case IDTutorial.TUT_FEATURE_STOP:

                this.StartDelayAction(0.2f, () =>
                {
                    mapController.SetOffTut();
                });

                SaveTutCompelete(mapController.levelCurrent);

                break;
        }

    }

    public void SaveTutCompelete(int levelCurrent)
    {
        if (dictSaveCompleteTut.ContainsKey(levelCurrent))
        {
            dictSaveCompleteTut[levelCurrent] = 1;
        }
        else
        {
            dictSaveCompleteTut.Add(levelCurrent, 1);
        }

        string dataSave = JsonConvert.SerializeObject(dictSaveCompleteTut);
        PlayerPrefs.SetString(keyCompletedTutorialStep, dataSave);

        if(levelCurrent == GameConfig.lvlUnlockDestroyBarrier)
        {
            AppsflyerHelper.SendTutComplete();
        }
    }

    private bool IsCompletedTut(int levelCurrent)
    {
        if (dictSaveCompleteTut.ContainsKey(levelCurrent))
        {
            if (dictSaveCompleteTut[levelCurrent] == 1)
            {
                return true;
            }
        }
        return false;
    }
}
