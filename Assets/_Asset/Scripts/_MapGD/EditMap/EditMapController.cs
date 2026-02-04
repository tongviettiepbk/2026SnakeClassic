using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EditMapController : MonoBehaviour
{
    public MapController mapController;

    public GameObject objCanvasEditMap;

    [Space(20)]
    public string linkPath = "/_Asset/MapData/_MapGDDoing/";
    public string fileName = "mapExit.json";

    [Space(20)]
    public stateCreateMapGD currentStateCreateMapGD = stateCreateMapGD.idle;

    [Space(20)]
    public Button btCreateGecko;
    public Button btCreateGekoDone;
    public Button btRemove;

    public Button btCreateStop;
    public Button btCreateBrick;
    public Button btCreateHoleEnd;
    public Button btCreateBarrier;

    [Space(20)]
    public Button btExportJson;

    [Space(20)]
    public TMP_InputField inputIdNameGecko;
    public TMP_InputField inputColor;
    public TMP_Text txtState;
    public TMP_Text txtNotice;

    [Space(20)]
    public CellGD2 cellPrefabsGecko;

    private int idGeckoCurrent = 0;
    private int idValueCurrent = 0;

    private List<int> listCellGeckoCurrent = new List<int>();

    private GeckoDataInMap geckoDataInMapTemp = new GeckoDataInMap();
    private List<CellGD2> listNodeGeckoObjView = new List<CellGD2>();

    private bool isOff = false;

    #region Unity

    private void Start()
    {

#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
        OffMode();
        return;
#endif
        /////// action
        StartTool();
    }

    private void Update()
    {
#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
        return;
#endif

        if (isOff)
        {
            return;
        }

        /////// action
        UpdateTool();
    }

    #endregion

    private void StartTool()
    {
        btCreateBrick.onClick.AddListener(ClickBtCreateBrick);
        btCreateGekoDone.onClick.AddListener(ClickBtDone);

        btRemove.onClick.AddListener(ClickBtRemoveCell);

        btCreateGecko.onClick.AddListener(ClickBtGecko);
        btCreateBarrier.onClick.AddListener(ClickBtCreateBarrier);
        btCreateHoleEnd.onClick.AddListener(ClickBtCreateHoleEnd);
        btCreateStop.onClick.AddListener(ClickBtCreateStop);

        btExportJson.onClick.AddListener(() =>
        {
            ClickBtDone();
            ClickExport();
        });

        btSave.onClick.AddListener(ClickBtSaveSetting);
        btNewMap.onClick.AddListener(ClickBtNewMap);
        btSetting.onClick.AddListener(ClickBtSetting);
        tooglePlayProduct.onValueChanged.AddListener(ClickTooglePlayType);
        btOpenMap.onClick.AddListener(ClickBtOpenMap);

        LoadTypeDev();
    }

    private void UpdateTool()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenEditMap();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadScene();
        }

        if (Input.GetMouseButtonDown(0))
        {
            DetectGecko(Input.mousePosition);
        }
    }

    private void OpenEditMap()
    {
        mapController.SetModeEditMap(true);
        objCanvasEditMap.SetActive(true);
        mapController.lgamePlay.StopAllCoroutines();

        foreach (var dot in mapController.dictDots)
        {
            dot.Value.gameObject.SetActive(true);
        }

    }



    private void ClickBtGecko()
    {
        if (currentStateCreateMapGD != stateCreateMapGD.idle)
        {
            ShowNotice("Hay ket thuc state hien tai click done");
            return;
        }

        idGeckoCurrent = -1;
        idValueCurrent = -1;

        try
        {
            //idGeckoCurrent = int.Parse(inputIdNameGecko.text);
            idValueCurrent = int.Parse(inputColor.text);
        }
        catch
        {

        }

        //if (idGeckoCurrent < 0)
        //{
        //    txtState.text = "Error nhap lai id";
        //    return;
        //}

        ShowNotice("Bat dau tao gecko color: " + idValueCurrent);

        listCellGeckoCurrent.Clear();

        geckoDataInMapTemp = null;
        geckoDataInMapTemp = new GeckoDataInMap();
        geckoDataInMapTemp.listNode = new List<int>();
        geckoDataInMapTemp.colorGecko = idValueCurrent;

        currentStateCreateMapGD = stateCreateMapGD.createGecko;
        txtState.text = "Start Create Gecko hay nhap id va idcolor";
    }

    private void ClickBtCreateBrick()
    {
        if (currentStateCreateMapGD != stateCreateMapGD.idle)
        {
            ShowNotice("Hay ket thuc state hien tai click done");
            return;
        }

        txtState.text = "Create Stop Hay nhap Hp";

        currentStateCreateMapGD = stateCreateMapGD.createBrick;
    }

    private void ClickBtCreateStop()
    {
        if (currentStateCreateMapGD != stateCreateMapGD.idle)
        {
            ShowNotice("Hay ket thuc state hien tai click done");
            return;
        }

        txtState.text = "Create Stop Ko can nhap them";
        currentStateCreateMapGD = stateCreateMapGD.createStop;
    }

    private void ClickBtCreateHoleEnd()
    {
        if (currentStateCreateMapGD != stateCreateMapGD.idle)
        {
            ShowNotice("Hay ket thuc state hien tai click done");
            return;
        }

        txtState.text = "Create Hole End Hay nhap Color ID";
        currentStateCreateMapGD = stateCreateMapGD.createHoleOut;
    }

    private void ClickBtCreateBarrier()
    {
        if (currentStateCreateMapGD != stateCreateMapGD.idle)
        {
            ShowNotice("Hay ket thuc state hien tai click done");
            return;
        }
        txtState.text = "Create Barrier Hay nhap trang thai 0 or 1";
        currentStateCreateMapGD = stateCreateMapGD.createBarrier;
    }

    private void ClickBtRemoveCell()
    {
        switch (currentStateCreateMapGD)
        {
            case stateCreateMapGD.createGecko:

                break;
        }

        currentStateCreateMapGD = stateCreateMapGD.remove;
        txtState.text = "Remove Cell";
    }

    private void DetectGecko(Vector3 posClick)
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // 1. convert screen → world
        Vector3 worldPos = mapController.GetWorldPositionFromClick(posClick);

        // 2. convert world → index
        int index = mapController.mapHelper.WorldPosToIndex(worldPos);

        switch (currentStateCreateMapGD)
        {
            case stateCreateMapGD.createGecko:
                CreateGecko(index);
                break;
            case stateCreateMapGD.createBrick:
                CreateBreak(index);
                break;

            case stateCreateMapGD.createBarrier:
                CreateBarrier(index);
                break;

            case stateCreateMapGD.createStop:
                CreateStop(index);
                break;
            case stateCreateMapGD.createHoleOut:
                CreateHole(index);
                break;
            case stateCreateMapGD.remove:
                RemoveCell(index);
                break;
        }


    }


    #region Create Element

    private void CreateGecko(int index)
    {
        DebugCustom.ShowDebug("Index Inmap", index);

        if (index < 0)
        {
            ShowNotice("Vi tri khong hop le");
            return;
        }

        if (mapController.listNodeMap[index].typeNodeValueInMap != TypeNodeValueInMap.EMPTY)
        {
            ShowNotice("node not empty");
            return;
        }

        string tempColor = inputColor.text;
        if (string.IsNullOrEmpty(tempColor))
        {
            ShowNotice("Hay nhap id cho gecko");
            return;
        }

        int idColor = int.Parse(tempColor);

        if (idColor < 0)
        {
            ShowNotice("color phai lon hon 0");
            return;
        }

        if (listCellGeckoCurrent.Contains(index))
        {
            ShowNotice("Da chon o nay roi");
            return;
        }

        listCellGeckoCurrent.Add(index);

        var obj = GetCellGecko();
        obj.gameObject.SetActive(true);
        obj.Reset();
        obj.ChangeMaterialColor(idColor);

        Vector3 tempVector3 = mapController.mapHelper.GetWorldPositionFromIndex(index);
        obj.transform.position = tempVector3;

        if (listCellGeckoCurrent.Count == 1)
        {
            geckoDataInMapTemp.indexHead = index;
            obj.ChoseHead();
            geckoDataInMapTemp.indexHead = index;
        }
        else if (listCellGeckoCurrent.Count == 2)
        {
            obj.ChoseSecond();
        }
    }

    private void CreateBreak(int index)
    {
        DebugCustom.ShowDebug("Index Inmap", index);

        if (index < 0)
        {
            ShowNotice("Vi tri khong hop le");
            return;
        }

        if (mapController.listNodeMap[index].typeNodeValueInMap != TypeNodeValueInMap.EMPTY)
        {
            ShowNotice("node not empty");
            return;
        }

        string tempHp = inputColor.text;
        if (string.IsNullOrEmpty(tempHp))
        {
            ShowNotice("Hay nhap Hp cho gach");
            return;
        }

        int hp = int.Parse(tempHp);

        if (hp <= 0)
        {
            ShowNotice("Hp phai lon hon 0");
            return;
        }

        mapController.listNodeMap[index].SetValueNote(TypeNodeValueInMap.OBSTACLE_BRICK);

        BrickDataInMap brickData = new BrickDataInMap();
        brickData.indexInMap = index;
        brickData.hp = hp;

        Vector3 tempVector3 = mapController.mapHelper.GetWorldPositionFromIndex(index);
        GameObject objBrick = Instantiate(mapController.brickPrefab, tempVector3, Quaternion.identity, transform);
        BrickObstacle brickObstacle = objBrick.GetComponent<BrickObstacle>();

        if (brickObstacle != null)
        {
            brickObstacle.Init(brickData, mapController);
            mapController.listBrickObstacle.Add(brickObstacle);
        }

        mapController.mapInfo.listBrickInMap.Add(brickData);
        currentStateCreateMapGD = stateCreateMapGD.idle;
    }

    private void CreateHole(int index)
    {
        DebugCustom.ShowDebug("Index Inmap", index);

        if (index < 0)
        {
            ShowNotice("Vi tri khong hop le");
            return;
        }

        if (mapController.listNodeMap[index].typeNodeValueInMap != TypeNodeValueInMap.EMPTY)
        {
            ShowNotice("node not empty");
            return;
        }

        string tempColor = inputColor.text;
        if (string.IsNullOrEmpty(tempColor))
        {
            ShowNotice("Hay nhap color cho hole");
            return;
        }

        int color = int.Parse(tempColor);

        if (color < 0)
        {
            ShowNotice("color phai lon hon 0");
            return;
        }

        if (color > 6)
        {
            ShowNotice("color phai <7");
            return;
        }

        mapController.listNodeMap[index].SetValueNote(TypeNodeValueInMap.OBSTALCE_EXIT_HOLE);

        ExitHoleDataInMap holeData = new ExitHoleDataInMap();
        holeData.indexInMap = index;
        holeData.color = color;

        Vector3 tempVector3 = mapController.mapHelper.GetWorldPositionFromIndex(index);
        GameObject objHole = Instantiate(mapController.exitPrefab[holeData.color], tempVector3, Quaternion.identity, transform);
        ExitsObstacle holeObstacle = objHole.GetComponent<ExitsObstacle>();

        if (holeObstacle != null)
        {
            holeObstacle.Init(holeData, mapController);
            mapController.listExitsObstacle.Add(holeObstacle);
        }

        mapController.mapInfo.listExitsInMap.Add(holeData);
        currentStateCreateMapGD = stateCreateMapGD.idle;
    }

    private void CreateBarrier(int index)
    {
        DebugCustom.ShowDebug("Index Inmap", index);

        if (index < 0)
        {
            ShowNotice("Vi tri khong hop le");
            return;
        }

        if (mapController.listNodeMap[index].typeNodeValueInMap != TypeNodeValueInMap.EMPTY)
        {
            ShowNotice("node not empty");
            return;
        }

        string tempOpen = inputColor.text;
        if (string.IsNullOrEmpty(tempOpen))
        {
            ShowNotice("Hay nhap trang thai cho barrier");
            return;
        }

        int open = int.Parse(tempOpen);

        if (open < 0)
        {
            ShowNotice("barrier phai lon hon =0");
            return;
        }

        mapController.listNodeMap[index].SetValueNote(TypeNodeValueInMap.OBSTALCE_BARRIER);

        BarrierDataInMap barrierData = new BarrierDataInMap();
        barrierData.indexInMap = index;
        barrierData.isOpen = open;

        Vector3 tempVector3 = mapController.mapHelper.GetWorldPositionFromIndex(index);
        GameObject objBarrier = Instantiate(mapController.barrierPrefab, tempVector3, Quaternion.identity, transform);
        BarrierObstacle barrierObstacle = objBarrier.GetComponent<BarrierObstacle>();

        if (barrierObstacle != null)
        {
            barrierObstacle.Init(barrierData, mapController);
            mapController.listBarrierObstacle.Add(barrierObstacle);
        }

        mapController.mapInfo.listBarrierInMap.Add(barrierData);
        currentStateCreateMapGD = stateCreateMapGD.idle;

    }

    private void CreateStop(int index)
    {
        DebugCustom.ShowDebug("Index Inmap", index);

        if (index < 0)
        {
            ShowNotice("Vi tri khong hop le");
            return;
        }

        if (mapController.listNodeMap[index].typeNodeValueInMap != TypeNodeValueInMap.EMPTY)
        {
            ShowNotice("node not empty");
            return;
        }

        mapController.listNodeMap[index].SetValueNote(TypeNodeValueInMap.OBSTALCE_STOP);

        StopDataInMap stopData = new StopDataInMap();
        stopData.indexInMap = index;

        Vector3 tempVector3 = mapController.mapHelper.GetWorldPositionFromIndex(index);
        GameObject objStop = Instantiate(mapController.StopPrefab, tempVector3, Quaternion.identity, transform);
        StopObstacle stopObstacle = objStop.GetComponent<StopObstacle>();

        if (stopObstacle != null)
        {
            stopObstacle.Init(stopData, mapController);
            mapController.listStopObstacle.Add(stopObstacle);
        }

        mapController.mapInfo.listStopInMap.Add(stopData);
        currentStateCreateMapGD = stateCreateMapGD.idle;
    }



    private void RemoveCell(int index)
    {
        if (mapController.listNodeMap[index].typeNodeValueInMap == TypeNodeValueInMap.EMPTY)
        {
            ShowNotice("node empty");
            return;
        }

        switch (mapController.listNodeMap[index].typeNodeValueInMap)
        {
            case TypeNodeValueInMap.NODE_GECKO:

                var geckoTemp = mapController.GetGeckoWithIndex(index);

                if (geckoTemp != null && geckoTemp.IsCanNewState())
                {
                    mapController.RemoveNodeOfGecko(geckoTemp);

                    var tempListDataGecko = mapController.mapInfo.listGeckoInMap;
                    for (int i = 0; i < tempListDataGecko.Count; i++)
                    {
                        if (tempListDataGecko[i].indexHead == geckoTemp.indexHead)
                        {
                            tempListDataGecko.RemoveAt(i);
                            break;
                        }
                    }

                    Destroy(geckoTemp.gameObject);
                }

                break;

            case TypeNodeValueInMap.OBSTALCE_STOP:
                //if (listCellStop.Contains(cell))
                //    listCellStop.Remove(cell);
                mapController.listNodeMap[index].SetValueNote(TypeNodeValueInMap.EMPTY);

                for (int i = 0; i < mapController.listStopObstacle.Count; i++)
                {
                    if (mapController.listStopObstacle[i].indexInMap == index)
                    {
                        var temp = mapController.listStopObstacle[i];
                        mapController.listStopObstacle.RemoveAt(i);
                        temp.RemoveObstacle();
                        break;
                    }
                }

                for (int i = 0; i < mapController.mapInfo.listStopInMap.Count; i++)
                {
                    if (mapController.mapInfo.listStopInMap[i].indexInMap == index)
                    {
                        mapController.mapInfo.listStopInMap.RemoveAt(i);
                        break;
                    }
                }


                break;
            case TypeNodeValueInMap.OBSTACLE_BRICK:
                mapController.listNodeMap[index].SetValueNote(TypeNodeValueInMap.EMPTY);

                for (int i = 0; i < mapController.listBrickObstacle.Count; i++)
                {
                    if (mapController.listBrickObstacle[i].indexInMap == index)
                    {
                        var temp = mapController.listBrickObstacle[i];
                        mapController.listBrickObstacle.RemoveAt(i);
                        temp.RemoveObstacle();
                        break;
                    }
                }

                for (int i = 0; i < mapController.mapInfo.listBrickInMap.Count; i++)
                {
                    if (mapController.mapInfo.listBrickInMap[i].indexInMap == index)
                    {
                        mapController.mapInfo.listBrickInMap.RemoveAt(i);
                        break;
                    }
                }

                break;
            case TypeNodeValueInMap.OBSTALCE_EXIT_HOLE:

                mapController.listNodeMap[index].SetValueNote(TypeNodeValueInMap.EMPTY);

                for (int i = 0; i < mapController.listExitsObstacle.Count; i++)
                {
                    if (mapController.listExitsObstacle[i].indexInMap == index)
                    {
                        var temp = mapController.listExitsObstacle[i];
                        mapController.listExitsObstacle.RemoveAt(i);
                        temp.RemoveObstacle();
                        break;
                    }
                }

                for (int i = 0; i < mapController.mapInfo.listExitsInMap.Count; i++)
                {
                    if (mapController.mapInfo.listExitsInMap[i].indexInMap == index)
                    {
                        mapController.mapInfo.listExitsInMap.RemoveAt(i);
                        break;
                    }
                }

                break;
            case TypeNodeValueInMap.OBSTALCE_BARRIER:

                mapController.listNodeMap[index].SetValueNote(TypeNodeValueInMap.EMPTY);

                for (int i = 0; i < mapController.listBarrierObstacle.Count; i++)
                {
                    if (mapController.listBarrierObstacle[i].indexInMap == index)
                    {
                        var temp = mapController.listBarrierObstacle[i];
                        mapController.listBarrierObstacle.RemoveAt(i);
                        temp.RemoveObstacle();
                        break;
                    }
                }

                for (int i = 0; i < mapController.mapInfo.listBarrierInMap.Count; i++)
                {
                    if (mapController.mapInfo.listBarrierInMap[i].indexInMap == index)
                    {
                        mapController.mapInfo.listBarrierInMap.RemoveAt(i);
                        break;
                    }
                }

                break;
        }

    }

    private void CreatGeckoDone()
    {
        currentStateCreateMapGD = stateCreateMapGD.idle;
        txtState.text = "None";

        for (int i = 0; i < listNodeGeckoObjView.Count; i++)
        {
            listNodeGeckoObjView[i].gameObject.SetActive(false);
        }

        if (geckoDataInMapTemp.listNode.Count > 0)
        {
            geckoDataInMapTemp.listNode.Clear();
        }

        if (listCellGeckoCurrent.Count < 2)
        {
            DebugCustom.ShowDebugColorRed("Chua du do dai geko");
            return;
        }

        for (int i = 0; i < listCellGeckoCurrent.Count; i++)
        {
            geckoDataInMapTemp.listNode.Add(listCellGeckoCurrent[i]);
        }

        mapController.mapInfo.listGeckoInMap.Add(geckoDataInMapTemp);
        mapController.SpawnGecko(geckoDataInMapTemp);

        for (int i = 0; i < listCellGeckoCurrent.Count; i++)
        {
            mapController.listNodeMap[listCellGeckoCurrent[i]].SetValueNote(TypeNodeValueInMap.NODE_GECKO);
        }

        geckoDataInMapTemp = null;
    }


    private void ClickBtDone()
    {
        switch (currentStateCreateMapGD)
        {
            case stateCreateMapGD.createGecko:
                CreatGeckoDone();
                break;

            case stateCreateMapGD.createStop:
                ShowNotice("Da tao xong Stop voi so phan tu:");

                break;

            case stateCreateMapGD.createBrick:
                ShowNotice("Da tao xong Brick voi so phan tu:");
                break;

            case stateCreateMapGD.createBarrier:
                ShowNotice("Da tao xong Barier voi so phan tu:");
                break;

            case stateCreateMapGD.createHoleOut:
                ShowNotice("Da tao xong HOle Out  voi so phan tu:");
                break;
        }

        txtState.text = "None";
        currentStateCreateMapGD = stateCreateMapGD.idle;
    }

    #endregion




    private void ClickExport()
    {
        if (GameConfig.typeDev != TypeDev.mkt)
        {
#if UNITY_EDITOR
            string linkPathFull = Application.dataPath + linkPath;
            var dataScore = MapDifficultyCalculator.Compute(mapController.mapInfo);
            mapController.mapInfo.score = dataScore.finalScore;

            string dataJsonTemp = JsonConvert.SerializeObject(mapController.mapInfo);

            ExportJson.Export(linkPathFull, fileName, dataJsonTemp);
#endif
        }
        else
        {
            ExportMkt(mapController.mapInfo);
        }

    }

    // -------------------------------------
    private void OffMode()
    {
        isOff = true;
        this.gameObject.SetActive(false);
    }

    private void ShowNotice(string note)
    {
        DebugCustom.ShowDebugColorRed("ShowNotice: " + note);

        txtNotice.text = note;
        txtNotice.gameObject.SetActive(true);
        this.StartDelayAction(2f, () =>
        {
            txtNotice.gameObject.SetActive(false);
        });
    }

    private CellGD2 GetCellGecko()
    {
        CellGD2 cellTemp = null;
        for (int i = 0; i < listNodeGeckoObjView.Count; i++)
        {
            if (listNodeGeckoObjView[i].gameObject.activeSelf)
                cellTemp = listNodeGeckoObjView[i];
        }

        cellTemp = Instantiate(cellPrefabsGecko).GetComponent<CellGD2>();
        listNodeGeckoObjView.Add(cellTemp);

        return cellTemp;
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(GameConfig.SCENE_GAME);
    }

    // Tool MKT Order
    #region Tool Info MKT

    [Space(20)]
    [Header("MKT")]

    [Space(20)]
    public GameObject objInfoTop;
    public TMP_Text txtLv;
    public TMP_Text txtRow;
    public TMP_Text txtCol;

    [Space(20)]
    public TMP_InputField inputIndexBG;

    [Space(20)]
    public Button btSetting;
    public GameObject objSetting;

    public TMP_InputField inputLv;
    public TMP_InputField inputLvProduct;
    public TMP_InputField inputRow;
    public TMP_InputField inputCol;
    public Toggle tooglePlayProduct;
    public Button btSave;
    public Button btNewMap;
    public Button btOpenMap;

    private int lvl = 1;
    private int column = 10;
    private int row = 10;
    private int lvInput;
    private int colInput;
    private int rowInput;
    private TypeDev typeDev;

    private void LoadTypeDev()
    {
        bool isProduct = true;
        if (GameConfig.typeDev == TypeDev.mkt)
            isProduct = false;

        tooglePlayProduct.isOn = isProduct;
    }

    private void ClickBtSetting()
    {
        objSetting.gameObject.SetActive(true);
        LoadTypeDev();

        int temp = 0;

        temp = GameConfig.lvMkt;
        if (temp > 0)
        {
            inputLv.text = temp.ToString();
        }

        temp = GameConfig.indexBgMkt;
        if (temp > 0)
        {
            inputIndexBG.text = temp.ToString();
        }

        temp = GameConfig.lvMktProductPA;
        if (temp > 0)
        {
            inputLvProduct.text = temp.ToString();
        }
    }

    private void ClickBtSaveSetting()
    {
        GameConfig.lvMkt = GetLv();
        GameConfig.lvMktProductPA = GetLvProduct();
        GameConfig.typeDev = typeDev;
        GameConfig.indexBgMkt = GetIndexBg();
        LoadTypeDev();

        CloseSetting();
    }

    private void ClickBtNewMap()
    {
        MapInfo mapInfo = new MapInfo();
        mapInfo.column = column;
        mapInfo.row = row;
        mapInfo.listGeckoInMap = new List<GeckoDataInMap>();
        mapInfo.listBarrierInMap = new List<BarrierDataInMap>();
        mapInfo.listBrickInMap = new List<BrickDataInMap>();
        mapInfo.listExitsInMap = new List<ExitHoleDataInMap>();
        mapInfo.listStopInMap = new List<StopDataInMap>();

        GameConfig.typeDev = TypeDev.mkt;
        LoadTypeDev();

        int tempRow = GetRow();
        int tempCol = GetColumn();
        int temp = GetLv();

        if (temp <= 0 || tempRow <= 0 || tempCol <= 0)
        {

            ShowNotice("Nhap lv Muon Mo va col,row");
            return;
        }
        else
        {
            GameConfig.lvMkt = temp;
            mapInfo.column = tempCol;
            mapInfo.row = tempRow;

            ExportMkt(mapInfo);

            ReloadScene();
        }

    }

    private void ClickBtOpenMap()
    {
        int temp = GetLv();

        if (temp > 0)
        {
            GameConfig.lvMkt = temp;
        }

        temp = GetLvProduct();

        if (temp > 0)
        {
            GameConfig.lvMktProductPA = temp;
        }

        GameConfig.typeDev = TypeDev.mkt;
        LoadTypeDev();

        ReloadScene();
    }

    private void ClickTooglePlayType(bool isOn)
    {
        if (isOn)
        {
            typeDev = TypeDev.Product;
        }
        else
        {
            typeDev = TypeDev.mkt;
        }

        GameConfig.typeDev = typeDev;
    }

    private void CloseSetting()
    {
        objSetting.gameObject.SetActive(false);
    }

    private int GetRow()
    {
        string temp = inputRow.text;
        if (string.IsNullOrEmpty(temp))
        {
            ShowNotice("Nhap Row");
            return -1;
        }

        int data = int.Parse(temp);

        return data;
    }

    private int GetColumn()
    {
        string temp = inputCol.text;
        if (string.IsNullOrEmpty(temp))
        {
            ShowNotice("Nhap Col");
            return -1;
        }

        int data = int.Parse(temp);

        return data;
    }

    private int GetLv()
    {
        string temp = inputLv.text;
        if (string.IsNullOrEmpty(temp))
        {
            //ShowNotice("Nhap Level");
            return -1;
        }

        int data = int.Parse(temp);

        return data;
    }

    private int GetLvProduct()
    {
        string temp = inputLvProduct.text;
        if (string.IsNullOrEmpty(temp))
        {
            //ShowNotice("Nhap Level");
            return -1;
        }

        int data = int.Parse(temp);

        return data;
    }

    private int GetIndexBg()
    {
        string temp = inputIndexBG.text;
        if (string.IsNullOrEmpty(temp))
        {
            //ShowNotice("Nhap Level");
            return -1;
        }

        int data = int.Parse(temp);

        return data;
    }



    private void ExportMkt(MapInfo data)
    {
        string dataJsonTemp = JsonConvert.SerializeObject(data);
        PlayerPrefs.SetString(GameConfig.keyDataMkt + GameConfig.lvMkt.ToString(), dataJsonTemp);
        PlayerPrefs.Save();

        ShowNotice("Export Done");

        if (data.listExitsInMap.Count > 0) {
            return;
        }

        if (data.listBarrierInMap.Count > 0)
        {
            return;
        }

        if (data.listBrickInMap.Count > 0)
        {
            return;
        }

        if (data.listStopInMap.Count > 0)
        {
            return;
        }

        ShowNotice("Export To Destop Done");
        ExportToDesktop("levelNew",dataJsonTemp);
    }


    public static void ExportToDesktop(string fileName, string json)
    {
        try
        {
            string desktopPath = System.Environment.GetFolderPath(
            System.Environment.SpecialFolder.Desktop
        );

            string filePath = Path.Combine(desktopPath, fileName);
            File.WriteAllText(filePath, json);

            Debug.Log("JSON exported to Desktop: " + filePath);
        }
        catch
        {
            Debug.Log("Error request export data");
        }
    }

    #endregion
}
