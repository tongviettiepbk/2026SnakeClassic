using System;
using UnityEngine;

namespace EA.Line3D.Demo
{
    [ExecuteInEditMode]
    public class Line3DDemoAnimator : MonoBehaviour
    {
        [SerializeField] LineRenderer3D line;
        [Space]
        [SerializeField] bool localSpace;
        [SerializeField] int usedPointIndex;
        [SerializeField] int usedPointCount;
        [SerializeField] Transform[] points;

        Vector3[] vpoints;

        void Update()
        {
            if (vpoints == null || vpoints.Length != points.Length)
                Array.Resize(ref vpoints, points.Length);

            line.PointsCount = usedPointCount;

            for (int a = usedPointIndex, c = Mathf.Min(usedPointIndex + usedPointCount, points.Length); a < c; a++)
                line.SetPoint(a - usedPointIndex, localSpace ? points[a].localPosition : points[a].position);
        }
    }
}