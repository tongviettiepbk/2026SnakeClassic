using System.Collections.Generic;
using System.Linq;
using EA.Line3D;
using UnityEngine;

public class GeckoBody : MonoBehaviour
{
    [SerializeField] LineRenderer3D line;

    private List<Vector3> positionStarts;
    public List<Vector3> PositionStarts => positionStarts;

    private List<Vector3> positionLasts;
    private List<Vector3> positionSmoothLasts;

    public void Init(List<Vector3> positions)
    {
        positionStarts = positions;
        positionLasts = positions.ToList();

        var smoothPositions = SmoothPoints(positions);
        smoothPositions.RemoveDuplicates();
        positionSmoothLasts = smoothPositions.ToList();

        line.PointsCount = smoothPositions.Count;
        for (int i = 0; i < smoothPositions.Count; i++)
        {
            line.SetPoint(i, smoothPositions[i]);
        }
    }

    public void UpdateListStartPos(List<Vector3> positions, bool reload = false)
    {
        positionStarts.Clear();
        positionStarts = positions;

        if(reload)
        {
            positionLasts = positions.ToList();
            
            var smoothPositions = SmoothPoints(positions);
            smoothPositions.RemoveDuplicates();
            positionSmoothLasts = smoothPositions.ToList();

            line.PointsCount = smoothPositions.Count;
            for (int i = 0; i < smoothPositions.Count; i++)
            {
                line.SetPoint(i, smoothPositions[i]);
            }
        }
    }

    public void UpdateBody(List<Vector3> positions)
    {
        // MappingSmoothPosition(positions);
        var smoothPositions = SmoothPoints(positions);
        smoothPositions.RemoveDuplicates();

        line.PointsCount = smoothPositions.Count;
        for (int i = 0; i < smoothPositions.Count; i++)
        {
            line.SetPoint(i, smoothPositions[i]);
        }
    }

    public void ReloadBody()
    {
        var smoothPositions = SmoothPoints(positionStarts);
        smoothPositions.RemoveDuplicates();
        positionSmoothLasts = smoothPositions.ToList();
        
        line.PointsCount = smoothPositions.Count;
        line.enabled = false;
        for (int i = 0; i < smoothPositions.Count; i++)
        {
            line.SetPoint(i, smoothPositions[i]);
        }
        line.enabled = true;
    }

    // private void MappingSmoothPosition(List<Vector3> positions)
    // {
    //     if(positionLasts.Count <= 2) return;

    //     int countLast = positionLasts.Count;
    //     int countNew = positions.Count;

    //     while (countLast >= 0)
    //     {
    //         countLast--;
    //         var plast = positionLasts[countLast];
    //         var pnew = positions[countNew];
    //     }

    //     Vector3 newPoint;
    //     for (int i = positionLasts.Count - 1; i >= 0; i--)
    //     {
    //         newPoint = positions[i];

    //         var p = positionLasts[i];
    //         var pnew = positions[i];
    //     }
    //     // if()
    // }

    public static List<Vector3> SmoothPoints(List<Vector3> points, float cornerRadius = 0.5f, int smoothSegments = 10, float angleThreshold = 150f)
    {
        if(!GameConfig.SMOOTH_CORNER)
        {
            return new List<Vector3>(points);
        }

        if (points == null || points.Count < 3)
            return new List<Vector3>(points);

        List<Vector3> smooth = new List<Vector3>();

        for (int i = 0; i < points.Count; i++)
        {
            if (i == 0 || i == points.Count - 1)
            {
                smooth.Add(points[i]);   // điểm đầu & cuối giữ nguyên
                continue;
            }

            Vector3 p0 = points[i - 1];
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];

            Vector3 dir1 = (p0 - p1).normalized;
            Vector3 dir2 = (p2 - p1).normalized;

            float angle = Vector3.Angle(dir1, dir2); // góc cua

            // CHỈ SMOOTH KHI GÓC NHỎ HƠN NGƯỠNG -> cua gắt
            if (angle > angleThreshold)
            {
                smooth.Add(p1); // quá thẳng, không cần bo
                continue;
            }

            Vector3 start = p1 + dir1 * cornerRadius;
            Vector3 end   = p1 + dir2 * cornerRadius;

            smooth.Add(start);

            // nội suy tạo curve
            for (int s = 1; s <= smoothSegments; s++)
            {
                float t = (float)s / smoothSegments;
                smooth.Add(Bezier(start, p1, end, t));
            }

            smooth.Add(end);
        }

        return smooth;
    }

    static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        // Bezier curve bậc 2
        return (1 - t) * (1 - t) * a +
               2 * (1 - t) * t * b +
                 t * t * c;
    }
}

public class GeckoWaveData
{
    public Vector3 position;
    public Vector3 positionTarget;
    public Vector3 direction;
    public bool isLeft;

    public void Init(Vector3 position, Vector3 positionTarget, bool isLeft)
    {
        
    }

    public void Update(float deltaTime)
    {
        
    }
}
