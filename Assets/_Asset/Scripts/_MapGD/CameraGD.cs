using UnityEngine;

public class CameraGD : MonoBehaviour
{
    public static CameraGD Instance { get; private set; }

    public Camera mainCamera;


    private Vector2 lastMousePos;
    private Vector2 currentMousePos;

    private bool dragging = false;
    private float sizeCameraDefault = 5f;
    private Vector3 posCameraCenter = Vector3.zero;

    private float dragSpeed = 0.05f;   // Tốc độ kéo camera
    private Vector2 lastPos;
    private bool isDragging = false;

    Vector3 posCamCurrent = Vector3.zero;
    Vector3 posTarget = Vector3.zero;
    Vector3 tempVector3 = Vector3.zero;

    //zoom in out
    private float zoomSpeed = 0.1f;
    public float minZoom = 3f;
    public float maxZoom = 15f;

    // phục vụ pinch zoom
    private float lastPinchDistance = 0f;
    private bool pinchStarted = false;

    private float timeCheck = 0.5f;
    private float timeCount = 0;

    private void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        MoveCamWithMouse();
        //SetCameraCenterMap(10);
    }

    public void SetCameraCenterMap(int col)
    {
        // Set camera position
        float sizeTemp = col;

        mainCamera.orthographicSize = sizeTemp + 2;

        Vector3 posCameraTemp = mainCamera.transform.position;

        posCameraTemp.x = sizeTemp / 2f - 0.5f;
        posCameraTemp.y = sizeTemp / 2f;

        mainCamera.transform.position = posCameraTemp;

        this.sizeCameraDefault = sizeTemp;
        this.posCameraCenter = posCameraTemp;
    }


    private void MoveCamWithMouse()
    {
        // Mouse Down
        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            lastMousePos = Input.mousePosition;
            posCamCurrent = this.transform.position;
        }

        // Mouse Hold + Move
        if (Input.GetMouseButton(0) && dragging)
        {
            currentMousePos = Input.mousePosition;

            Vector2 delta = currentMousePos - lastMousePos;

            float camFactor = mainCamera.orthographicSize / sizeCameraDefault;

            tempVector3.x = -delta.x * camFactor;
            tempVector3.y = -delta.y * camFactor;
            tempVector3.z = -10;

            posTarget = posCamCurrent + tempVector3 * dragSpeed;
            transform.position = posTarget;
        }

        // Mouse Up
        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        // ====== ZOOM PC (Scroll Wheel) ======
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            float newSize = mainCamera.orthographicSize - scroll * zoomSpeed * 100f;
            mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }


    public void RecenterCamera()
    {
        this.transform.position = posCameraCenter;
    }
}
