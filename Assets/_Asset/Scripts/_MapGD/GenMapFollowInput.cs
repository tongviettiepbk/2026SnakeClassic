
using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum stateCreateMapGD
{
    idle,
    createGecko,
    createStop,
    createBrick,
    createHoleOut,
    createBarrier,
    remove
}

// chọn event muốn thực hiện Create: gecko,brick,stop,hole,barrier
// với mỗi cách tạo cần nhập thêm các giá trị tương ứng như id, color, hp, trạng thái
// Nếu muốn kết thúc quá trình tạo thì bấm Done
// khi sai thì bấm Remove để xóa ô đã chọn
// với tạo gecko sai thì nên xóa toàn bộ gecko đó rồi tạo lại từ đầu

public class GenMapFollowInput : MonoBehaviour
{
    public CameraGD cameraMain;
    public float maxSize = 20f;
    public TextAsset jsonMapInput;

    [Space(20)]
    public bool isEditMap = false;
    public string nameMap = "Map_01";
    public int column;
    public int row;

    [Space(20)]
    public CellGD objPrefabCell;
    [Space(20)]
    public List<Material> listMaterialColor;

    public Vector3 posStart = Vector3.zero;

    private float sizeCell = 1;
    private List<CellGD> listCell = new List<CellGD>();

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


    private Dictionary<int, List<CellGD>> dictGeckoInMap = new Dictionary<int, List<CellGD>>(); // key: id gecko, value: list index in map theo thứ tự từ đuôi đến đầu
    private int idGeckoCurrent = 0;
    private int idValueCurrent = 0;

    public List<CellGD> listCellGeckoCurrent = new List<CellGD>();

    [Space(20)]
    public List<CellGD> listCellStop = new List<CellGD>();
    public List<CellGD> listCellBrick = new List<CellGD>();
    public List<CellGD> listCellHoleOut = new List<CellGD>();
    public List<CellGD> listCellBarrier = new List<CellGD>();

    private MapInfo mapInfo;
    private void Start()
    {
        btCreateGecko.onClick.AddListener(ClickBtCreateGecko);
        btCreateGekoDone.onClick.AddListener(ClickBtDone);
        btRemove.onClick.AddListener(ClickBtRemoveCell);

        btCreateBarrier.onClick.AddListener(ClickBtCreateBarrier);
        btCreateBrick.onClick.AddListener(ClickBtCreateBrick);
        btCreateHoleEnd.onClick.AddListener(ClickBtCreateHoleEnd);
        btCreateStop.onClick.AddListener(ClickBtCreateStop);

        btExportJson.onClick.AddListener(() =>
        {
            ClickBtDone();
            ExportJson();
        });
        txtState.text = "None";

        if (isEditMap && jsonMapInput != null)
        {
            LoadMapFromJson(jsonMapInput.text);
            return;
        }
        else
        {
            CreateMap();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectGecko();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(GameConfig.SCENE_GD);
        }
    }

    #region Create Gecko

    private void ClickBtCreateGecko()
    {
        if (currentStateCreateMapGD != stateCreateMapGD.idle)
        {
            ShowNotice("Hay ket thuc state hien tai bang cach click Done");
            return;
        }

        idGeckoCurrent = -1;
        idValueCurrent = -1;

        try
        {
            idGeckoCurrent = int.Parse(inputIdNameGecko.text);
            idValueCurrent = int.Parse(inputColor.text);
        }
        catch
        {

        }

        if (idGeckoCurrent < 0)
        {
            txtState.text = "Error nhap lai id";
            return;
        }

        if (dictGeckoInMap.ContainsKey(idGeckoCurrent))
        {
            txtState.text = "Error nhap lai id da trung";
            return;
        }

        ShowNotice("Bat dau tao gecko id: " + idGeckoCurrent + " color: " + idValueCurrent);

        listCellGeckoCurrent.Clear();
        currentStateCreateMapGD = stateCreateMapGD.createGecko;
        txtState.text = "Start Create Gecko hay nhap id va idcolor";
    }

    private void ClickBtDone()
    {
        switch (currentStateCreateMapGD)
        {
            case stateCreateMapGD.createGecko:
                CreatGeckoDone();
                break;

            case stateCreateMapGD.createStop:
                ShowNotice("Da tao xong Stop voi so phan tu:" + listCellStop.Count);

                break;

            case stateCreateMapGD.createBrick:
                ShowNotice("Da tao xong Brick voi so phan tu:" + listCellBrick.Count);
                break;

            case stateCreateMapGD.createBarrier:
                ShowNotice("Da tao xong Barier voi so phan tu:" + listCellBarrier.Count);
                break;

            case stateCreateMapGD.createHoleOut:
                ShowNotice("Da tao xong HOle Out  voi so phan tu:" + listCellHoleOut.Count);
                break;
        }

        txtState.text = "None";
        currentStateCreateMapGD = stateCreateMapGD.idle;
    }

    #region Click event

    private void ClickBtRemoveCell()
    {
        switch (currentStateCreateMapGD)
        {
            case stateCreateMapGD.createGecko:
                CreatGeckoDone();
                break;
        }

        currentStateCreateMapGD = stateCreateMapGD.remove;
        txtState.text = "Remove Cell";
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

    #endregion


    private void CreatGeckoDone()
    {
        dictGeckoInMap.Add(idGeckoCurrent, new List<CellGD>(listCellGeckoCurrent));
        currentStateCreateMapGD = stateCreateMapGD.idle;
        txtState.text = "None";
    }
    #endregion

    #region Map

    private void CreateMap()
    {
        float sizeMap = column + 2;
        if (sizeCell > maxSize) sizeCell = maxSize;

        cameraMain.SetCameraCenterMap(column);

        posStart.x = -column / 2;
        posStart.y = -row / 2;

        posStart.x = 0;
        posStart.y = 0;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var obj = Instantiate(objPrefabCell);

                Vector3 posTemp = Vector3.zero;
                posTemp.x = posStart.x + j * sizeCell;
                posTemp.y = posStart.y + i * sizeCell;

                obj.transform.position = posTemp;
                obj.gameObject.SetActive(true);

                CellGD cellTemp = obj.GetComponent<CellGD>();

                cellTemp.indexInmap = i * column + j;
                cellTemp.Init(this);
                listCell.Add(cellTemp);
            }
        }
    }

    private void DetectGecko()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            CellGD cellGeckoGD = hit.collider.GetComponent<CellGD>();
            if (cellGeckoGD != null)
            {
                ClickCellGecko(cellGeckoGD);
            }
        }
    }

    #endregion

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

    private void ClickCellGecko(CellGD cell)
    {
        switch (currentStateCreateMapGD)
        {
            case stateCreateMapGD.createGecko:
                CreateGecko(cell);

                break;
            case stateCreateMapGD.createBarrier:

                CreateBarrier(cell);
                break;
            case stateCreateMapGD.createBrick:

                CreateBrick(cell);

                break;
            case stateCreateMapGD.createStop:

                CreateStop(cell);

                break;
            case stateCreateMapGD.createHoleOut:
                CreateHoleOut(cell);
                break;
            case stateCreateMapGD.remove:

                switch (cell.typeNode)
                {
                    case TypeNodeValueInMap.NODE_GECKO:

                        if (dictGeckoInMap.ContainsKey(cell.idCell))
                        {
                            dictGeckoInMap[cell.idCell].Remove(cell);
                        }

                        if (dictGeckoInMap[cell.idCell].Count == 0)
                        {
                            dictGeckoInMap.Remove(cell.idCell);
                        }

                        break;

                    case TypeNodeValueInMap.OBSTALCE_STOP:
                        if (listCellStop.Contains(cell))
                            listCellStop.Remove(cell);
                        break;
                    case TypeNodeValueInMap.OBSTACLE_BRICK:
                        if (listCellBrick.Contains(cell))
                            listCellBrick.Remove(cell);
                        break;
                    case TypeNodeValueInMap.OBSTALCE_EXIT_HOLE:
                        if (listCellHoleOut.Contains(cell))
                            listCellHoleOut.Remove(cell);
                        break;
                    case TypeNodeValueInMap.OBSTALCE_BARRIER:
                        if (listCellBarrier.Contains(cell))
                            listCellBarrier.Remove(cell);
                        break;
                }

                cell.Remove();

                break;
        }
    }

    private void CreateGecko(CellGD cell)
    {
        if (cell.typeNode != TypeNodeValueInMap.EMPTY)
        {
            ShowNotice("khong the tao gecko o day, neu muon hay xoa no truoc");
            return;
        }

        if (listCellGeckoCurrent.Contains(cell))
        {
            return;
        }

        cell.SetNameGecko(idGeckoCurrent, idValueCurrent);


        if (listCellGeckoCurrent.Count == 0)
        {
            cell.ChoseHead();
        }
        else if (listCellGeckoCurrent.Count == 1)
        {
            cell.ChoseSecond();
        }

        listCellGeckoCurrent.Add(cell);
    }

    private void CreateBarrier(CellGD cell)
    {
        if (cell.typeNode != TypeNodeValueInMap.EMPTY)
        {
            ShowNotice("khong the tao createBarrier o day, neu muon hay xoa no truoc");
            return;
        }

        try
        {
            idValueCurrent = int.Parse(inputColor.text);
        }
        catch
        {
            ShowNotice("Nhap isopne cho barrier sai 0 or 1");
        }

        cell.SetBarrier(idValueCurrent);
        listCellBarrier.Add(cell);
    }

    private void CreateStop(CellGD cell)
    {

        if (cell.typeNode != TypeNodeValueInMap.EMPTY)
        {
            ShowNotice("khong the tao createStop o day, neu muon hay xoa no truoc");
            return;
        }

        if (listCellStop.Contains(cell))
        {
            return;
        }

        ShowNotice("Ban vua tao ra STOP:" + cell.indexInmap);

        cell.SetStop();
        listCellStop.Add(cell);
    }

    private void CreateBrick(CellGD cell)
    {
        if (cell.typeNode != TypeNodeValueInMap.EMPTY)
        {
            ShowNotice("khong the tao createBrick o day, neu muon hay xoa no truoc");
            return;
        }

        try
        {
            idValueCurrent = int.Parse(inputColor.text);
        }
        catch
        {
            ShowNotice("Nhap hp cho brick sai");
        }

        Debug.Log("Ban vua tao ra BRICK:" + cell.indexInmap + " HP:" + idValueCurrent);
        cell.SetBrick(idValueCurrent);

        listCellBrick.Add(cell);
    }

    private void CreateHoleOut(CellGD cell)
    {
        if (cell.typeNode != TypeNodeValueInMap.EMPTY)
        {
            ShowNotice("khong the tao createHoleOut o day, neu muon hay xoa no truoc");
            return;
        }

        try
        {
            idValueCurrent = int.Parse(inputColor.text);
        }
        catch
        {
            ShowNotice("Nhap mau cho hold sai");
        }

        cell.SetHoleOut(idValueCurrent);
        listCellHoleOut.Add(cell);
    }

    private void ResetInputValue()
    {
        inputIdNameGecko.text = "";
        inputColor.text = "";
    }


    #region Export Json Map

    private void ExportJson()
    {
        MapInfo map = new MapInfo();
        map.column = column;
        map.row = row;

        map.listGeckoInMap = new List<GeckoDataInMap>();
        map.listBrickInMap = new List<BrickDataInMap>();
        map.listStopInMap = new List<StopDataInMap>();
        map.listExitsInMap = new List<ExitHoleDataInMap>();
        map.listBarrierInMap = new List<BarrierDataInMap>();

        // =============================
        // 1. GROUP CÁC GECKO
        // =============================
        foreach (var kv in dictGeckoInMap)
        {
            List<CellGD> listCells = kv.Value;

            if (listCells.Count == 0)
                continue;

            GeckoDataInMap data = new GeckoDataInMap();
            data.listNode = new List<int>();

            // LẤY COLOR TỪ CELL ĐẦU TIÊN (tất cả cell của 1 gecko luôn cùng màu)
            data.colorGecko = listCells[0].valueCell;

            // LẤY TOÀN BỘ NODE
            foreach (var cell in listCells)
            {
                data.listNode.Add(cell.indexInmap);
            }

            // TÌM HEAD
            // fallback an toàn
            data.indexHead = data.listNode[0];

            map.listGeckoInMap.Add(data);
        }

        // ============================================
        // 2. BRICK
        // ============================================

        foreach (var cell in listCellBrick)
        {
            BrickDataInMap brick = new BrickDataInMap();
            brick.indexInMap = cell.indexInmap;
            brick.hp = cell.valueCell; // HP LÀ valueCell TRONG TOOL

            map.listBrickInMap.Add(brick);
        }

        // ============================================
        // 3. STOP (lấy từ listCellStop)
        // ============================================

        foreach (var cell in listCellStop)
        {
            StopDataInMap stop = new StopDataInMap();
            stop.indexInMap = cell.indexInmap;

            map.listStopInMap.Add(stop);
        }

        // ============================================
        // 4. EXIT HOLE (lấy từ listCellExit)
        // ============================================

        foreach (var cell in listCellHoleOut)
        {
            ExitHoleDataInMap exit = new ExitHoleDataInMap();
            exit.indexInMap = cell.indexInmap;
            exit.color = cell.valueCell;   // màu hole

            map.listExitsInMap.Add(exit);
        }

        // ============================================
        // 5. BARRIER (lấy từ listCellBarrier)
        // ============================================

        foreach (var cell in listCellBarrier)
        {
            BarrierDataInMap bar = new BarrierDataInMap();
            bar.indexInMap = cell.indexInmap;
            bar.isOpen = cell.valueCell;   // 0 = đóng, 1 = mở

            map.listBarrierInMap.Add(bar);
        }

        var dataScore = MapDifficultyCalculator.Compute(map);
        map.score = dataScore.finalScore;

        string jsonMap = JsonConvert.SerializeObject(map, Formatting.Indented);

        DebugCustom.ShowDebugColorRed("MAP JSON: \n" + jsonMap);

        try
        {
            SaveJsonToMapGDDoing(nameMap, jsonMap);
        }
        catch
        {

        }

    }

    public static void SaveJsonToMapGDDoing(string fileName, string json)
    {
        // Thư mục bạn muốn lưu
        string folderPath = Application.dataPath + "/_Asset/MapData/_MapGDDoing/";

        // Tạo thư mục nếu chưa có
        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

        // Đường dẫn file
        string filePath = folderPath + fileName + ".json";

        // Ghi file
        System.IO.File.WriteAllText(filePath, json);

        Debug.Log("JSON saved to: " + filePath);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    #endregion

    #region Import

    public void LoadMapFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            ShowNotice("JSON null hoặc rỗng");
            return;
        }

        // Parse JSON thành MapInfo
        MapInfo map = JsonConvert.DeserializeObject<MapInfo>(json);
        if (map == null)
        {
            ShowNotice("Parse JSON thất bại!");
            return;
        }

        column = map.column;
        row = map.row;
        CreateMap();

        // Gecko 
        for (int i = 0; i < map.listGeckoInMap.Count; i++)
        {
            var geckoData = map.listGeckoInMap[i];
            int idGecko = i + 1; // Gán ID Gecko từ 1 trở đi

            for (int j = 0; j < geckoData.listNode.Count; j++)
            {
                int indexCell = geckoData.listNode[j];
                CellGD cell = listCell[indexCell];
                cell.SetNameGecko(idGecko, geckoData.colorGecko);
                if (j == 0)
                {
                    cell.ChoseHead();
                }
                else if (j == 1)
                {
                    cell.ChoseSecond();
                }

                if (!dictGeckoInMap.ContainsKey(idGecko))
                {
                    dictGeckoInMap[idGecko] = new List<CellGD>();
                }
                dictGeckoInMap[idGecko].Add(cell);
            }
        }

        // Brick
        foreach (var brickData in map.listBrickInMap)
        {
            int indexCell = brickData.indexInMap;
            CellGD cell = listCell[indexCell];
            cell.SetBrick(brickData.hp);
            listCellBrick.Add(cell);
        }

        // Stop
        foreach (var stopData in map.listStopInMap)
        {
            int indexCell = stopData.indexInMap;
            CellGD cell = listCell[indexCell];
            cell.SetStop();
            listCellStop.Add(cell);
        }
        // Exit Hole
        foreach (var exitData in map.listExitsInMap)
        {
            int indexCell = exitData.indexInMap;
            CellGD cell = listCell[indexCell];
            cell.SetHoleOut(exitData.color);
            listCellHoleOut.Add(cell);
        }
        // Barrier
        foreach (var barrierData in map.listBarrierInMap)
        {
            int indexCell = barrierData.indexInMap;
            CellGD cell = listCell[indexCell];
            cell.SetBarrier(barrierData.isOpen);
            listCellBarrier.Add(cell);
        }
    }

    #endregion

    #region Genera Shape

    public void GenerateHeartAndPrintJson()
    {
        // sinh map trái tim
        MapInfo heartMap = HeartMapGenerator.GenerateHeartMap(30, 30, 2, 30, seed: 123);

        // convert sang json
        string json = JsonConvert.SerializeObject(heartMap, Formatting.Indented);

        // in ra Console để bạn copy
        Debug.Log(json);

        SaveJsonToMapGDDoing("Map_Heart223", json);
    }

    public void GenerateTree()
    {
        MapInfo map = ShapeMapGenerator.Generate(
            cols: 26,
            rows: 26,
            shape: ShapeMapGenerator.ShapeType.Tree,
            minLen: 3,
            maxLen: 40,
            seed: 999
        );

        string json = JsonConvert.SerializeObject(map, Formatting.Indented);
        Debug.Log(json);

        SaveJsonToMapGDDoing("Map_Tree26", json);
    }

    public void GenerateStart()
    {
        MapInfo map = ShapeMapGenerator.Generate(
            cols: 26,
            rows: 26,
            shape: ShapeMapGenerator.ShapeType.Star,
            minLen: 3,
            maxLen: 40,
            seed: 999
        );

        string json = JsonConvert.SerializeObject(map, Formatting.Indented);
        Debug.Log(json);

        SaveJsonToMapGDDoing("Map_Star26", json);
    }

    public void GenerateBrickMap()
    {
        MapInfo map = MapBrickGenerator.Generate(
            cols: 20,
            rows: 20,
            difficulty: 3,
            seed: 2024
        );

        string json = JsonConvert.SerializeObject(map, Formatting.Indented);
        Debug.Log(json);
        SaveJsonToMapGDDoing("Map_Brick20_Diff3", json);
    }

    public void GenerateBrickMapTest()
    {
        MapInfo map = MapBrickGenerator.Generate(
            cols: 20,
            rows: 20,
            difficulty: 3,
            seed: 123
        );

        string json = JsonConvert.SerializeObject(map, Formatting.Indented);
        Debug.Log(json);

        SaveJsonToMapGDDoing("Map_Brick20_Diff3_Test", json);
    }

    public void GenPuzzleD2()
    {
        int rows = 14;
        int cols = 14;

        int difficulty = 0;   // 0 dễ – 3 cực khó
        int snakeCount = 11;   // bạn thay số tùy ý
        int seed = 3;

        MapInfo map = PuzzleGenerator.Generate(cols, rows, difficulty, snakeCount, seed);

        string json = JsonConvert.SerializeObject(map, Formatting.Indented);

        SaveJsonToMapGDDoing("Map_Puzzle10_Diff2_2", json);
    }

    #endregion


}
