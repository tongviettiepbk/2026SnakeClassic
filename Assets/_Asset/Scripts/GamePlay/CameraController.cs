using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    public Camera mainCamera;
    public MapController mapController;

    public LayerMask groundLayer;
    public Transform tranLeft;
    public Transform tranRight;
    public Transform tranTop;
    public Transform tranBottom;

    public Vector3 posStart = Vector3.zero;

    ///  private
    private MapGrid2dHelper mapHelper;

    // support move
    private Vector2 wallTopBottom = Vector2.zero;

    private Vector3 startWorldPoint;
    private Vector2 startMousePoint;

    private float offsetWorldWithMouse;


    // support zoom
    private bool dragging = false;
    private Vector3 posCameraCenter = Vector3.zero;
    private Vector3 posCenterMap = Vector3.zero;

    public float dragSpeed = 0.01f;   // Tốc độ kéo camera

    //zoom in out
    public float zoomSpeed = 0.01f;
    public float minZoom = 3f;
    public float maxZoom = 15f;
    public float defaultZoom = 15f;
    private float sizeStart = 0;

    // phục vụ pinch zoom
    private float lastPinchDistance = 0f;
    private bool pinchStarted = false;

    private float timeCheck = 0.5f;
    private float timeCount = 0;

    // play anim
    private float animZoomTarget = 0;
    private bool animZoomPlaying = false;


    public float timePlayCameraStart = 1f;

    //Tut zoomOut
    private bool isTutZoomOut = false;

    public bool isLockCaremaByTut = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (isLockCaremaByTut)
        {
            return;
        }

        if (MapController.Instance != null && MapController.Instance.stateCurrentGame == StateGame.PAUSE)
        {
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }
        }

        if (animZoomPlaying)
        {
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        MoveCamWithMouse();
#else
        TouchZoom();
        TouchMoveCamera();
#endif
    }

    public void InitCameraByMap(int col, int row)
    {
        float aspect = (float)(Screen.height / Screen.width);

        float size = row;
        if (row * aspect > col)
            size = col;

        //if (GameConfig.IS_TABLET)
        //{
        //    Debug.Log("IsTablet");
        //    size -= 8;
        //}

        if (size < minZoom)
            size = minZoom;

        // Set camera position
        defaultZoom = size + 2;
        maxZoom = defaultZoom + 5;
        sizeStart = size;
    }

    public void SetCameraCenterMap()
    {
        this.mapHelper = mapController.mapHelper;
        var sizeTemp = mapController.mapHelper.column;

        mainCamera.orthographicSize = minZoom;

        Vector3 posCameraTemp = mainCamera.transform.position;
        this.posCenterMap = posCameraTemp;
        this.posCenterMap.x = sizeTemp / 2f - 0.5f;
        this.posCenterMap.y = sizeTemp / 2f;

        posCameraTemp.x = sizeTemp / 2f - 0.5f;
        posCameraTemp.y = mapController.mapHelper.row / 2f;

        mainCamera.transform.position = posCameraTemp;

        // wall left - right
        tranLeft.position = new Vector3(posCameraTemp.x - defaultZoom / 2, 0f, 0f);
        tranRight.position = new Vector3(posCameraTemp.x + defaultZoom / 2, 0f, 0f);

        // wall top - bottom
        tranTop.position = new Vector3(mainCamera.transform.position.x, this.mapHelper.row * GameConfig.sizeCell * 1.5f, 0f);
        tranBottom.position = new Vector3(mainCamera.transform.position.x, this.mapHelper.row * GameConfig.sizeCell * -0.5f, 0f);

        wallTopBottom = new Vector2(tranTop.position.y, tranBottom.position.y);

        // size anim
        this.posCameraCenter = posCameraTemp;

        PlayCamareStartGame(minZoom, defaultZoom, timePlayCameraStart);
    }


    public void TouchMoveCamera()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // Touch Down
            if (touch.phase == TouchPhase.Began)
            {
                startWorldPoint = GetWorldPosition(touch.position);
                startMousePoint = touch.position;
                offsetWorldWithMouse = -1f;

                dragging = true;
            }
            // Touch Move
            else if (touch.phase == TouchPhase.Moved && dragging)
            {
                Vector3 currentWorldPoint = GetWorldPosition(touch.position);
                Vector2 currentMousePoint = touch.position;

                if (offsetWorldWithMouse <= 0)
                {
                    float worldDistance = Vector3.Distance(startWorldPoint, currentWorldPoint);
                    float mouseDistance = Vector2.Distance(startMousePoint, currentMousePoint);

                    if (worldDistance > 0.02f && mouseDistance > 0)
                    {
                        offsetWorldWithMouse = worldDistance / mouseDistance;
                    }
                }

                if (offsetWorldWithMouse > 0)
                {
                    Vector3 delta = startMousePoint - currentMousePoint;
                    delta = new Vector3(delta.x * offsetWorldWithMouse, 0f, delta.y * offsetWorldWithMouse);

                    var newPos = transform.position + delta;

                    // check camera đi ra khỏi biên left - right?
                    if (newPos.x < tranLeft.position.x) newPos.x = tranLeft.position.x;
                    else if (newPos.x > tranRight.position.x) newPos.x = tranRight.position.x;

                    if (newPos.z > wallTopBottom.x) newPos.z = wallTopBottom.x;
                    else if (newPos.y < wallTopBottom.y) newPos.y = wallTopBottom.y;

                    // check camera đi ra khỏi biên top - bottom?
                    transform.position = newPos;

                    startMousePoint = currentMousePoint; // update lại để không jitter

                    CheckOpenRecenterCam();
                }
            }
        }

        // Touch Up
        if (Input.touchCount == 0)
        {
            dragging = false;
        }
    }

    private void TouchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float dist = Vector2.Distance(t0.position, t1.position);

            // Bắt đầu pinch
            if (!pinchStarted)
            {
                pinchStarted = true;
                lastPinchDistance = dist;
                return;
            }

            float delta = dist - lastPinchDistance;
            lastPinchDistance = dist;

            float newSize = mainCamera.orthographicSize - delta * zoomSpeed * 0.1f;

            //Tut zoom out
            if (isTutZoomOut)
            {
                if (newSize < mainCamera.orthographicSize)
                {
                    isTutZoomOut = false;
                    mapController.tutController.NextStep();
                }
            }

            mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);



            // Nếu đang zoom thả 1 ngón thì chuyển sang di chuyển và vị trí bắt đầu chính tại thời điểm ngón tay thứ 2 thả ra.
            if (t1.phase == TouchPhase.Ended)
            {
                dragging = true;

                startMousePoint = t0.position;
                startWorldPoint = GetWorldPosition(startMousePoint);
                offsetWorldWithMouse = -1f;
            }
            else if (t0.phase == TouchPhase.Ended)
            {
                dragging = true;
                startMousePoint = t1.position;
                startWorldPoint = GetWorldPosition(startMousePoint);
                offsetWorldWithMouse = -1f;
            }
        }
        else
        {
            pinchStarted = false;
        }
    }

    private void MoveCamWithMouse()
    {
        // Mouse Down
        if (!dragging && Input.GetMouseButtonDown(0))
        {
            startWorldPoint = GetWorldPosition(Input.mousePosition);
            startMousePoint = Input.mousePosition;
            offsetWorldWithMouse = -1f;

            dragging = true;
        }

        // Mouse Up
        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        // Mouse Hold + Move
        if (Input.GetMouseButton(0) && dragging)
        {
            Vector3 currentWorldPoint = GetWorldPosition(Input.mousePosition);
            Vector2 currentMousePoint = Input.mousePosition;

            if (offsetWorldWithMouse <= 0)
            {
                float worldDistance = Vector3.Distance(startWorldPoint, currentWorldPoint);
                float mouseDistance = Vector2.Distance(startMousePoint, currentMousePoint);

                if (worldDistance > 0.02f && mouseDistance > 0)
                {
                    offsetWorldWithMouse = worldDistance / mouseDistance;
                }
            }

            if (offsetWorldWithMouse > 0)
            {
                Vector3 delta = startMousePoint - currentMousePoint;
                delta = new Vector3(delta.x * offsetWorldWithMouse,  delta.y * offsetWorldWithMouse,0);

                var newPos = transform.position + delta;

                // check camera đi ra khỏi biên left - right?
                if (newPos.x < tranLeft.position.x)
                    newPos.x = tranLeft.position.x;
                else if (newPos.x > tranRight.position.x)
                    newPos.x = tranRight.position.x;

                // check camera đi ra khỏi biên top - bottom?
                //if (newPos.y > wallTopBottom.x) 
                //    newPos.y = wallTopBottom.x;
                //else if (newPos.x < wallTopBottom.y)
                //    newPos.y = wallTopBottom.y;

               
                transform.position = newPos;

                startMousePoint = currentMousePoint; // update lại để không jitter

                CheckOpenRecenterCam();
            }
        }

        // ====== ZOOM PC (Scroll Wheel) ======
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            float newSize = mainCamera.orthographicSize - scroll * zoomSpeed * 20f;

            //Tut zoom out
            if (isTutZoomOut)
            {
                if (newSize > mainCamera.orthographicSize)
                {
                    isTutZoomOut = false;
                    mapController.tutController.NextStep();
                }
            }

            mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
#endif
    }

    private void CheckOpenRecenterCam()
    {
        timeCount += Time.deltaTime;
        if (timeCount > timeCheck)
        {
            timeCount = 0;
            float distance = Vector3.Distance(this.transform.position, posCameraCenter);
            if (distance > mapHelper.row / 3)
            {
                // Recenter camera
                if (LGamePlay.Instance != null)
                {
                    LGamePlay.Instance.ShowClickToCenter(true);
                }
            }
            else if (distance < 2f)
            {
                if (LGamePlay.Instance != null)
                {
                    LGamePlay.Instance.ShowClickToCenter(false);
                }
            }
        }
    }

    public void RecenterCamera()
    {
        this.transform.position = posCameraCenter;
    }

    public Vector3 GetCenterMap()
    {
        return posCenterMap;
    }

    // suport
    Vector3 GetWorldPosition(Vector3 position)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(position);

        worldPos.z = 0f;

        return worldPos;

        //Ray ray = Camera.main.ScreenPointToRay(position);
        //if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundLayer))
        //{
        //    return hit.point;
        //}
        //return transform.position; // fallback
    }

    // animation
    Tween zoomTween;
    public void PlayCameraZoom(float start, float target, float time, System.Action onComplete = null, Ease ease = Ease.InOutSine)
    {
        mainCamera.orthographicSize = start;

        animZoomPlaying = true;
        zoomTween?.Kill();                        // hủy tween cũ nếu đang chạy

        // chạy tween
        zoomTween = DOTween.To(
            () => mainCamera.orthographicSize,
            v =>
            {
                mainCamera.orthographicSize = v;
            },
            target, time
        ).SetEase(ease)
         .OnComplete(() =>
         {
             animZoomPlaying = false;
             onComplete?.Invoke();
         });
    }

    public void PlayAnimEndgame(float timeDone)
    {
        PlayCameraZoom(mainCamera.orthographicSize, maxZoom - 4, timeDone);
        transform.DOMove(posCameraCenter, 0.2f);
    }

    public void ShareCamera()
    {
        //this.transform.DOShakeRotation(0.5f, new Vector3(0f, 0f, 10f), 10, 90f);
    }

    public void PlayCamareStartGame(float start, float target, float time)
    {
        PlayCameraZoom(start, target, time);
    }

    public void PlayCameraZoomoutTut(float timeShow = 0.7f)
    {
        float start = mainCamera.orthographicSize;
        float target = start * 1.5f;

        isTutZoomOut = true;
        PlayCameraZoom(start, target, timeShow);
    }

    public void MoveToTarget(Vector3 posTarget, float timeMove)
    {
        posTarget.y = this.transform.position.y;
        posTarget.z = posTarget.z - minZoom / 2 - 2.5f;
        transform.DOMove(posTarget, timeMove);
        PlayCameraZoom(mainCamera.orthographicSize, minZoom, timeMove);
    }
}
