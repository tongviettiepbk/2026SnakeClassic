using UnityEngine;

public class test : MonoBehaviour
{

    private void Start()
    {
        float groundY = 0f;

        // TRÁI
        Vector3 leftPoint = GetCameraPlaneIntersection(
            Camera.main,
            new Vector2(0f, 0.5f),
            groundY
        );

        DebugCustom.ShowDebugColorRed("trai", leftPoint);

        // GIỮA
        Vector3 centerPoint = GetCameraPlaneIntersection(
            Camera.main,
            new Vector2(0.5f, 0.5f),
            groundY
        );

        DebugCustom.ShowDebugColorRed("Giu", centerPoint);

        // PHẢI
        Vector3 rightPoint = GetCameraPlaneIntersection(
            Camera.main,
            new Vector2(1f, 0.5f),
            groundY
        );

        DebugCustom.ShowDebugColorRed("phai", rightPoint);

    }

    public static Vector3 GetCameraPlaneIntersection(
     Camera cam,
     Vector2 viewportPos,
     float planeY)
    {
        Ray ray = cam.ViewportPointToRay(
            new Vector3(viewportPos.x, viewportPos.y, 0)
        );

        Plane plane = new Plane(Vector3.up, new Vector3(0, planeY, 0));

        if (plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        return Vector3.zero;
    }


}
