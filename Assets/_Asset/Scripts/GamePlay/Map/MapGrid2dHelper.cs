using System.Collections.Generic;
using UnityEngine;

public class MapGrid2dHelper : MonoBehaviour
{
    public int column;
    public int row;

    public int totalCell => column * row;

    public float sizeCell = 1f;                // kích thước ô
    public Vector3 originPoint = Vector3.zero; // đểm bắt đầu của map

    private Vector3 originPointWorld;

    // =============================
    // INIT MAP
    // =============================

    public void InitMap(int col, int row, Vector3? origin = null, float size = 1)
    {
        this.column = col;
        this.row = row;
        this.sizeCell = size;

        if (origin != null)
        {
            this.originPoint = (Vector3)origin;
        }

        originPointWorld = originPoint - new Vector3(0.5f, 0f, 0.5f);
    }

    // =============================
    // Index ↔ (x,y)
    // =============================

    public Vector2Int IndexToXY(int index)
    {
        int x = index % column;
        int y = index / column;

        return new Vector2Int(x, y);
    }

    public int XYToIndex(int x, int y)
    {
        int index = y * column + x;
        return index;
    }

    // =============================
    // Kiểm tra trong map
    // =============================

    public bool InMap(int index)
    {
        bool inside = index >= 0 && index < totalCell;
        return inside;
    }

    public bool InMap(int x, int y)
    {
        bool insideX = (x >= 0) && (x < column);
        bool insideY = (y >= 0) && (y < row);

        bool inside = insideX && insideY;
        return inside;
    }

    // =============================
    // HÀNG XÓM
    // =============================

    public int GetLeft(int index)
    {
        Vector2Int xy = IndexToXY(index);

        int newX = xy.x - 1;
        int newY = xy.y;

        bool canMove = InMap(newX, newY);

        if (!canMove)
        {
            return -1;
        }

        int newIndex = XYToIndex(newX, newY);
        return newIndex;
    }

    public int GetRight(int index)
    {
        Vector2Int xy = IndexToXY(index);

        int newX = xy.x + 1;
        int newY = xy.y;

        bool canMove = InMap(newX, newY);

        if (!canMove)
        {
            return -1;
        }

        int newIndex = XYToIndex(newX, newY);
        return newIndex;
    }

    public int GetUp(int index)
    {
        Vector2Int xy = IndexToXY(index);

        int newX = xy.x;
        int newY = xy.y + 1;

        bool canMove = InMap(newX, newY);

        if (!canMove)
        {
            return -1;
        }

        int newIndex = XYToIndex(newX, newY);
        return newIndex;
    }

    public int GetDown(int index)
    {
        Vector2Int xy = IndexToXY(index);

        int newX = xy.x;
        int newY = xy.y - 1;

        bool canMove = InMap(newX, newY);

        if (!canMove)
        {
            return -1;
        }

        int newIndex = XYToIndex(newX, newY);
        return newIndex;
    }


    // =============================
    // LẤY VỊ TRÍ WORLD TỪ INDEX
    // =============================

    public Vector3 GetWorldPositionFromIndex(int index)
    {
        // Lấy tọa độ grid 2D theo (x,z)
        Vector2Int grid = IndexToXY(index);

        float worldX = originPoint.x + (grid.x * sizeCell);
        float worldZ = originPoint.z + (grid.y * sizeCell);

        // Không đổi trục Y (để đặt nhân vật/đối tượng lên mặt phẳng)
        float worldY = originPoint.y;

        Vector3 worldPos = new Vector3(worldX, worldY, worldZ);
        return worldPos;
    }


    // =============================
    // LẤY HƯỚNG DI CHUYỂN
    // =============================

    public DirectionMove GetDirection(int currentIndex, int nextIndex)
    {
        Vector2Int cur = IndexToXY(currentIndex);
        Vector2Int nxt = IndexToXY(nextIndex);

        int xA = cur.x;
        int zA = cur.y;

        int xB = nxt.x;
        int zB = nxt.y;

        // Trục Z → UP/DOWN
        if (zB > zA)
        {
            return DirectionMove.UP;
        }

        if (zB < zA)
        {
            return DirectionMove.DOWN;
        }

        // Trục X → RIGHT/LEFT
        if (xB > xA)
        {
            return DirectionMove.RIGHT;
        }

        if (xB < xA)
        {
            return DirectionMove.LEFT;
        }

        Debug.LogWarning("Hai index giống nhau, không xác định hướng!");
        return DirectionMove.UP; // default tránh lỗi
    }

    public int WorldPosToIndex(Vector3 worldPos)
    {
        float offsetX = worldPos.x - originPointWorld.x;
        float offsetZ = worldPos.z - originPointWorld.z;

        int x = Mathf.FloorToInt(offsetX / sizeCell);
        int z = Mathf.FloorToInt(offsetZ / sizeCell);

        bool inside = InMap(x, z);
        if (!inside)
        {
            return -1;
        }

        int index = XYToIndex(x, z);
        return index;
    }

    //public int GetIndexFromWorldPosition(Vector3 worldPos)
    //{
    //    float localX = worldPos.x - originPoint.x;
    //    float localZ = worldPos.z - originPoint.z;

    //    int x = Mathf.FloorToInt(localX / sizeCell);
    //    int y = Mathf.FloorToInt(localZ / sizeCell);

    //    if (!InMap(x, y))
    //        return -1;

    //    return XYToIndex(x, y);
    //}

    public Vector2 GetDirectionGecko(int indexHead, int indexNecko)
    {
        int headX = indexHead % column;
        int headY = indexHead / column;

        int neckX = indexNecko % column;
        int neckY = indexNecko / column;

        // Determine direction (dx, dy)
        int dx = headX - neckX;
        int dy = headY - neckY;

        return new Vector2(dx, dy);

    }
    public  int ConvertIndexExpand4Dir(
    int indexOld,
    int oldRow,
    int oldColumn,
    int expandRow,
    int expandColumn
)
    {
        // Tính kích thước map mới
        int newColumn = oldColumn + expandColumn * 2;

        // Tách tọa độ cũ
        int oldX = indexOld % oldColumn;
        int oldY = indexOld / oldColumn;

        // Dịch tọa độ do mở rộng 4 hướng
        int newX = oldX + expandColumn;
        int newY = oldY + expandRow;

        // Ghép index mới
        int indexNew = newY * newColumn + newX;

        return indexNew;
    }

    public int ConvertIndexExpandTop(
    int indexOld,
    int oldRow,
    int oldColumn,
    int expandRow
)
    {
        int newColumn = oldColumn;

        int oldX = indexOld % oldColumn;
        int oldY = indexOld / oldColumn;

        int newX = oldX;
        int newY = oldY + expandRow;

        return newY * newColumn + newX;
    }
}
