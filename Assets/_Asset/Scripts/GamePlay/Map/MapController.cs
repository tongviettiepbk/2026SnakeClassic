using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public enum TypeDev
{
    Product = 0,
    dev = 1,
    mkt = 2,
    none = 99,
}

public enum StateGame
{
    NONE = 0,
    PLAYING = 2,
    END = 3,
    PAUSE = 4,
}

public enum Booster
{
    NONE = 0,
    ESCAPES = 1,
    TIMESTOP = 2,
    BARRIER_DESTROY = 3,
    BRICK_DESTROY = 4,
    FIND_GECKO_MOVE = 5,

    SHOW_LINE = 99,
}

public class MapController : MonoBehaviour
{
    public static MapController Instance;
    public bool isHack = false;
    public bool isHackInSetting = false;
    public TypeDev typeDev = TypeDev.dev;

    [Space(20)]
    public TextAsset mapTest;

    [Space(40)]
    public LGamePlay lgamePlay;

    [Space(20)]
    public CameraController mainCamera;
    public MapGrid2dHelper mapHelper;

    [Space(20)]
    public TutorialController tutController;

    public GameObject pivotMap;

    [Space(20)]
    public DotItem dotPrefab;

    [Space(20)]
    public List<Gecko> geckoPrefab;


    [Space(20)]
    public GameObject brickPrefab;
    public List<GameObject> exitPrefab;
    public GameObject StopPrefab;
    public GameObject barrierPrefab;

    [Space(20)]
    public GameObject objNoticeBooster;

    [Space(20)]
    public int levelCurrent;

    public MapInfo mapInfo;

    public List<NodeMap> listNodeMap = new List<NodeMap>();

    [Space(20)]
    public List<GameObject> listPlanObj;
    public GameObject objFirework;

    public List<Gecko> listGecko = new List<Gecko>();
    public Dictionary<int, DotItem> dictDots = new Dictionary<int, DotItem>();

    [Space(20)]
    public List<BrickObstacle> listBrickObstacle = new List<BrickObstacle>();
    public List<ExitsObstacle> listExitsObstacle = new List<ExitsObstacle>();
    public List<StopObstacle> listStopObstacle = new List<StopObstacle>();
    public List<BarrierObstacle> listBarrierObstacle = new List<BarrierObstacle>();
    public bool isStartCountTimeGame { get; private set; }
    [SerializeField]
    private int countGeckoCurrent = 0;

    [SerializeField]
    private int totalGecko = 0;
    public int TotalGecko => totalGecko;
    public int CurrentGecko => countGeckoCurrent;

    private Vector3 tempVector3 = Vector3.zero;

    private Vector3 mouseClickStart = Vector3.zero;
    private Vector3 mouseClickHold = Vector3.zero;
    private bool isClickChoseGecko = false;

    private bool isShowLine = false;

    public StateGame stateCurrentGame = StateGame.NONE;
    public Booster boosterUse = Booster.NONE;

    private List<GameObject> listObjectNoticeBooster = new List<GameObject>();
    private List<int> listIntTemp = new List<int>();

    private bool isEditMode = false;

    [HideInInspector]
    public bool isTut = false;

    private bool isStart = false;
    private GameObject objNoticeFindGecko = null;

    // Send Event
    private float timeStartGame = 0;

    void Start()
    {
        Instance = this;
        LoadBgGame();

        levelCurrent = GameData.userData.profile.currentStageId;

        LoadMap(levelCurrent);

        mainCamera.InitCameraByMap(mapInfo.column, mapInfo.row);
        mainCamera.SetCameraCenterMap();

        

        objFirework.SetActive(false);

        lgamePlay.Init();

        tutController.CheckTutorial(this);

        if(GameConfig.isShowOpenAdsFisrt == false)
        {
            GameConfig.isShowOpenAdsFisrt = true;
            MediationAds.Instance.TryShowAppOpenAd();
        }
    }

    private void OnEnable()
    {
        SimpleEventManager.Instance.Register(EventIDSimple.geckoMoveOutCompelteMap, OnGeckoMoveOutMapSucess);
        SimpleEventManager.Instance.Register(EventIDSimple.geckoOutMapSucess, OnGeckoOutMapSucess);
        SimpleEventManager.Instance.Register(EventIDSimple.pauseGameUI, OnRequestPauseGameUI);
        SimpleEventManager.Instance.Register(EventIDSimple.unPauseGameUI, OnRequestUnPauseGameUI);

        SimpleEventManager.Instance.Register(EventIDSimple.moveSuccess, OnMoveSucess);
    }


    private void OnDisable()
    {
        SimpleEventManager.Instance.Unregister(EventIDSimple.geckoMoveOutCompelteMap, OnGeckoMoveOutMapSucess);
        SimpleEventManager.Instance.Unregister(EventIDSimple.geckoOutMapSucess, OnGeckoOutMapSucess);
        SimpleEventManager.Instance.Unregister(EventIDSimple.pauseGameUI, OnRequestPauseGameUI);
        SimpleEventManager.Instance.Unregister(EventIDSimple.unPauseGameUI, OnRequestUnPauseGameUI);

        SimpleEventManager.Instance.Unregister(EventIDSimple.moveSuccess, OnMoveSucess);
    }

    void Update()
    {
        if (!isStart)
        {
            return;
        }

        if (isEditMode)
        {
            return;
        }

        UpdateEditor();

        if (EventSystem.current.IsPointerOverGameObject())
        {
            isClickChoseGecko = false;
            return;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                isClickChoseGecko = false;
                return;
            }
        }

        // Check Click Gecko
        if (Input.GetMouseButtonDown(0))
        {
            isClickChoseGecko = true;

            mouseClickStart = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            mouseClickHold = Input.mousePosition;

            if (isClickChoseGecko == true && Vector3.Distance(mouseClickStart, mouseClickHold) > 10f)
            {
                isClickChoseGecko = false;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isClickChoseGecko)
            {
                DetectGecko(mouseClickStart);
            }
        }
    }


    #region Spawn Element Inmap: Gecko, brick...

    private void LoadMap(int level)
    {
        isShowLine = false;

        if (GameConfig.lvMktOpenNeedVideo > 0)
        {
            level = GameConfig.lvMktOpenNeedVideo;
        }

        if (GameConfig.typeDev != TypeDev.none)
        {
            typeDev = GameConfig.typeDev;
        }

        if (typeDev == TypeDev.dev)
        {
            string mapDataMapJson = mapTest.text;
            mapInfo = JsonConvert.DeserializeObject<MapInfo>(mapDataMapJson);
        }
        else if (typeDev == TypeDev.mkt)
        {
            LoadMapMkt();
        }
        else
        {
            //----------------- product

            if (level > 90)
            {
                level = UnityEngine.Random.Range(48, 91);
            }

            string fileName = level.ToString("D2");
            string jsonData = "";
            string path = "";

            // -------------- Load JSON trực tiếp (Editor) ---------------- c2
            //path = $"{Application.dataPath}/_Asset/MapData/MapFinal/{fileName}.json";
            //jsonData = System.IO.File.ReadAllText(path);

            //-------------------load bytes + giải mã
            path = $"MapBytes/{fileName}.json";
            DebugCustom.ShowDebugColorRed("Load map: " + path);
            var ta = Resources.Load<TextAsset>(path);
            jsonData = AESUtil.DecryptAES(ta.text);
            //--------------- end ma --------------------------------------------------

            mapInfo = JsonConvert.DeserializeObject<MapInfo>(jsonData);
            DebugCustom.ShowDebugColorRed("Load map: " + path);
        }

        mapHelper.InitMap(mapInfo.column, mapInfo.row, pivotMap.transform.position);
        SpawnGetko(mapInfo);
        SpawnBrick(mapInfo);
        SpawnExitHole(mapInfo);
        SpawnStop(mapInfo);
        SpawnBarrier(mapInfo);
        SetInfoMap(mapInfo);

        this.StartDelayAction(CameraController.Instance.timePlayCameraStart + 0.2f, () =>
        {

            isStart = true;
        });

        FirebaseAnalyticsHelper.LogLevelStart(level);
    }

    private void SpawnGetko(MapInfo mapInfo)
    {
        for (int i = 0; i < mapInfo.listGeckoInMap.Count; i++)
        {
            GeckoDataInMap geckoData = mapInfo.listGeckoInMap[i];

            Gecko gecko = null;
            try
            {
                gecko = Instantiate(geckoPrefab[geckoData.colorGecko], transform);
            }
            catch
            {
                gecko = Instantiate(geckoPrefab[0], transform);
            }

            gecko.transform.position = Vector3.zero;

            gecko.Init(this, geckoData, i);

            listGecko.Add(gecko);
        }

        totalGecko = listGecko.Count;
        countGeckoCurrent = listGecko.Count;

        DebugCustom.ShowDebugColorRed("Total gecko:", countGeckoCurrent);
    }

    public void SpawnGecko(GeckoDataInMap geckoData)
    {
        if (geckoData == null)
        {
            DebugCustom.ShowDebugColorRed("Lỗi spawn gecko data null");
            return;
        }
        Gecko gecko = null;
        try
        {
            gecko = Instantiate(geckoPrefab[geckoData.colorGecko], transform);
        }
        catch
        {
            gecko = Instantiate(geckoPrefab[0], transform);
        }
        gecko.transform.position = Vector3.zero;
        gecko.Init(this, geckoData);
        listGecko.Add(gecko);

    }

    private void SpawnBrick(MapInfo mapInfo)
    {
        if (mapInfo.listBrickInMap == null)
            return;

        for (int i = 0; i < mapInfo.listBrickInMap.Count; i++)
        {
            BrickDataInMap brickData = mapInfo.listBrickInMap[i];

            tempVector3 = mapHelper.GetWorldPositionFromIndex(brickData.indexInMap);
            GameObject objBrick = Instantiate(brickPrefab, tempVector3, Quaternion.identity, transform);
            BrickObstacle brickObstacle = objBrick.GetComponent<BrickObstacle>();

            if (brickObstacle != null)
            {
                brickObstacle.Init(brickData, this, i);
                listBrickObstacle.Add(brickObstacle);
            }
        }
    }

    private void SpawnExitHole(MapInfo mapInfo)
    {
        if (mapInfo.listExitsInMap == null)
            return;

        for (int i = 0; i < mapInfo.listExitsInMap.Count; i++)
        {
            ExitHoleDataInMap exitholeData = mapInfo.listExitsInMap[i];

            tempVector3 = mapHelper.GetWorldPositionFromIndex(exitholeData.indexInMap);
            GameObject objExitHole = Instantiate(exitPrefab[exitholeData.color], tempVector3, Quaternion.identity, transform);

            ExitsObstacle exitHoleObstacle = objExitHole.GetComponent<ExitsObstacle>();

            if (exitHoleObstacle != null)
            {
                exitHoleObstacle.Init(exitholeData, this);
                listExitsObstacle.Add(exitHoleObstacle);
            }
        }
    }

    private void SpawnStop(MapInfo mapInfo)
    {
        if (mapInfo.listStopInMap == null)
            return;

        for (int i = 0; i < mapInfo.listStopInMap.Count; i++)
        {
            StopDataInMap StopData = mapInfo.listStopInMap[i];

            tempVector3 = mapHelper.GetWorldPositionFromIndex(StopData.indexInMap);
            GameObject objStop = Instantiate(StopPrefab, tempVector3, Quaternion.identity, transform);

            StopObstacle stopObstacle = objStop.GetComponent<StopObstacle>();

            if (stopObstacle != null)
            {
                stopObstacle.Init(StopData, this);
                listStopObstacle.Add(stopObstacle);
            }
        }
    }

    private void SpawnBarrier(MapInfo mapInfo)
    {
        if (mapInfo.listBarrierInMap == null)
            return;

        for (int i = 0; i < mapInfo.listBarrierInMap.Count; i++)
        {
            BarrierDataInMap barrierData = mapInfo.listBarrierInMap[i];

            tempVector3 = mapHelper.GetWorldPositionFromIndex(barrierData.indexInMap);
            GameObject objBarrier = Instantiate(barrierPrefab, tempVector3, Quaternion.identity, transform);

            BarrierObstacle barrierObstacle = objBarrier.GetComponent<BarrierObstacle>();

            if (barrierObstacle != null)
            {
                barrierObstacle.Init(barrierData, this, i);
                listBarrierObstacle.Add(barrierObstacle);
            }
        }
    }

    private void SetInfoMap(MapInfo mapInfo)
    {
        dictDots = new Dictionary<int, DotItem>();

        // Tạo node map rỗng
        int totalNode = mapInfo.column * mapInfo.row;
        for (int i = 0; i < totalNode; i++)
        {
            NodeMap nodeData = new NodeMap(i, TypeNodeValueInMap.EMPTY);
            listNodeMap.Add(nodeData);

            var dot = Instantiate(dotPrefab, transform);
            dot.transform.position = mapHelper.GetWorldPositionFromIndex(i);
            dot.Init(i);
            dot.gameObject.SetActive(false);
            dictDots.Add(i, dot);
        }

        // Gán node gecko
        try
        {
            for (int i = 0; i < mapInfo.listGeckoInMap.Count; i++)
            {
                var dataGecko = mapInfo.listGeckoInMap[i];

                for (int j = 0; j < dataGecko.listNode.Count; j++)
                {
                    var valueNode = dataGecko.listNode[j];
                    //listNodeMap[valueNode].typeNodeValueInMap = TypeNodeValueInMap.NODE_GECKO;
                    listNodeMap[valueNode].SetValueNote(TypeNodeValueInMap.NODE_GECKO);
                    //dictDots[valueNode].gameObject.SetActive(true);
                }
            }
        }
        catch
        {
            DebugCustom.ShowDebugColorRed("Lỗi gán node gecko trong map");
        }

        // Gán node Brick
        if (mapInfo.listBrickInMap != null)
        {
            for (int i = 0; i < mapInfo.listBrickInMap.Count; i++)
            {
                var valueNode = mapInfo.listBrickInMap[i].indexInMap;
                //listNodeMap[valueNode].typeNodeValueInMap = TypeNodeValueInMap.OBSTACLE_BRICK;
                listNodeMap[valueNode].SetValueNote(TypeNodeValueInMap.OBSTACLE_BRICK);
            }
        }

        // Gán node ExitHole 
        if (mapInfo.listExitsInMap != null)
        {
            for (int i = 0; i < mapInfo.listExitsInMap.Count; i++)
            {
                var valueNode = mapInfo.listExitsInMap[i].indexInMap;
                //listNodeMap[valueNode].typeNodeValueInMap = TypeNodeValueInMap.OBSTALCE_EXIT_HOLE;

                listNodeMap[valueNode].SetValueNote(TypeNodeValueInMap.OBSTALCE_EXIT_HOLE);
            }
        }

        // Gán node Stop
        if (mapInfo.listStopInMap != null)
        {
            for (int i = 0; i < mapInfo.listStopInMap.Count; i++)
            {
                var valueNode = mapInfo.listStopInMap[i].indexInMap;
                //listNodeMap[valueNode].typeNodeValueInMap = TypeNodeValueInMap.OBSTALCE_STOP;
                listNodeMap[valueNode].SetValueNote(TypeNodeValueInMap.OBSTALCE_STOP);
            }
        }

        // Gán node Barrier
        if (mapInfo.listBarrierInMap != null)
        {
            for (int i = 0; i < mapInfo.listBarrierInMap.Count; i++)
            {
                var valueNode = mapInfo.listBarrierInMap[i].indexInMap;
                //listNodeMap[valueNode].typeNodeValueInMap = TypeNodeValueInMap.OBSTALCE_BARRIER;
                listNodeMap[valueNode].SetValueNote(TypeNodeValueInMap.OBSTALCE_BARRIER);
                //DebugCustom.ShowDebugColor("Node barrier", valueNode);
            }
        }

        lgamePlay.SetTime(GetTimeLevel(mapInfo));
        lgamePlay.SetTypeLevel(mapInfo.typeMap);
        CheckHideBoosterNonUse();
    }

    private float GetTimeLevel(MapInfo mapInfo)
    {
        float timeTemp = 120;
        if (mapInfo.timeLimit < 10)
        {
            // Nhỏ hơn level 30 mặc định là 5p
            if (levelCurrent <= 30)
            {
                timeTemp = 300;
            }
            else if (mapInfo.score < 85)
            {
                timeTemp = 180;
            }
            else if (mapInfo.score < 110)
            {
                timeTemp = 240;
            }
            else if (mapInfo.score < 130)
            {
                timeTemp = 300;
            }
            //else if (mapInfo.score < 150)
            //{
            //    timeTemp = 360;
            //}
            else
            {
                timeTemp = 300;
            }
        }

        // Level cao map kho them 1p
        if (levelCurrent > 60)
            timeTemp += 60;

        return timeTemp;
    }

    private void LoadBgGame()
    {
        for (int i = 0; i < listPlanObj.Count; i++)
        {
            listPlanObj[i].gameObject.SetActive(false);
        }

        int indexOpenBg = 0;
        indexOpenBg = Random.Range(0, 3);
        listPlanObj[indexOpenBg].SetActive(true);
    }
    #endregion

    #region Logic Map

    private void CheckHideBoosterNonUse()
    {
        if (listBarrierObstacle.Count < 1)
        {
            LGamePlay.Instance.HideBooster(Booster.BARRIER_DESTROY);
        }

        if (listBrickObstacle.Count < 1)
        {
            LGamePlay.Instance.HideBooster(Booster.BRICK_DESTROY);
        }
    }

    public NodeMap GetNode(int index)
    {
        NodeMap nodeTemp = null;
        if (index < 0 || index >= listNodeMap.Count)
            return nodeTemp;

        nodeTemp = listNodeMap[index];

        return nodeTemp;
    }

    public void RemoveNodeOfGecko(Gecko gecko)
    {
        if (gecko == null || gecko.dataGeckoInMap == null)
            return;

        List<int> listNode = gecko.dataGeckoInMap.listNode;

        for (int i = 0; i < listNode.Count; i++)
        {
            int index = listNode[i];

            if (index >= 0 && index < listNodeMap.Count)
            {
                //listNodeMap[index].typeNodeValueInMap = TypeNodeValueInMap.EMPTY;
                listNodeMap[index].UpdateNote(TypeNodeValueInMap.EMPTY);
            }
        }

        listGecko.Remove(gecko);

        DebugCustom.ShowDebug("Đã cập nhật map – xóa toàn bộ node của gecko theo listNode.");
    }

    private bool IsNodeCanMove(NodeMap nodeTemp)
    {
        if (nodeTemp == null)
            return false;

        if (nodeTemp.typeNodeValueInMap == TypeNodeValueInMap.OBSTACLE_BRICK)
            return false;

        if (nodeTemp.typeNodeValueInMap == TypeNodeValueInMap.OBSTALCE_STOP)
        {
            var stopTemp = GetStopObstacle(nodeTemp.indexNodeInMap);

            if (stopTemp == null)
            {
                DebugCustom.LogError("Big Error with cell stop");
                return true;
            }

            if (stopTemp.IsContentHeadGecko() == true)
            {
                return false;
            }

            // là node stop, không chưa đầu nhung lại chưa thân của gecko khác thì cũng ko di chuyển được.
            if (nodeTemp.GetValueNoteBonus().Count > 0)
            {
                return false;
            }
        }

        if (nodeTemp.typeNodeValueInMap == TypeNodeValueInMap.OBSTALCE_BARRIER)
        {
            var barrierTemp = GetBarrierObstacle(nodeTemp.indexNodeInMap);
            if (barrierTemp.IsOpen() == false)
            {
                DebugCustom.ShowDebugColorRed("isOpen false note move");

                return false;
            }
        }

        if (nodeTemp.typeNodeValueInMap == TypeNodeValueInMap.NODE_GECKO)
        {
            return false;
        }

        return true;
    }

    public BarrierObstacle GetBarrierObstacle(int index)
    {
        if (listBarrierObstacle.Count < 1)
            return null;

        for (int i = 0; i < listBarrierObstacle.Count; i++)
        {
            if (listBarrierObstacle[i].indexInMap == index)
                return listBarrierObstacle[i];
        }

        return null;
    }

    public StopObstacle GetStopObstacle(int index)
    {
        if (listStopObstacle.Count < 1)
            return null;

        for (int i = 0; i < listStopObstacle.Count; i++)
        {
            if (listStopObstacle[i].indexInMap == index)
                return listStopObstacle[i];
        }

        return null;
    }

    public ExitsObstacle GetExitObstacle(int indexInMap)
    {
        for (int i = 0; i < listExitsObstacle.Count; i++)
        {
            if (listExitsObstacle[i].indexInMap == indexInMap)
            {
                return listExitsObstacle[i];
            }
        }
        return null;
    }

    public int GetLastNodeForward(Gecko gecko)
    {
        // Lấy grid helper
        int rows = mapHelper.row;
        int cols = mapHelper.column;

        // Lấy đầu và hướng
        int headIndex = gecko.dataGeckoInMap.listNode[0];
        DirectionMove dir = gecko.directionStart;

        // Chuyển hướng thành delta
        int dx = 0;
        int dz = 0;
        switch (dir)
        {
            case DirectionMove.UP: dz = 1; break;
            case DirectionMove.DOWN: dz = -1; break;
            case DirectionMove.LEFT: dx = -1; break;
            case DirectionMove.RIGHT: dx = 1; break;
        }

        // Chuyển index → (x,y)
        int x = headIndex % cols;
        int y = headIndex / cols;

        int lastIndex = headIndex;

        while (true)
        {
            // Tiến 1 bước
            x += dx;
            y += dz;

            // Check ra khỏi map
            if (x < 0 || x >= cols || y < 0 || y >= rows)
                return lastIndex;

            int idx = y * cols + x;

            lastIndex = idx;
        }
    }
    #endregion

    #region Logic Game

    private void RequestGeckoEscape(Gecko gecko)
    {
        var actionGecko = GetStateNextGecko(gecko);
        gecko.StartState(actionGecko);

        DebugCustom.ShowDebugColorRed("action Next:" + actionGecko.stateNext);

        AudioManager.Instance.ClickGecko();

        if (objNoticeFindGecko != null)
            objNoticeFindGecko.gameObject.SetActive(false);
    }

    private void DetectGecko(Vector3 posClick)
    {
        // 1. convert screen → world
        Vector3 worldPos = GetWorldPositionFromClick(posClick);

        // 2. convert world → index
        int index = mapHelper.WorldPosToIndex(worldPos);

        if (EventSystem.current.IsPointerOverGameObject())
        {
            index = -1;
            return;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                index = -1;
                return;
            }
        }

        if (stateCurrentGame == StateGame.PAUSE || stateCurrentGame == StateGame.END)
        {
            return;
        }

        // Tut user flollower different
        if (isTut)
        {
            DetectGeckoWithTut(index);
            return;
        }

        //Normal follow
        if (boosterUse != Booster.NONE)
        {
            DetectBooster(index);

            RequestStartCountTimeGame();

        }
        else
        {
            var geckoTemp = GetGeckoWithIndex(index);

            if (geckoTemp != null && geckoTemp.IsCanNewState())
            {
                RequestGeckoEscape(geckoTemp);

                RequestStartCountTimeGame();
            }
        }
    }

    public Vector3 GetWorldPositionFromClick(Vector3 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float dist;

        if (plane.Raycast(ray, out dist))
        {
            return ray.GetPoint(dist);
        }

        return Vector3.zero;
    }

    public Gecko GetGeckoWithIndex(int index)
    {
        for (int i = 0; i < listGecko.Count; i++)
        {
            Gecko geckoTemp = listGecko[i];
            if (geckoTemp.dataGeckoInMap.listNode.Contains(index))
            {
                return geckoTemp;
            }
        }
        return null;
    }

    private GeckoAction GetStateNextGecko(Gecko gecko)
    {
        GeckoAction geckoAction = new GeckoAction();
        geckoAction.stateNext = StateGecko.MOVE_FAIL;
        geckoAction.nodeTarget = GetNode(gecko.indexHead);

        int currentIndex = gecko.indexHead;
        DirectionMove dir = gecko.directionStart;

        //DebugCustom.ShowDebugColorRed("current index", currentIndex);
        //DebugCustom.ShowDebug("dir", dir);

        while (true)
        {
            // Lấy index tiếp theo theo hướng
            int nextIndex = -1;

            switch (dir)
            {
                case DirectionMove.UP:
                    nextIndex = mapHelper.GetUp(currentIndex);
                    break;
                case DirectionMove.DOWN:
                    nextIndex = mapHelper.GetDown(currentIndex);
                    break;
                case DirectionMove.LEFT:
                    nextIndex = mapHelper.GetLeft(currentIndex);
                    break;
                case DirectionMove.RIGHT:
                    nextIndex = mapHelper.GetRight(currentIndex);
                    break;
            }

            // Nếu nextIndex = -1 => ra khỏi map => THOÁT
            if (nextIndex == -1)
            {
                geckoAction.stateNext = StateGecko.ESCAPE_OUT_MAP;
                return geckoAction;
            }

            //DebugCustom.ShowDebug("Node index check", nextIndex);


            // Lấy node map
            NodeMap node = GetNode(nextIndex);

            // Nếu có node 
            if (node != null)
            {
                //Nếu là node bị chặn ⇒ KO THOÁT
                if (IsNodeCanMove(node) == false)
                {
                    geckoAction.nodeTarget = node;
                    geckoAction.stateNext = StateGecko.MOVE_FAIL;
                    geckoAction.nodeFail = node;
                    return geckoAction;
                }
                else
                {
                    //geckoAction.nodeFail = node;
                }

                // Nếu là node stop thì check xem đi tới node stop dc ko
                if (node.typeNodeValueInMap == TypeNodeValueInMap.OBSTALCE_STOP)
                {
                    var stopObstacleTemp = GetStopObstacle(node.indexNodeInMap);

                    if (stopObstacleTemp.IsContentHeadGecko() == false)
                    {
                        geckoAction.nodeTarget = node;
                        geckoAction.stateNext = StateGecko.MOVE_TO_STOP_OBSTACLE;
                        return geckoAction;
                    }
                }

                else if (node.typeNodeValueInMap == TypeNodeValueInMap.OBSTALCE_EXIT_HOLE)
                {
                    // check exithole cùng màu với gecko thì escape
                    var exitHoleObstace = GetExitObstacle(node.indexNodeInMap);

                    if (exitHoleObstace != null)
                    {

                        if (gecko.dataGeckoInMap.colorGecko == exitHoleObstace.value)
                        {

                            geckoAction.nodeTarget = node;
                            geckoAction.stateNext = StateGecko.ESCAPE_EXIT_HOLE;
                            return geckoAction;
                        }

                    }
                }
            }


            // Nếu node rỗng ⇒ bước tiếp
            currentIndex = nextIndex;
        }

        return null;
    }

    private void OnGeckoMoveOutMapSucess(object param)
    {

    }

    private void OnGeckoOutMapSucess(object param)
    {
        countGeckoCurrent--;

        if (countGeckoCurrent <= 0)
        {
            SetEndGame(true);
        }
        else
        {
            var tempGeckoFind = FindGeckoEscape();
            if (tempGeckoFind == null)
            {
                lgamePlay.toastIngame.ShowToast(GameConfig.NotFindGeckoEscapse);
            }
        }
    }

    public void SetMoveFail()
    {
        LGamePlay.Instance.RequestMoveFail();
    }

    public void ShowDirectionLine()
    {
        if (isShowLine)
        {
            HideDirectionLine();
        }
        else
        {
            isShowLine = true;
            listGecko.ForEach(x => x.OnOffLineHead(true));
        }
    }

    public void HideDirectionLine()
    {
        isShowLine = false;
        listGecko.ForEach(x => x.OnOffLineHead(false));
    }


    #endregion

    #region Obstacle

    // Brick Obstacle
    public void RemoveBrickObstacle(BrickObstacle brick)
    {
        // Update map
        DebugCustom.ShowDebugColor("remove brick", brick.indexInMap);

        //GetNode(brick.indexInMap).typeNodeValueInMap = TypeNodeValueInMap.EMPTY;
        GetNode(brick.indexInMap).UpdateNote(TypeNodeValueInMap.EMPTY);
        if (listBrickObstacle.Contains(brick))
        {
            listBrickObstacle.Remove(brick);

            //brick.RemoveObstacle();

            CheckHideBoosterNonUse();
        }
    }

    public ExitsObstacle GetObstacleExits(NodeMap node)
    {
        for (int i = 0; i < listExitsObstacle.Count; i++)
        {
            if (node.indexNodeInMap == listExitsObstacle[i].indexInMap)
            {
                return listExitsObstacle[i];
            }
        }

        return null;
    }
    #endregion

    #region Logic ObstacleStop Move 

    public List<int> GetPathForward(int indexStart, int indexStop, DirectionMove dir)
    {
        List<int> path = new List<int>();

        int current = indexStart;

        while (true)
        {
            int next = -1;

            if (dir == DirectionMove.UP)
            {
                next = mapHelper.GetUp(current);
            }
            else if (dir == DirectionMove.DOWN)
            {
                next = mapHelper.GetDown(current);
            }
            else if (dir == DirectionMove.LEFT)
            {
                next = mapHelper.GetLeft(current);
            }
            else if (dir == DirectionMove.RIGHT)
            {
                next = mapHelper.GetRight(current);
            }

            // Không tới được stop hoặc ra khỏi map
            if (next == -1)
            {
                break;
            }

            path.Add(next);

            if (next == indexStop)
            {
                break;
            }

            current = next;
        }

        return path;
    }

    public List<int> SimulateGeckoMoveToTarget(List<int> listNodeCurrent, int indexHeadCurrent, int indexHeadTarget, DirectionMove dir)
    {
        // 1. Copy listNode hiện tại để không sửa trực tiếp
        List<int> newListNode = new List<int>(listNodeCurrent);

        // 2. Lấy đường đi từ head → target
        List<int> pathForward = GetPathForward(indexHeadCurrent, indexHeadTarget, dir);

        // 3. Mỗi ô trong path là 1 bước tiến
        for (int i = 0; i < pathForward.Count; i++)
        {
            int nextHeadIndex = pathForward[i];

            // Head chèn vào đầu list
            newListNode.Insert(0, nextHeadIndex);

            // Bỏ phần tử đuôi cuối cùng
            int lastIndex = newListNode.Count - 1;
            newListNode.RemoveAt(lastIndex);
        }

        return newListNode;
    }

    public void UpdateMapAfterSimulateMove(Gecko gecko, List<int> newListNode)
    {
        // 1. Xoá dấu vết cũ của gecko trên map
        List<int> oldList = gecko.dataGeckoInMap.listNode;
        for (int i = 0; i < oldList.Count; i++)
        {
            int indexOld = oldList[i];

            bool inRange = indexOld >= 0 && indexOld < listNodeMap.Count;
            if (inRange)
            {
                //listNodeMap[indexOld].typeNodeValueInMap = TypeNodeValueInMap.EMPTY;
                listNodeMap[indexOld].UpdateNote(TypeNodeValueInMap.EMPTY);
            }
        }

        // 2. Đánh dấu lại toàn bộ node mới
        for (int i = 0; i < newListNode.Count; i++)
        {
            int indexNew = newListNode[i];

            bool inRange = indexNew >= 0 && indexNew < listNodeMap.Count;
            if (inRange)
            {
                //listNodeMap[indexNew].typeNodeValueInMap = TypeNodeValueInMap.NODE_GECKO;

                listNodeMap[indexNew].UpdateNote(TypeNodeValueInMap.NODE_GECKO);
            }
        }

        // 3. Ghi ngược lại vào gecko
        gecko.dataGeckoInMap.listNode = newListNode;
        gecko.dataGeckoInMap.indexHead = newListNode[0];
        gecko.indexHead = gecko.dataGeckoInMap.indexHead;

    }

    #endregion

    #region Dot
    public void DotPlayAnim(int index)
    {
        //DebugCustom.Log("DotPlayAnim = " + index);
        if (dictDots != null && dictDots.ContainsKey(index))
        {
            //DebugCustom.Log("OK PLAY = " + index);
            dictDots[index].PlayAnim();
        }
    }

    public void ActiveDot(Gecko gecko)
    {
        for (int i = 0; i < gecko.dataGeckoInMap.listNode.Count; i++)
        {
            dictDots[gecko.dataGeckoInMap.listNode[i]].gameObject.SetActive(true);
        }
    }

    List<List<Vector2Int>> DotBoomOrderTopLeft(int width, int height)
    {
        var visited = new bool[width, height];
        var result = new List<List<Vector2Int>>();
        var queue = new Queue<Vector2Int>();

        // Góc trên trái (theo Unity trục dưới lên trên)
        Vector2Int start = new Vector2Int(0, height - 1);

        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        int[] dx = { 1, -1, 0, 0, 1, 1, -1, -1 };
        int[] dy = { 0, 0, 1, -1, 1, -1, 1, -1 };

        while (queue.Count > 0)
        {
            int count = queue.Count;         // số điểm trong vòng hiện tại
            var ring = new List<Vector2Int>(); // danh sách điểm của vòng này

            for (int i = 0; i < count; i++)
            {
                var p = queue.Dequeue();
                ring.Add(p);

                // Thêm hàng xóm
                for (int d = 0; d < 8; d++)
                {
                    int nx = p.x + dx[d];
                    int ny = p.y + dy[d];

                    if (nx >= 0 && ny >= 0 && nx < width && ny < height && !visited[nx, ny])
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }

            result.Add(ring);
        }

        return result;
    }

    List<List<Vector2Int>> DotBoomFast(int width, int height)
    {
        int maxRing = Mathf.Max(width, height);
        var result = new List<List<Vector2Int>>(maxRing);

        for (int i = 0; i < maxRing; i++)
            result.Add(new List<Vector2Int>());

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int ring = Mathf.Max(x, (height - 1) - y);
                result[ring].Add(new Vector2Int(x, y));
            }

        return result;
    }
    #endregion

    #region UI and Event
    public void RequestStartCountTimeGame()
    {
        if (isStartCountTimeGame == false)
        {
            isStartCountTimeGame = true;
            SimpleEventManager.Instance.PostEvent(EventIDSimple.startgame);
            timeStartGame = Time.realtimeSinceStartup;

            if (stateCurrentGame == StateGame.NONE)
                stateCurrentGame = StateGame.PLAYING;
        }
    }

    public void SetEndGame(bool isWin)
    {

        //return;

        stateCurrentGame = StateGame.END;

        if (isWin)
        {
            GameData.userData.profile.EndStage(isWin);

            float timeShowCamera = 0.35f;
            CameraController.Instance.PlayAnimEndgame(timeShowCamera);

            this.StartDelayAction(timeShowCamera + 0.1f, () =>
            {
                objFirework.SetActive(true);
            });

            // diễn anim win
            float interval = GameConfig.dotSpeed * 0.4f;

            StartCoroutine(IEPlayAnimEndGame(mapHelper.column, mapHelper.row, interval));
        }
        else
        {
            AudioManager.Instance.PlayLose();
        }

        LGamePlay.Instance.StopTimer();

        //  Log Event
        try
        {
            if (isWin)
            {
                AppsflyerHelper.SendLevelAchievedEvent(levelCurrent, 0);

                if (levelCurrent <= 50)
                    FirebaseAnalyticsHelper.LogEventCheckPoint(levelCurrent);

                FirebaseAnalyticsHelper.LogLevelComplete(levelCurrent, timeStartGame);
            }
            else
            {
                FirebaseAnalyticsHelper.LogLevelFail(levelCurrent);
            }
        }
        catch
        {

        }
    }

    private void ShowPopupEndGame(bool isWin)
    {
        if (isWin)
        {
            if (LGamePlay.Instance != null)
            {
                LGamePlay.Instance.StopTimer();
            }

            List<RewardData> rewards = GameData.GetWinRewards(1);
            LWin ui = UIManager.Instance.LoadUI(UIKey.WIN) as LWin;
            AudioManager.Instance.PauseMusic();
            ui.Open(isWin, rewards);
        }
    }

    private void OnRequestPauseGameUI(object para)
    {
        stateCurrentGame = StateGame.PAUSE;
        LGamePlay.Instance.StopTimer();
    }

    private void OnRequestUnPauseGameUI(object para)
    {
        if (stateCurrentGame != StateGame.END)
        {
            stateCurrentGame = StateGame.PLAYING;
            if (LGamePlay.Instance != null)
            {
                LGamePlay.Instance.CountinueTimer();
            }
        }
    }

    // IE
    private IEnumerator IEPlayAnimEndGame(int width, int height, float interval)
    {
        AudioManager.Instance.PlayWin();

        int maxRing = Mathf.Max(width, height);

        float timeDelay = 2f;
        if (interval * maxRing > timeDelay)
        {
            interval = timeDelay / maxRing;
        }

        // play
        var visited = new bool[width, height];
        var queue = new Queue<Vector2Int>(width * height / 4);

        // góc trên trái (Unity: y tăng lên trên)
        Vector2Int start = new Vector2Int(0, height - 1);

        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        int[] dx = { 1, -1, 0, 0, 1, 1, -1, -1 };
        int[] dy = { 0, 0, 1, -1, 1, -1, 1, -1 };

        while (queue.Count > 0)
        {
            int count = queue.Count;

            // 👉 VÒNG NỔ HIỆN TẠI
            for (int i = 0; i < count; i++)
            {
                var p = queue.Dequeue();

                // 💥 chạy anim nổ ngay tại đây
                int indexDot = p.y * width + p.x;
                if (dictDots.ContainsKey(indexDot))
                {
                    dictDots[indexDot].PlayAnimWin(0);
                }

                // BFS 8 hướng
                for (int d = 0; d < 8; d++)
                {
                    int nx = p.x + dx[d];
                    int ny = p.y + dy[d];

                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        continue;

                    if (visited[nx, ny])
                        continue;

                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }

            // ⏱ delay giữa các vòng nổ
            yield return new WaitForSeconds(interval);
        }

        yield return new WaitForSeconds(0.2f);
        ShowPopupEndGame(true);
    }

    public float GetProgressPercent()
    {
        if (totalGecko <= 0)
        {
            return 0;
        }

        return (float)(totalGecko - countGeckoCurrent) / totalGecko;
    }

    #endregion

    #region Booster

    public void RequestUseBooster(ItemType typeBooster)
    {

        if (IsCanUseBooster(typeBooster) == false)
        {
            // Thông báo booster này ko dùng được cho map này
            return;
        }

        DebugCustom.ShowDebug("Request Use booster", typeBooster);



        listIntTemp.Clear();

        switch (typeBooster)
        {
            case ItemType.ESCAPE_GECKO:
                boosterUse = Booster.ESCAPES;

                for (int i = 0; i < listGecko.Count; i++)
                {
                    listIntTemp.Add(listGecko[i].dataGeckoInMap.indexHead);
                }
                SpawnNotice(listIntTemp);


                break;
            case ItemType.BARRIER:

                boosterUse = Booster.BARRIER_DESTROY;

                for (int i = 0; i < listBarrierObstacle.Count; i++)
                {
                    listIntTemp.Add(listBarrierObstacle[i].indexInMap);
                }
                SpawnNotice(listIntTemp);

                break;
            case ItemType.TIME_STOP_ICE:

                //boosterUse = Booster.TIMESTOP;
                break;
            case ItemType.FIND_GECKO_MOVE:

                var geckoEscapse = FindGeckoEscape();

                if (geckoEscapse == null)
                {
                    CancelBoosterUse(ItemType.FIND_GECKO_MOVE);
                    UIManager.Instance.ShowToastMessage(GameConfig.NotFindGeckoEscapse);
                }
                else
                {
                    ShowGeckoFindOut(geckoEscapse);

                }


                break;
            case ItemType.BRICK_BROKEN:

                boosterUse = Booster.BRICK_DESTROY;

                for (int i = 0; i < listBrickObstacle.Count; i++)
                {
                    listIntTemp.Add(listBrickObstacle[i].indexInMap);
                }
                SpawnNotice(listIntTemp);

                break;

        }
    }

    // cap nhat lai thong tin booter data o day
    public void DoneUserBooster(ItemType typeBooster)
    {

        LGamePlay.Instance.DoneUseBooster(typeBooster);


        switch (typeBooster)
        {
            case ItemType.ESCAPE_GECKO:
                OffNoticeBooster();
                boosterUse = Booster.NONE;
                break;
            case ItemType.BARRIER:
                OffNoticeBooster();
                boosterUse = Booster.NONE;
                break;
            case ItemType.TIME_STOP_ICE:

                break;
            case ItemType.FIND_GECKO_MOVE:

                break;
            case ItemType.BRICK_BROKEN:
                OffNoticeBooster();
                boosterUse = Booster.NONE;
                break;

        }

        CheckHideBoosterNonUse();
    }

    // Khi huy dung booster thi goi
    public void CancelBoosterUse(ItemType typeBooster)
    {
        boosterUse = Booster.NONE;
        OffNoticeBooster();
    }

    private bool IsCanUseBooster(ItemType typeBooster)
    {
        return true;
    }

    private void DetectBooster(int indexInMapTemp)
    {
        switch (boosterUse)
        {
            case Booster.ESCAPES:

                var geckoTemp = GetGeckoWithIndex(indexInMapTemp);

                if (geckoTemp != null && geckoTemp.IsCanNewState())
                {
                    GeckoAction geckoAction = new GeckoAction();
                    geckoAction.stateNext = StateGecko.ESCAPE_OUT_MAP;

                    int indexNodeLastTemp = GetLastNodeForward(geckoTemp);

                    if (indexNodeLastTemp < 0 || indexNodeLastTemp > listNodeMap.Count)
                    {
                        DebugCustom.ShowDebugColorRed("Error big");
                        return;
                    }
                    for (int i = 0; i < listNodeMap.Count; i++)
                    {
                        if (listNodeMap[i].indexNodeInMap == indexNodeLastTemp)
                        {
                            geckoAction.nodeTarget = listNodeMap[i];
                            break;
                        }
                    }

                    geckoTemp.StartState(geckoAction);
                    DoneUserBooster(ItemType.ESCAPE_GECKO);

                    if (objNoticeFindGecko != null)
                        objNoticeFindGecko.gameObject.SetActive(false);
                }

                break;
            case Booster.TIMESTOP:

                //DoneUserBooster(ItemType.TIME_STOP_ICE);


                break;
            case Booster.BRICK_DESTROY:
                GameObject objTemp = null;
                for (int i = 0; i < listBrickObstacle.Count; i++)
                {
                    if (listBrickObstacle[i].indexInMap == indexInMapTemp)
                    {
                        objTemp = listBrickObstacle[i].gameObject;

                        // huy bỏ brick 
                        listBrickObstacle.RemoveAt(i);

                    }
                }

                if (objTemp != null)
                {
                    // Update Node Map
                    for (int i = 0; i < listNodeMap.Count; i++)
                    {
                        if (listNodeMap[i].indexNodeInMap == indexInMapTemp)
                        {
                            listNodeMap[i].RemoveBrick();
                        }
                    }

                    FxController.Instance.ShowBroken(objTemp.transform.position, () =>
                    {
                        //spawn Fx break ice
                        var objFxTemp = PoolingController.Instance.GetGameObjectFromPool(TypeObject.FX_ICE_BREAK, FxController.Instance.objBreakIcePrefab);

                        objFxTemp.transform.position = objTemp.transform.position;
                        objFxTemp.gameObject.SetActive(true);
                        AudioManager.Instance.PlayBreakBrick();

                        Destroy(objTemp);
                    });

                    DoneUserBooster(ItemType.BRICK_BROKEN);
                }

                break;
            case Booster.BARRIER_DESTROY:
                objTemp = null;
                for (int i = 0; i < listBarrierObstacle.Count; i++)
                {
                    if (listBarrierObstacle[i].indexInMap == indexInMapTemp)
                    {
                        objTemp = listBarrierObstacle[i].gameObject;

                        // huy bỏ brick 
                        listBarrierObstacle.RemoveAt(i);
                    }
                }

                if (objTemp != null)
                {
                    // Update Node Map
                    for (int i = 0; i < listNodeMap.Count; i++)
                    {
                        if (listNodeMap[i].indexNodeInMap == indexInMapTemp)
                        {
                            listNodeMap[i].RemoveBarrier();
                        }
                    }

                    FxController.Instance.ShowDestroyBarrier(objTemp.transform.position, () =>
                    {
                        Destroy(objTemp);
                    });

                    DoneUserBooster(ItemType.BARRIER);
                }


                break;
        }

        //OnRequestStartGame();
    }

    private void SpawnNotice(List<int> listIndexSpawn)
    {
        for (int i = 0; i < listIndexSpawn.Count; i++)
        {
            tempVector3 = mapHelper.GetWorldPositionFromIndex(listIndexSpawn[i]);
            var objTemp = GetObjNotice();
            objTemp.SetActive(true);
            objTemp.transform.position = tempVector3;
        }
    }

    private void OffNoticeBooster()
    {
        for (int i = 0; i < listObjectNoticeBooster.Count; i++)
        {
            listObjectNoticeBooster[i].SetActive(false);
        }
    }

    private GameObject GetObjNotice()
    {
        for (int i = 0; i < listObjectNoticeBooster.Count; i++)
        {
            if (listObjectNoticeBooster[i].gameObject.activeSelf == false)
                return listObjectNoticeBooster[i];
        }

        var obj = Instantiate(objNoticeBooster);

        listObjectNoticeBooster.Add(obj);
        return obj;
    }

    private void ShowGeckoFindOut(Gecko geckoFind)
    {
        if (objNoticeFindGecko == null)
        {
            objNoticeFindGecko = Instantiate(objNoticeBooster);
        }
        objNoticeFindGecko.SetActive(false);

        tempVector3 = mapHelper.GetWorldPositionFromIndex(geckoFind.indexHead);
        objNoticeFindGecko.transform.position = tempVector3;
        objNoticeFindGecko.gameObject.SetActive(true);

        float timeMove = 0.5f;
        mainCamera.MoveToTarget(tempVector3, timeMove);

        this.StartDelayAction(timeMove, () =>
        {
            DoneUserBooster(ItemType.FIND_GECKO_MOVE);
        });

    }

    #endregion

    #region Logic Basic Map

    public Vector2 GetDirection(Gecko geckoTemp)
    {
        if (geckoTemp == null)
        {
            DebugCustom.ShowDebugColorRed("Error gecko null");
            return Vector2.zero;
        }

        int rows = mapHelper.row;
        int cols = mapHelper.column;

        int head = geckoTemp.dataGeckoInMap.listNode[0];
        int neck = geckoTemp.dataGeckoInMap.listNode[1];

        int headX = head % cols;
        int headY = head / cols;

        int neckX = neck % cols;
        int neckY = neck / cols;

        // Determine direction (dx, dy)
        int dx = headX - neckX;
        int dy = headY - neckY;

        return new Vector2(dx, dy);
    }

    #endregion

    public void OnMoveSucess(object objTemp)
    {
        this.StartDelayAction(0.1f, () =>
        {
            if (countGeckoCurrent > 1)
            {
                var tempGeckoFind = FindGeckoEscape();
                if (tempGeckoFind == null)
                {
                    lgamePlay.toastIngame.ShowToast(GameConfig.NotFindGeckoEscapse);
                }
            }
        });

    }

    // ------------- Non Gameplay Code ------------------------------
    #region GD GAME

    private void UpdateEditor()
    {

#if UNITY_EDITOR

        if (Input.GetKeyDown(KeyCode.W))
        {
            SetEndGame(true);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SetEndGame(false);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            RequestStartCountTimeGame();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            RequestUseBooster(ItemType.ESCAPE_GECKO);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            RequestUseBooster(ItemType.BARRIER);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            RequestUseBooster(ItemType.BRICK_BROKEN);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            RequestUseBooster(ItemType.TIME_STOP_ICE);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            FindGeckoEscapeTool();
        }

#endif

    }
    public void SetModeEditMap(bool isEditTemp)
    {
        isEditMode = isEditTemp;
    }

    public Gecko FindGeckoEscape()
    {
        for (int i = 0; i < listGecko.Count; i++)
        {
            var geckoTemp = listGecko[i];

            var stateTemp = GetStateNextGecko(geckoTemp);

            if (stateTemp.stateNext == StateGecko.ESCAPE_OUT_MAP || stateTemp.stateNext == StateGecko.ESCAPE_EXIT_HOLE
                || stateTemp.stateNext == StateGecko.MOVE_TO_STOP_OBSTACLE
                )
            {
                return geckoTemp;
            }
        }

        return null;
    }

    public void FindGeckoEscapeTool()
    {
        for (int i = 0; i < listGecko.Count; i++)
        {
            var geckoTemp = listGecko[i];

            var stateTemp = GetStateNextGecko(geckoTemp);

            if (stateTemp.stateNext == StateGecko.ESCAPE_OUT_MAP || stateTemp.stateNext == StateGecko.ESCAPE_EXIT_HOLE
                || stateTemp.stateNext == StateGecko.MOVE_TO_STOP_OBSTACLE
                )
            {
                RequestGeckoEscape(geckoTemp);
                break;
            }

        }
    }


    #endregion

    #region Tutorial

    public void SetTut(IDTutorial id)
    {
        if (id == IDTutorial.NONE)
            return;

        DebugCustom.ShowDebug("Tut Map1");

        switch (id)
        {
            case IDTutorial.BOOSTER_ESCAPSE:
            case IDTutorial.BOOSTER_STOP_TIME:
            case IDTutorial.BOOSTER_DESTROY_BRICK:
            case IDTutorial.BOOSTER_DESTROY_BARRIER:
            case IDTutorial.TUT_FEATURE_BARRIER:
            case IDTutorial.TUT_FEATURE_HOLE_EXIT:
            case IDTutorial.TUT_FEATURE_BRICK:
            case IDTutorial.TUT_FEATURE_STOP:
            case IDTutorial.SHOW_LINE_GECKO:

            case IDTutorial.ZOOM_OUT_IN:
                isTut = true;
                break;
        }

    }

    public void SetOffTut()
    {
        isTut = false;
    }

    private void DetectGeckoWithTut(int index)
    {
        switch (tutController.idTutCurrent)
        {
            case IDTutorial.BOOSTER_ESCAPSE:

                if (tutController.StepCurrent == 2)
                {
                    var geckoTemp = GetGeckoWithIndex(index);

                    if (geckoTemp == tutController.geckoTargetTut)
                    {

                        if (boosterUse != Booster.NONE)
                        {
                            DetectBooster(index);
                        }
                    }

                }
                break;
            case IDTutorial.BOOSTER_DESTROY_BRICK:
                if (tutController.StepCurrent == 2)
                {
                    BrickObstacle objBrickTemp = null;
                    for (int i = 0; i < listBrickObstacle.Count; i++)
                    {
                        if (listBrickObstacle[i].indexInMap == index)
                        {
                            objBrickTemp = listBrickObstacle[i];
                        }
                    }

                    if (objBrickTemp != null && objBrickTemp == tutController.brickTarget)
                    {
                        DetectBooster(index);

                        tutController.NextStep();
                    }
                }
                break;
            case IDTutorial.BOOSTER_DESTROY_BARRIER:
                if (tutController.StepCurrent == 2)
                {
                    BarrierObstacle objBarrierTemp = null;
                    for (int i = 0; i < listBarrierObstacle.Count; i++)
                    {
                        if (listBarrierObstacle[i].indexInMap == index)
                        {
                            objBarrierTemp = listBarrierObstacle[i];
                        }
                    }

                    if (objBarrierTemp != null && objBarrierTemp == tutController.barrierTarget)
                    {
                        DetectBooster(index);

                        tutController.NextStep();
                    }
                }
                break;

        }
    }
    #endregion

    #region Editor MKT order

    private void LoadMapMkt()
    {
        string keyData = GameConfig.keyDataMkt;

        int level = GameConfig.lvMkt;
        if (level < 1)
        {
            level = 1;
        }
        string dataString = "";

        if(GameConfig.lvMktProductPA > 0)
        {
            level = GameConfig.lvMktProductPA;
            string fileName = level.ToString("D2");
            string path = "";

            //-------------------load bytes + giải mã
            path = $"MapBytes/{fileName}.json";
            DebugCustom.ShowDebugColorRed("Load map: " + path);
            var ta = Resources.Load<TextAsset>(path);
            dataString = AESUtil.DecryptAES(ta.text);
            //--------------- end ma --------------------------------------------------
        }
        else
        {
            // Load map Save In data player
            dataString = PlayerPrefs.GetString(keyData + level.ToString(), "");
        }

        if (string.IsNullOrEmpty(dataString))
        {
            CreateMapDefaultMkt();

            Debug.Log("Auto create Map");
        }
        else
        {
            try
            {
                mapInfo = JsonConvert.DeserializeObject<MapInfo>(dataString);
            }
            catch
            {
                CreateMapDefaultMkt();
            }
        }

        if (GameConfig.indexBgMkt > 0)
        {
            for (int i = 0; i < listPlanObj.Count; i++)
            {
                listPlanObj[i].SetActive(false);
            }

            var indexLoadTemp = GameConfig.indexBgMkt;
            if (indexLoadTemp > listPlanObj.Count - 1)
            {

            }
            else
            {
                listPlanObj[indexLoadTemp].SetActive(true);
            }
        }
    }

    private void CreateMapDefaultMkt()
    {
        mapInfo = new MapInfo();
        mapInfo.column = 10;
        mapInfo.row = 10;

        mapInfo.listGeckoInMap = new List<GeckoDataInMap>();
        mapInfo.listBarrierInMap = new List<BarrierDataInMap>();
        mapInfo.listBrickInMap = new List<BrickDataInMap>();
        mapInfo.listExitsInMap = new List<ExitHoleDataInMap>();
        mapInfo.listStopInMap = new List<StopDataInMap>();
    }

    #endregion
}
