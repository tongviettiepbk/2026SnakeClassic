using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;

public enum StateGecko
{
    IDLE = 0,
    MOVE_FAIL = 1,
    ESCAPE_OUT_MAP = 3,
    ESCAPE_EXIT_HOLE = 4,
    ESCAPE_EXIT_HOLE_FINAL = 5,
    MOVE_TO_STOP_OBSTACLE = 6,
    ESCAPE_POWER_OUT_MAP = 7,
}

public enum MoveStateGecko
{
    MOVE = 0,
    IDLE = 1,
    MOVE_BACK = 2,
    NONE = 99,
}

public class Gecko : MonoBehaviour
{
    public GameObject objHeadPrefab;
    public GameObject objTailPrefab;
    public GameObject objGroupTail;
    public SpriteRenderer lineHead;
    public Color colorLineNormal;
    public Color colorLineError;

    public int indexHead { get; set; }
    public DirectionMove directionStart { get; private set; }
    public GeckoDataInMap dataGeckoInMap { get; private set; }

    private MapController mapController;
    private GameObject objHeadGecko = null;

    //[SerializeField] private GeckoBody geckoBody;
    [SerializeField] private List<GameObject> listObjTail = new List<GameObject>();
    private List<Vector3> listPosStart = new List<Vector3>();
    private List<Vector3> geckoPositions = new List<Vector3>();

    private GeckoPart geckoTailEnd = null;

    [SerializeField]
    private StateGecko stateCurrent = StateGecko.IDLE;
    [SerializeField]
    private MoveStateGecko moveStateGecko = MoveStateGecko.NONE;

    private float angleUp = 0;
    private float angleDown = 180;
    private float angleLeft = -90;
    private float angleRight = 90;

    private NodeMap targetNodeFinal;
    private NodeMap targetNodeFinalTemp;
    private Vector3 posTarget = new Vector3();
    private Vector3 dirHead = Vector3.zero;

    private StopObstacle stopObstacleContentGecko = null;
    private List<List<Vector3>> positionMoves = new List<List<Vector3>>();
    private int lastTailIndex = -1;

    private bool isCheckOutMapComplete = false;
    private float timeCount;
    private float timeCheckOutMap = 0.5f;

    private List<int> listIndexMoveTemp = new List<int>();
    private BaseFx fxStun = null;

    private bool isActiveDot = false;
    private bool isDie = false;

    private ExitsObstacle obstacleExit;

    public int indexInList = 0;

    public void Init(MapController mapController, GeckoDataInMap dataGeckoInMap, int index = -99)
    {
        this.mapController = mapController;
        this.dataGeckoInMap = dataGeckoInMap;
        SpawnGecko();
        SetInfoGecko();
        stopObstacleContentGecko = null;

        lineHead.color = colorLineNormal;

        isDie = false;
        this.indexInList = index;
    }

    private void Update()
    {
        UpdateMove();
    }

    private void SpawnGecko()
    {
        int indexPre = 0;

        if (dataGeckoInMap.listNode.Count <= 1)
        {
            DebugCustom.ShowDebugColorRed("Gecko error need than tow node");
            Destroy(this);
            return;
        }

        for (int i = 0; i < dataGeckoInMap.listNode.Count; i++)
        {
            int indexNode = dataGeckoInMap.listNode[i];
            if (i == 0)
            {
                indexHead = indexNode;
            }
            else if (i == 1)
            {
                indexPre = indexNode;
            }


            Vector3 posTemp = mapController.mapHelper.GetWorldPositionFromIndex(indexNode);
            listPosStart.Add(posTemp);
            geckoPositions.Add(posTemp);

            GeckoPart part = null;

            if (i == 0)
            {
                // đầu gecko
                objHeadGecko = Instantiate(objHeadPrefab, this.transform);
                objHeadGecko.transform.position = posTemp;
                part = objHeadGecko.GetComponent<GeckoPart>();
                objHeadGecko.GetComponent<GeckoPart>().Init(this, i, true);
                objHeadGecko.SetActive(true);

                lineHead = objHeadGecko.transform.GetChild(1).GetComponent<SpriteRenderer>();
                lineHead.color = colorLineNormal;

            }
            else if (i == dataGeckoInMap.listNode.Count - 1)
            {
                // Đuôi Gecko
                GameObject objTemp = Instantiate(objTailPrefab, this.transform);
                objTemp.transform.position = posTemp;

                geckoTailEnd = objTemp.GetComponent<GeckoPart>();
                part = geckoTailEnd;

                geckoTailEnd.Init(this, i, false, true);
                objTemp.SetActive(true);
                listObjTail.Add(objTemp);
            }
            else
            {
                // Thân gecko
                GameObject objTemp = Instantiate(objTailPrefab, this.transform);
                objTemp.transform.position = posTemp;

                part = objTemp.GetComponent<GeckoPart>();
                objTemp.GetComponent<GeckoPart>().Init(this, i);
                objTemp.SetActive(true);
                listObjTail.Add(objTemp);
            }

            part.indexInmap = indexNode;
        }

        // load thân 
        //geckoBody.Init(listPosStart.ToList());
        directionStart = mapController.mapHelper.GetDirection(indexPre, indexHead);

        switch (directionStart)
        {
            case DirectionMove.UP:
                objHeadGecko.transform.localEulerAngles = new Vector3(0, angleUp, 0);
                break;
            case DirectionMove.DOWN:
                objHeadGecko.transform.localEulerAngles = new Vector3(0, angleDown, 0);
                break;
            case DirectionMove.LEFT:
                objHeadGecko.transform.localEulerAngles = new Vector3(0, angleLeft, 0);
                break;
            case DirectionMove.RIGHT:
                objHeadGecko.transform.localEulerAngles = new Vector3(0, angleRight, 0);
                break;
        }
    }

    public void OnOffLineHead(bool isOn)
    {
        //Debug.Log("OnOffLineHead");
        lineHead.gameObject.SetActive(isOn);
    }

    private void SetInfoGecko()
    {
        if (listObjTail.Count < 1)
        {
            DebugCustom.ShowDebugColorRed("Error gecko very short", dataGeckoInMap);
            return;
        }

        for (int i = 0; i < listObjTail.Count; i++)
        {
            GeckoPart partTail = listObjTail[i].GetComponent<GeckoPart>();

            if (i == 0)
            {
                partTail.SetInfo(objHeadGecko.transform);
            }
            else
            {
                partTail.SetInfo(listObjTail[i - 1].transform);
            }
        }
    }

    #region Move

    public void EscapeMapForwardDirHead()
    {
        //DebugCustom.ShowDebug("Escapse");

        AddWaveMove();
        StartMoveForwardDirHead();
    }

    private void StartMoveForwardDirHead()
    {
        posTarget = GetPosNextOfHead();
        dirHead = (posTarget - objHeadGecko.transform.position).normalized;

        moveStateGecko = MoveStateGecko.MOVE;

        for (int i = 0; i < listObjTail.Count; i++)
        {
            listObjTail[i].GetComponent<GeckoPart>().StartMoveToTargetPos();
        }
    }


    private List<Vector3> listTempTest = new List<Vector3>();
    private void MoveToTargetCellDone(float padding)
    {
        moveStateGecko = MoveStateGecko.IDLE;
        objHeadGecko.transform.position = posTarget;

        for (int i = 0; i < listObjTail.Count; i++)
        {
            listObjTail[i].GetComponent<GeckoPart>().EndMoveToTargetPos();
        }

        switch (stateCurrent)
        {
            case StateGecko.ESCAPE_OUT_MAP:
                StartMoveForwardDirHead();
                objHeadGecko.transform.position += dirHead * padding;
                break;
            case StateGecko.ESCAPE_EXIT_HOLE:

                // Check theo index xem phải hole chưa
                if (targetNodeFinal.indexNodeInMap == mapController.mapHelper.WorldPosToIndex(posTarget))
                {
                    StartExitHoleFinal();

                    for (int i = 0; i < listObjTail.Count; i++)
                    {
                        listObjTail[i].GetComponent<GeckoPart>().StartMoveToFollowing();
                    }

                    AudioManager.Instance.PlayExitHole();
                    HapticController.Instance.VibrateGeckoExit();
                }
                else
                {
                    StartMoveForwardDirHead();
                }
                objHeadGecko.transform.position += dirHead * padding;
                break;
            case StateGecko.MOVE_TO_STOP_OBSTACLE:
                if (targetNodeFinal.indexNodeInMap == mapController.mapHelper.WorldPosToIndex(posTarget))
                {
                    // ở vị trí cell stop dừng di chuyển 
                    geckoPositions = listPosStart.ToList();
                    geckoPositions.RemoveDuplicates();

                    // --- Thư viện lỗi nên cho nó vẽ thêm 1 đường nữa rồi mới vẽ đường mới.
                    if (geckoPositions.Count > 2)
                    {
                        listTempTest.Clear();
                        for (int i = 0; i < geckoPositions.Count - 1; i++)
                        {
                            listTempTest.Add(geckoPositions[i]);
                        }
                        //geckoBody.UpdateBody(listTempTest);
                    }
                    //--- end --------------------------------------

                    //geckoBody.ReloadBody();

                }
                else
                {
                    StartMoveForwardDirHead();
                }

                break;
            case StateGecko.MOVE_FAIL:
                if (targetNodeFinal.indexNodeInMap == mapController.mapHelper.WorldPosToIndex(posTarget))
                {
                    // move ngược lại
                    //positionMoves.Insert(0, geckoBody.PositionStarts);
                    moveStateGecko = MoveStateGecko.MOVE_BACK;

                    //MapController.Instance.SetMoveFail();

                    HapticController.Instance.VibrateGeckoStun();
                    FxController.Instance.ShowHeartBreack(posTarget);
                    AudioManager.Instance.PlayMoveFail();

                    CameraController.Instance.ShareCamera();
                }
                else
                {
                    StartMoveForwardDirHead();
                    objHeadGecko.transform.position += dirHead * padding;
                }
                break;
        }
    }

    private void StartExitHoleFinal()
    {
        // thêm 1 pos vào line là điểm lỗ
        geckoPositions.Insert(1, new Vector3(posTarget.x, posTarget.y, posTarget.z));

        // đâm xuống
        posTarget.y -= 1000f;

        dirHead = (posTarget - objHeadGecko.transform.position).normalized;

        stateCurrent = StateGecko.ESCAPE_EXIT_HOLE_FINAL;
        moveStateGecko = MoveStateGecko.MOVE;

        for (int i = 0; i < listObjTail.Count; i++)
        {
            listObjTail[i].GetComponent<GeckoPart>().StartMoveToTargetPos();
        }
        StartCheckOutMapComplete();

        if (obstacleExit != null)
        {
            obstacleExit.OpenFx(dataGeckoInMap.listNode.Count);
        }
    }

    private void UpdateMove()
    {
        if (moveStateGecko == MoveStateGecko.MOVE_BACK)
        {
            if (positionMoves.Count > 0)
            {
                var listMove = positionMoves.Last();
                positionMoves.RemoveAt(positionMoves.Count - 1);

                objHeadGecko.transform.position = listMove[0];
                //geckoBody.UpdateBody(listMove);
            }
            else
            {
                //geckoPositions = geckoBody.PositionStarts.ToList();
                moveStateGecko = MoveStateGecko.IDLE;
            }
            return;
        }

        if (moveStateGecko != MoveStateGecko.MOVE)
            return;

        //Move Head
        var lastPosition = objHeadGecko.transform.position;
        var tempVector3 = objHeadGecko.transform.position;
        tempVector3 += GameConfig.speed * dirHead * Time.deltaTime;

        // xoay đầu
        if (stateCurrent == StateGecko.ESCAPE_EXIT_HOLE)
        {
            float pitchAngle = -Mathf.Atan2(dirHead.y, dirHead.z) * Mathf.Rad2Deg;
            Vector3 targetEuler = new Vector3(pitchAngle, objHeadGecko.transform.eulerAngles.y, objHeadGecko.transform.eulerAngles.z);

            objHeadGecko.transform.rotation = Quaternion.Slerp(
                objHeadGecko.transform.rotation,
                Quaternion.Euler(targetEuler),
                Time.deltaTime * 1f
            );
        }


        var newDir = (posTarget - tempVector3).normalized;
        if (newDir != dirHead)
        {
            MoveToTargetCellDone(Vector3.Distance(posTarget, tempVector3));
        }
        else
        {
            objHeadGecko.transform.position = tempVector3;
        }

        if (moveStateGecko == MoveStateGecko.MOVE)
        {
            MoveBody(objHeadGecko.transform.position, Vector3.Distance(objHeadGecko.transform.position, lastPosition));
            // Check die
            CheckOutCompleteMap();
        }
    }

    private int waveCount = 0;
    private int addHeadFirst = 0;
    private void AddWaveMove()
    {
        waveCount = 0;
        addHeadFirst = 0;
        if (geckoPositions.Count <= 2) return;
        if (!GameConfig.MOVE_ZIGZAG && !GameConfig.MOVE_OUT_ZIGZAG) return;
        if (GameConfig.OUT_ZIGZAG)
        {
            waveCount = 1;
            addHeadFirst = 0;
            return;
        }

        // add các điểm wave vào giữa các đốt, không add vào đốt đầu
        int count = geckoPositions.Count;
        while (count > 0)
        {
            if (count < 3) break;

            Vector3 cur = geckoPositions[count - 1];
            Vector3 next = geckoPositions[count - 2];

            // lấy hướng
            var dir = (next - cur).normalized;
            Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
            if (waveCount % 2 == 0) right = -1 * right;

            // lấy điểm ở giữa
            Vector3 midpoint = (cur + next) / 2f;
            midpoint += right * 0.125f;

            geckoPositions.Insert(count - 1, midpoint);

            waveCount++;
            count--;
        }
    }

    private void MoveBody(Vector3 posHead, float distance)
    {
        int checkBreak = 2;
        if (waveCount > 0)
        {
            if (addHeadFirst > 0)
            {
                geckoPositions[0] = posHead;
                addHeadFirst++;
            }
            else
            {
                geckoPositions.Insert(0, posHead);
                addHeadFirst = 1;
            }
            if (addHeadFirst <= 2) checkBreak = 3;

        }
        else
        {
            geckoPositions[0] = posHead;
        }

        int count = geckoPositions.Count;
        while (count > 0)
        {
            if (count < checkBreak) break;

            var dis = Vector3.Distance(geckoPositions[count - 2], geckoPositions[count - 1]);
            if (dis > distance)
            {
                var dir = (geckoPositions[count - 2] - geckoPositions[count - 1]).normalized;
                geckoPositions[count - 1] += dir * distance;
                break;
            }
            else
            {
                geckoPositions.RemoveAt(count - 1);
                distance = distance - dis;

                if (distance <= 0) break;
            }
            count--;
        }

        // đoạn này làm uốn lượn
        if (GameConfig.MOVE_ZIGZAG || (GameConfig.MOVE_OUT_ZIGZAG && (stateCurrent == StateGecko.ESCAPE_OUT_MAP || stateCurrent == StateGecko.ESCAPE_POWER_OUT_MAP)))
        {
            if (waveCount > 0 && addHeadFirst > 0 && geckoPositions.Count > 2)
            {
                // chỗ này check nếu đầu move đc 1 khoảng > 0.5f thì add thêm 1 dot
                var dis = Vector3.Distance(geckoPositions[0], geckoPositions[1]);
                if (dis >= GameConfig.sizeCell)
                {
                    var dir = (geckoPositions[0] - geckoPositions[1]).normalized;
                    var newPos = geckoPositions[1] + (dir * GameConfig.sizeCell);

                    geckoPositions.Insert(1, newPos);

                    // tạo wave
                    Vector3 cur = geckoPositions[2];
                    Vector3 next = geckoPositions[1];

                    // lấy hướng
                    dir = (next - cur).normalized;
                    Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
                    if (waveCount % 2 == 0) right = -1 * right;

                    // lấy điểm ở giữa
                    Vector3 midpoint = (cur + next) / 2f;
                    midpoint += right * 0.05f;

                    geckoPositions.Insert(2, midpoint);

                    waveCount++;
                }
            }
        }


        // check last index 
        int index = MapController.Instance.mapHelper.WorldPosToIndex(geckoPositions[geckoPositions.Count - 1]);
        if (lastTailIndex != index)
        {
            MapController.Instance.DotPlayAnim(index);
            lastTailIndex = index;
        }

        geckoPositions.RemoveDuplicates();
        //geckoBody.UpdateBody(geckoPositions);
        positionMoves.Add(geckoPositions.ToList());
    }

    private Vector3 GetPosNextOfHead()
    {
        Vector3 posNextTemp = objHeadGecko.transform.position;
        switch (directionStart)
        {
            case DirectionMove.UP:
                posNextTemp.z += 1;
                break;
            case DirectionMove.DOWN:
                posNextTemp.z -= 1;
                break;
            case DirectionMove.LEFT:
                posNextTemp.x -= 1;
                break;
            case DirectionMove.RIGHT:
                posNextTemp.x += 1;
                break;
        }

        return posNextTemp;
    }


    public bool IsCanNewState()
    {
        if (moveStateGecko == MoveStateGecko.IDLE || moveStateGecko == MoveStateGecko.NONE)
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Action

    public void StartState(GeckoAction stateNext)
    {
        stateCurrent = StateGecko.IDLE;
        targetNodeFinal = stateNext.nodeTarget;
        this.targetNodeFinalTemp = targetNodeFinal;

        positionMoves = new List<List<Vector3>>();
        lastTailIndex = -1;
        listIndexMoveTemp.Clear();
        obstacleExit = null;

        ActiveDot();

        if (stateNext.stateNext != StateGecko.MOVE_FAIL)
        {
            OffFxStun();
        }
        MapController.Instance.HideDirectionLine();

        switch (stateNext.stateNext)
        {
            case StateGecko.ESCAPE_OUT_MAP:
                {
                    //DebugCustom.ShowDebugColor("Out Map");

                    stateCurrent = StateGecko.ESCAPE_OUT_MAP;
                    mapController.RemoveNodeOfGecko(this);

                    targetNodeFinal = null;
                    EscapeMapForwardDirHead();

                    ResetRelationShip();

                    GetListNodeMove();
                    SimpleEventManager.Instance.PostEvent(EventIDSimple.moveSuccess, this);
                    SimpleEventManager.Instance.PostEvent(EventIDSimple.geckoOutMapSucess, this);


                    StartCheckOutMapComplete();

                    SetIdle();

                    AudioManager.Instance.PlayGeckoMove();
                    HapticController.Instance.VibarateGeckoWave();
                    //HapticController.Instance.VibrateOne();

                    break;
                }

            case StateGecko.ESCAPE_EXIT_HOLE:
                {
                    //DebugCustom.ShowDebugColor("Out Map exit hole", stateNext.nodeTarget.indexNodeInMap);
                    stateCurrent = StateGecko.ESCAPE_EXIT_HOLE;
                    mapController.RemoveNodeOfGecko(this);

                    targetNodeFinal = stateNext.nodeTarget;
                    EscapeMapForwardDirHead();

                    ResetRelationShip();

                    GetListNodeMove();
                    SimpleEventManager.Instance.PostEvent(EventIDSimple.moveSuccess, this);
                    SimpleEventManager.Instance.PostEvent(EventIDSimple.geckoOutMapSucess, this);


                    SetIdle();

                    AudioManager.Instance.PlayGeckoMove();
                    HapticController.Instance.VibarateGeckoWave();
                    //HapticController.Instance.VibrateOne();
                    obstacleExit = MapController.Instance.GetExitObstacle(targetNodeFinal.indexNodeInMap);

                    break;
                }

            case StateGecko.MOVE_TO_STOP_OBSTACLE:
                {
                    stateCurrent = StateGecko.MOVE_TO_STOP_OBSTACLE;
                    targetNodeFinal = stateNext.nodeTarget;

                    // Kiểm tra xem có mỗi liên kết cũ nào với stop khác không thì xóa mối liên kết đấy đi.
                    ResetRelationShip();

                    // Đánh dấu node stop đã có gecko
                    var stopObstacleTemp = mapController.GetStopObstacle(stateNext.nodeTarget.indexNodeInMap);
                    stopObstacleTemp.SetContentGecko(true);
                    stopObstacleContentGecko = stopObstacleTemp;

                    //Thực hiện update laị map tính toán vị trị mới của gecko tới và găn vào map.
                    List<int> listIndexNew = mapController.SimulateGeckoMoveToTarget(dataGeckoInMap.listNode, indexHead, targetNodeFinal.indexNodeInMap, directionStart);
                    mapController.UpdateMapAfterSimulateMove(this, listIndexNew);
                    listPosStart.Clear();
                    for (int i = 0; i < listIndexNew.Count; i++)
                    {
                        Vector3 posTemp = mapController.mapHelper.GetWorldPositionFromIndex(listIndexNew[i]);
                        listPosStart.Add(posTemp);
                    }
                    //geckoBody.UpdateListStartPos(listPosStart.ToList());

                    EscapeMapForwardDirHead();

                    GetListNodeMove();
                    SimpleEventManager.Instance.PostEvent(EventIDSimple.moveSuccess, this);

                    SetIdle();

                    AudioManager.Instance.PlayGeckoMove();
                    HapticController.Instance.VibarateGeckoWave(0.3f);
                    //HapticController.Instance.VibrateOne();
                    break;
                }
            case StateGecko.MOVE_FAIL:
                {
                    stateCurrent = StateGecko.MOVE_FAIL;
                    targetNodeFinal = stateNext.nodeTarget;
                    EscapeMapForwardDirHead();

                    HapticController.Instance.VibarateGeckoWave(0.3f);
                    //HapticController.Instance.VibrateOne();
                    SetStun();

                    MapController.Instance.SetMoveFail();

                    AudioManager.Instance.PlayGeckoMove();
                    break;
                }
        }
    }

    private void ResetRelationShip()
    {
        if (stopObstacleContentGecko != null)
        {
            //DebugCustom.ShowDebugColorRed("Remove relationShip stop", stopObstacleContentGecko.gameObject.name);

            stopObstacleContentGecko.SetContentGecko(false);
            stopObstacleContentGecko = null;
        }
    }

    #endregion

    private void StartCheckOutMapComplete()
    {
        isCheckOutMapComplete = true;
        timeCount = 0f;
    }

    private void CheckOutCompleteMap()
    {
        if (stateCurrent != StateGecko.ESCAPE_EXIT_HOLE_FINAL && stateCurrent != StateGecko.ESCAPE_OUT_MAP)
        {
            return;
        }

        if (!isCheckOutMapComplete)
            return;


        timeCount += Time.deltaTime;
        if (timeCount < timeCheckOutMap)
        {
            return;
        }

        timeCount = 0;

        if (stateCurrent == StateGecko.ESCAPE_OUT_MAP)
        {
            int index = mapController.mapHelper.WorldPosToIndex(geckoTailEnd.transform.position);
            if (index == -1 && !isDie)
            {
                //DebugCustom.ShowDebugColor("Gecko out complete map");
                SimpleEventManager.Instance.PostEvent(EventIDSimple.geckoMoveOutCompelteMap, this);
                isDie = true;

                this.StartDelayAction(1.5f, () =>
                {
                    Destroy(this.gameObject);
                });

                return;
            }
        }
        else if (stateCurrent == StateGecko.ESCAPE_EXIT_HOLE_FINAL)
        {
            if (geckoTailEnd.transform.position.y < -2)
            {
                //DebugCustom.ShowDebugColor("Gecko out complete map");
                SimpleEventManager.Instance.PostEvent(EventIDSimple.geckoMoveOutCompelteMap, this);
                isDie = true;
                Destroy(this.gameObject);
                return;
            }
        }
    }

    public List<int> GetListNodeMove()
    {
        listIndexMoveTemp.Clear();

        if (targetNodeFinalTemp == null)
        {
            return listIndexMoveTemp;
        }

        //DebugCustom.ShowDebugColorRed("Node Target:", targetNodeFinalTemp.indexNodeInMap);

        int cols = mapController.mapHelper.column;
        int rows = mapController.mapHelper.row;

        int head = this.dataGeckoInMap.indexHead;
        int neck = this.dataGeckoInMap.listNode[1];

        int hx = head % cols;
        int hy = head / cols;

        int nx = neck % cols;
        int ny = neck / cols;

        int dx = -nx + hx;
        int dy = -ny + hy;

        // bước đầu tiên luôn là head
        listIndexMoveTemp.Add(head);

        //DebugCustom.ShowDebug("x,y:" + dx,dy);

        int curX = hx;
        int curY = hy;

        while (true)
        {
            curX += dx;
            curY += dy;

            int idx = curY * cols + curX;


            if (idx == targetNodeFinalTemp.indexNodeInMap)
            {
                //DebugCustom.ShowDebugColorRed("Thay node");
                break;
            }

            if (curX < 0 || curX >= cols || curY < 0 || curY >= rows)
            {
                //DebugCustom.ShowDebugColorRed("Không thấy node tới (ra ngoài map)");
                break;
            }

            listIndexMoveTemp.Add(idx);
        }

        //DebugCustom.ShowDebugColor("list Move New:" + targetNodeFinalTemp.indexNodeInMap, JsonConvert.SerializeObject(listIndexMoveTemp));

        return listIndexMoveTemp;
    }

    public List<int> GetListNodeMoveData()
    {
        return listIndexMoveTemp;
    }

    private void SetIdle()
    {
 
    }

    private void SetStun()
    {

        if (fxStun == null)
        {
            var objTemp = PoolingController.Instance.GetGameObjectFromPool(TypeObject.FX_STUN_GECKO, FxController.Instance.objStunPrefab);

            objTemp.transform.parent = this.objHeadGecko.transform;
            objTemp.transform.localPosition = Vector3.zero;
            objTemp.gameObject.SetActive(true);

            fxStun = objTemp.GetComponent<BaseFx>();
        }
        lineHead.color = colorLineError;

        //AudioManager.Instance.PlayMoveFail();
    }

    private void OffFxStun()
    {
        if (fxStun != null)
        {
            fxStun.transform.SetParent(null);
            fxStun.gameObject.SetActive(false);
        }

        fxStun = null;
        lineHead.color = colorLineNormal;
    }

    public Transform GetTransHead()
    {
        return objHeadGecko.transform;
    }

    private void ActiveDot()
    {
        if (!isActiveDot)
            isActiveDot = true;

        mapController.ActiveDot(this);
    }
}
