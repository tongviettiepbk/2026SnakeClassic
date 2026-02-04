using System.Collections.Generic;
using UnityEngine;

public class MapGeneratorBasic
{
    private MapGrid2dHelper grid;
    private int rows;
    private int cols;

    // Đánh dấu node đã có gecko chiếm
    private bool[] occupied;

    public MapGeneratorBasic(MapGrid2dHelper grid)
    {
        this.grid = grid;
        this.rows = grid.row;
        this.cols = grid.column;

        occupied = new bool[rows * cols];
    }

    // ===================================================================
    // TẠO MAPINFO HOÀN CHỈNH
    // ===================================================================
    public MapInfo GenerateMapInfo(int countGecko, int minLen, int maxLen)
    {
        List<GeckoDataInMap> listGecko = new List<GeckoDataInMap>();

        for (int i = 0; i < countGecko; i++)
        {
            GeckoDataInMap g = GenerateGecko(minLen, maxLen, 0);

            if (g != null)
                listGecko.Add(g);
            else
                Debug.LogWarning($"Không thể sinh gecko thứ {i + 1}");
        }

        MapInfo info = new MapInfo();
        info.column = cols;
        info.row = rows;
        info.listGeckoInMap = listGecko;

        // Các obstacle không dùng
        info.listBrickInMap = null;
        info.listExitsInMap = null;
        info.listStopInMap = null;
        info.listBarrierInMap = null;

        // Optional
        info.score = 0;
        info.timeLimit = 0;

        return info;
    }

    // Tạo Gecko với đầu vào là %gecko chiếm trong map
    public MapInfo GenerateMapRateGecko(float rateGeckoInMap, int minLen, int maxLen)
    {
        int totalNodeMap = rows * cols;
        int targetNodeFill = Mathf.RoundToInt(totalNodeMap * rateGeckoInMap);

        int currentFill = 0;
        List<GeckoDataInMap> listGecko = new List<GeckoDataInMap>();

        int safeBreak = 3000;   // tránh vòng lặp vô hạn
        int failCount = 0;

        while (currentFill < targetNodeFill && safeBreak-- > 0)
        {
            int remainNeed = targetNodeFill - currentFill;

            // Tự động giảm độ dài để khớp phần còn lại
            int realMin = Mathf.Min(minLen, remainNeed);
            int realMax = Mathf.Min(maxLen, remainNeed);

            int colorTemp = Random.Range(0, 7); // Màu ngẫu nhiên tạm thời
            GeckoDataInMap g = GenerateGecko(realMin, realMax, colorTemp);

            if (g != null)
            {
                listGecko.Add(g);
                currentFill += g.listNode.Count;
                failCount = 0; // reset
            }
            else
            {
                failCount++;

                // Nếu fail nhiều lần nghĩa là map không còn vị trí để sinh
                if (failCount > 50)
                {
                    Debug.LogWarning("Không thể sinh thêm gecko để đạt tỷ lệ mong muốn.");
                    break;
                }
            }
        }

        // Tạo MapInfo cuối cùng
        MapInfo info = new MapInfo();
        info.column = cols;
        info.row = rows;
        info.listGeckoInMap = listGecko;

        info.listBrickInMap = null;
        info.listExitsInMap = null;
        info.listStopInMap = null;
        info.listBarrierInMap = null;

        info.score = 0;
        info.timeLimit = 0;

        Debug.Log($"GenerateMapInfo: yêu cầu {targetNodeFill} nodes, tạo được {currentFill} nodes");

        return info;
    }


    public MapInfo GenerateRateGeckoWithBaseMap(
    MapInfo baseMap,
    float rateGeckoInMap,
    int minLen,
    int maxLen
)
    {
        grid.row = baseMap.row;
        grid.column = baseMap.column;
        occupied = null;
        occupied = new bool[grid.row * grid.column];

        // Reset trước cho an toàn
        for (int i = 0; i < occupied.Length; i++)
            occupied[i] = false;

        foreach (var g in baseMap.listGeckoInMap)
        {
            foreach (int idx in g.listNode)
            {
                if (idx >= 0 && idx < occupied.Length)
                    occupied[idx] = true;
            }
        }

        int totalNodeMap = baseMap.row * baseMap.column;
        int targetNodeFill = Mathf.RoundToInt(totalNodeMap * rateGeckoInMap);

        List<GeckoDataInMap> listGecko = new List<GeckoDataInMap>();

        int currentFill = 0;

        // 1️⃣ Copy gecko cũ
        if (baseMap != null && baseMap.listGeckoInMap != null)
        {
            foreach (var g in baseMap.listGeckoInMap)
            {
                listGecko.Add(g);
                currentFill += g.listNode.Count;
            }
        }

        // 2️⃣ Nếu đã đủ thì trả luôn
        if (currentFill >= targetNodeFill)
        {
            Debug.Log("Map đã đạt hoặc vượt tỷ lệ gecko yêu cầu.");
            return baseMap;
        }

        int safeBreak = 3000;
        int failCount = 0;

        // 3️⃣ Sinh thêm gecko
        while (currentFill < targetNodeFill && safeBreak-- > 0)
        {
            int remainNeed = targetNodeFill - currentFill;

            int realMin = Mathf.Min(minLen, remainNeed);
            int realMax = Mathf.Min(maxLen, remainNeed);

            int colorTemp = Random.Range(0, 7); // Màu ngẫu nhiên tạm thời

            GeckoDataInMap g = GenerateGecko(realMin, realMax, colorTemp);

            if (g != null)
            {
                listGecko.Add(g);
                currentFill += g.listNode.Count;
                failCount = 0;
            }
            else
            {
                failCount++;
                if (failCount > 50)
                {
                    Debug.LogWarning("Không thể sinh thêm gecko trên map hiện tại.");
                    break;
                }
            }
        }

        // 4️⃣ Tạo MapInfo mới
        MapInfo info = new MapInfo();
        info.column = cols;
        info.row = rows;
        info.listGeckoInMap = listGecko;

        info.listBrickInMap = null;
        info.listExitsInMap = null;
        info.listStopInMap = null;
        info.listBarrierInMap = null;

        info.score = baseMap != null ? baseMap.score : 0;
        info.timeLimit = baseMap != null ? baseMap.timeLimit : 0;

        Debug.Log($"Extend Map: target {targetNodeFill}, final {currentFill}");

        return info;
    }

    // ===================================================================
    // TẠO 1 GECKO
    // ===================================================================
    public GeckoDataInMap GenerateGecko(int minLen, int maxLen, int color,bool isHeadEscapse = true)
    {
        for (int attempt = 0; attempt < 200; attempt++)
        {
            GeckoDataInMap g = TryGenerateOne(minLen, maxLen, color, isHeadEscapse);

            if (g != null)
            {
                // Đánh dấu map
                foreach (int idx in g.listNode)
                    occupied[idx] = true;

                return g;
            }
        }

        Debug.LogError("Không thể sinh gecko sau nhiều lần thử!");
        return null;
    }


    // ===================================================================
    // CỐ GẮNG SINH 1 GECKO
    // ===================================================================
    private GeckoDataInMap TryGenerateOne(int minLen, int maxLen, int color, bool isHeadEscapse)
    {
        int targetLength = Random.Range(minLen, maxLen + 1);

        // 1) Chọn HEAD
        int head = RandomHead();
        if (head == -1) return null;

        List<int> neighbors = GetFreeNeighbors(head);
        if (neighbors.Count == 0)
            return null;

        // 2) TÌM NECK HỢP LỆ (phải escape được)
        int neck = -1;
        Vector2 escapeDir = Vector2.zero;
        List<int> escapePath = null;

        foreach (int nb in neighbors)
        {
            Vector2 dir = grid.GetDirectionGecko(head, nb);  // ← HƯỚNG CHÍNH THỨC

            List<int> path = ComputeEscapePath(head, dir, isHeadEscapse);
            if (path != null && path.Count > 0)
            {
                neck = nb;
                escapeDir = dir;
                escapePath = path;
                break;
            }
        }

        if (neck == -1)
            return null;

        // 3) BẮT ĐẦU GHÉP GECKO
        List<int> listNode = new List<int>();
        listNode.Add(head);
        listNode.Add(neck);

        // 4) EXPAND TIẾP
        while (listNode.Count < targetLength)
        {
            int last = listNode[listNode.Count - 1];
            List<int> candidates = GetFreeNeighbors(last);

            // Chỉ lấy node nào KHÔNG thuộc escapePath và KHÔNG thuộc gecko hiện có
            candidates.RemoveAll(idx =>
                escapePath.Contains(idx) ||
                listNode.Contains(idx)
            );

            if (candidates.Count == 0)
                break;

            int next = candidates[Random.Range(0, candidates.Count)];
            listNode.Add(next);
        }

        // 5) Phải có ít nhất 2 node
        if (listNode.Count < 2)
            return null;

        // 6) TRẢ VỀ KẾT QUẢ
        return new GeckoDataInMap
        {
            colorGecko = color,
            listNode = listNode,
            indexHead = head
        };
    }

    // ===================================================================
    // SUPPORT FUNCTIONS
    // ===================================================================

    private int RandomHead()
    {
        List<int> free = new List<int>();

        for (int i = 0; i < occupied.Length; i++)
        {
            if (!occupied[i])
                free.Add(i);
        }

        if (free.Count == 0)
            return -1;

        return free[Random.Range(0, free.Count)];
    }

    private List<int> GetFreeNeighbors(int index)
    {
        List<int> res = new List<int>();

        int l = grid.GetLeft(index);
        int r = grid.GetRight(index);
        int u = grid.GetUp(index);
        int d = grid.GetDown(index);

        if (l != -1 && !occupied[l]) res.Add(l);
        if (r != -1 && !occupied[r]) res.Add(r);
        if (u != -1 && !occupied[u]) res.Add(u);
        if (d != -1 && !occupied[d]) res.Add(d);

        return res;
    }

    // Tạo đường escape (tất cả node HEAD đi thẳng theo escapeDir)
    private List<int> ComputeEscapePath(int start, Vector2 escapeDir, bool isHeadEscapse)
    {
        List<int> path = new List<int>();

        int dx = (int)escapeDir.x;
        int dy = (int)escapeDir.y;

        int x = start % cols;
        int y = start / cols;

        while (true)
        {
            x += dx;
            y += dy;

            if (x < 0 || x >= cols || y < 0 || y >= rows)
                break; // thoát map

            int idx = y * cols + x;

            if (isHeadEscapse)
            {
                if (occupied[idx])
                    return null; // Không escape được
            }

            path.Add(idx);
        }

        return path;
    }

    public MapInfo ExpandMapInfo4Dir(
     MapInfo oldMap,
     int expandRow,
     int expandColumn
 )
    {
        if (oldMap == null)
            return null;

        int oldRow = oldMap.row;
        int oldColumn = oldMap.column;

        int newRow = oldRow + expandRow * 2;
        int newColumn = oldColumn + expandColumn * 2;

        MapInfo newMap = new MapInfo();
        newMap.row = newRow;
        newMap.column = newColumn;

        newMap.score = oldMap.score;
        newMap.timeLimit = oldMap.timeLimit;

        // Không dùng obstacle
        newMap.listBrickInMap = null;
        newMap.listExitsInMap = null;
        newMap.listStopInMap = null;
        newMap.listBarrierInMap = null;

        // ================================
        // Update Gecko
        // ================================
        if (oldMap.listGeckoInMap != null)
        {
            newMap.listGeckoInMap = new List<GeckoDataInMap>();

            foreach (var g in oldMap.listGeckoInMap)
            {
                GeckoDataInMap newG = new GeckoDataInMap();
                newG.colorGecko = g.colorGecko;
                newG.listNode = new List<int>();

                // Update body
                foreach (int oldIndex in g.listNode)
                {
                    int newIndex = grid.ConvertIndexExpand4Dir(
                        oldIndex,
                        oldRow,
                        oldColumn,
                        expandRow,
                        expandColumn
                    );

                    newG.listNode.Add(newIndex);
                }

                // Update head
                newG.indexHead = grid.ConvertIndexExpand4Dir(
                    g.indexHead,
                    oldRow,
                    oldColumn,
                    expandRow,
                    expandColumn
                );

                newMap.listGeckoInMap.Add(newG);
            }
        }
        else
        {
            newMap.listGeckoInMap = null;
        }

        // =================================================
        // BRICK
        // =================================================
        if (oldMap.listBrickInMap != null)
        {
            newMap.listBrickInMap = new List<BrickDataInMap>();

            foreach (var b in oldMap.listBrickInMap)
            {
                BrickDataInMap nb = new BrickDataInMap();
                nb.hp = b.hp;

                nb.indexInMap = grid.ConvertIndexExpand4Dir(
                    b.indexInMap, oldRow, oldColumn,
                    expandRow, expandColumn
                );
                newMap.listBrickInMap.Add(nb);
            }
        }
        else newMap.listBrickInMap = null;

        // =================================================
        // EXIT
        // =================================================
        if (oldMap.listExitsInMap != null)
        {
            newMap.listExitsInMap = new List<ExitHoleDataInMap>();

            foreach (var e in oldMap.listExitsInMap)
            {
                ExitHoleDataInMap ne = new ExitHoleDataInMap();

                ne.color = e.color;
                ne.indexInMap = grid.ConvertIndexExpand4Dir(
                    e.indexInMap, oldRow, oldColumn,
                    expandRow, expandColumn
                );
                newMap.listExitsInMap.Add(ne);
            }
        }
        else newMap.listExitsInMap = null;

        // =================================================
        // STOP
        // =================================================
        if (oldMap.listStopInMap != null)
        {
            newMap.listStopInMap = new List<StopDataInMap>();

            foreach (var s in oldMap.listStopInMap)
            {
                StopDataInMap ns = new StopDataInMap();
                ns.indexInMap =grid.ConvertIndexExpand4Dir(
                    s.indexInMap, oldRow, oldColumn,
                    expandRow, expandColumn
                );
                newMap.listStopInMap.Add(ns);
            }
        }
        else newMap.listStopInMap = null;

        // =================================================
        // BARRIER
        // =================================================
        if (oldMap.listBarrierInMap != null)
        {
            newMap.listBarrierInMap = new List<BarrierDataInMap>();

            foreach (var b in oldMap.listBarrierInMap)
            {
                BarrierDataInMap nb = new BarrierDataInMap();
                nb.isOpen = b.isOpen;

                nb.indexInMap =grid.ConvertIndexExpand4Dir(
                    b.indexInMap, oldRow, oldColumn,
                    expandRow, expandColumn
                );
                newMap.listBarrierInMap.Add(nb);
            }
        }
        else newMap.listBarrierInMap = null;

        return newMap;
    }

    public MapInfo ExpandMapInfoTop(
    MapInfo oldMap,
    int expandRow
)
    {
        if (oldMap == null)
            return null;

        int oldRow = oldMap.row;
        int oldColumn = oldMap.column;

        int newRow = oldRow + expandRow;
        int newColumn = oldColumn;

        MapInfo newMap = new MapInfo();
        newMap.row = newRow;
        newMap.column = newColumn;

        newMap.score = oldMap.score;
        newMap.timeLimit = oldMap.timeLimit;

        // ================================
        // GECKO
        // ================================
        if (oldMap.listGeckoInMap != null)
        {
            newMap.listGeckoInMap = new List<GeckoDataInMap>();

            foreach (var g in oldMap.listGeckoInMap)
            {
                GeckoDataInMap newG = new GeckoDataInMap();
                newG.colorGecko = g.colorGecko;
                newG.listNode = new List<int>();

                foreach (int oldIndex in g.listNode)
                {
                    int newIndex = grid.ConvertIndexExpandTop(
                        oldIndex,
                        oldRow,
                        oldColumn,
                        expandRow
                    );
                    newG.listNode.Add(newIndex);
                }

                newG.indexHead = grid.ConvertIndexExpandTop(
                    g.indexHead,
                    oldRow,
                    oldColumn,
                    expandRow
                );

                newMap.listGeckoInMap.Add(newG);
            }
        }
        else newMap.listGeckoInMap = null;

        // ================================
        // BRICK
        // ================================
        if (oldMap.listBrickInMap != null)
        {
            newMap.listBrickInMap = new List<BrickDataInMap>();
            foreach (var b in oldMap.listBrickInMap)
            {
                BrickDataInMap nb = new BrickDataInMap();
                nb.hp = b.hp;
                nb.indexInMap = grid.ConvertIndexExpandTop(
                    b.indexInMap,
                    oldRow,
                    oldColumn,
                    expandRow
                );
                newMap.listBrickInMap.Add(nb);
            }
        }
        else newMap.listBrickInMap = null;

        // ================================
        // EXIT
        // ================================
        if (oldMap.listExitsInMap != null)
        {
            newMap.listExitsInMap = new List<ExitHoleDataInMap>();
            foreach (var e in oldMap.listExitsInMap)
            {
                ExitHoleDataInMap ne = new ExitHoleDataInMap();
                ne.color = e.color;
                ne.indexInMap = grid.ConvertIndexExpandTop(
                    e.indexInMap,
                    oldRow,
                    oldColumn,
                    expandRow
                );
                newMap.listExitsInMap.Add(ne);
            }
        }
        else newMap.listExitsInMap = null;

        // ================================
        // STOP
        // ================================
        if (oldMap.listStopInMap != null)
        {
            newMap.listStopInMap = new List<StopDataInMap>();
            foreach (var s in oldMap.listStopInMap)
            {
                StopDataInMap ns = new StopDataInMap();
                ns.indexInMap = grid.ConvertIndexExpandTop(
                    s.indexInMap,
                    oldRow,
                    oldColumn,
                    expandRow
                );
                newMap.listStopInMap.Add(ns);
            }
        }
        else newMap.listStopInMap = null;

        // ================================
        // BARRIER
        // ================================
        if (oldMap.listBarrierInMap != null)
        {
            newMap.listBarrierInMap = new List<BarrierDataInMap>();
            foreach (var b in oldMap.listBarrierInMap)
            {
                BarrierDataInMap nb = new BarrierDataInMap();
                nb.isOpen = b.isOpen;
                nb.indexInMap = grid.ConvertIndexExpandTop(
                    b.indexInMap,
                    oldRow,
                    oldColumn,
                    expandRow
                );
                newMap.listBarrierInMap.Add(nb);
            }
        }
        else newMap.listBarrierInMap = null;

        return newMap;
    }

}
