using UnityEngine;

public class FxFirework : MonoBehaviour
{
    public float sizeDefault = 8;

    public Transform objleft;
    public Transform objRight;
    public Transform objCenter1;
    public Transform objCenter2;

    private float scale = 1f;

    private void Awake()
    {
        this.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        ActiveFx(false);
        SetPosFx();
        ActiveFx(true);
    }

    private void ActiveFx(bool isOff)
    {
        objleft.gameObject.SetActive(isOff);
        objRight.gameObject.SetActive(isOff);
        objCenter1.gameObject.SetActive(isOff);
        objCenter2.gameObject.SetActive(isOff);
    }

    private void SetPosFx()
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

        Vector3 centerTop = GetCameraPlaneIntersection(
            Camera.main,
            new Vector2(0.5f, 1f),
            groundY   
        );

        Vector3 centerBetween = (centerTop + centerPoint) * 0.5f;

        DebugCustom.ShowDebugColorRed("phai", rightPoint);

        // sale follow camera
        scale = CameraController.Instance.mainCamera.orthographicSize / sizeDefault;
        objleft.localScale = new Vector3(scale, scale, scale);
        objRight.localScale = new Vector3(scale, scale, scale);
        objCenter1.localScale = new Vector3(scale, scale, scale);
        objCenter2.localScale = new Vector3(scale, scale, scale);

        // pos follow camera
        objleft.position = new Vector3(leftPoint.x, leftPoint.y, leftPoint.z / 2);
        objRight.position = new Vector3(rightPoint.x, rightPoint.y, rightPoint.z / 2);

        objCenter1.position = centerBetween + new Vector3(0.5f, 0, 0);

        centerBetween.y += 20f;
        objCenter2.position = centerBetween + new Vector3(0.5f, 0, 0);

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

    public void GetInfoFirework()
    {

    }


}
