using UnityEngine;

public enum GeckoPartState
{
    IDLE = 0,
    MOVE_TARGET_POS = 1,
    MOVE_TARGET_TRANSFORM = 2,
}

public class GeckoPart : MonoBehaviour
{
    public Gecko owner;
    public bool isHead = false;
    public bool isTailEnd = false;
    public Transform following;

    private GeckoPartState stateCurrent = GeckoPartState.IDLE;

    private Vector3 posTarget = Vector3.zero;
    public int index;
    public int indexInmap = 0;

    public void Init(Gecko gecko, int indexInGecko, bool isHeadTemp = false, bool isTailEnd = false)
    {
        this.isHead = isHeadTemp;
        this.isTailEnd = isTailEnd;
        owner = gecko;

        this.index = indexInGecko;
    }

    public void FixedUpdate()
    {
        MoveToTargetPos();
        MoveToFollowing();
    }

    public void SetInfo(Transform following)
    {
        this.following = following;
    }

    public void StartMoveToTargetPos()
    {
        if (following == null)
        {
            DebugCustom.ShowDebugColorRed(index + " following is null");
            return;
        }

        posTarget = following.position;
        stateCurrent = GeckoPartState.MOVE_TARGET_POS;
    }

    public void EndMoveToTargetPos()
    {
        transform.position = posTarget;
        stateCurrent = GeckoPartState.IDLE;
    }

    private void MoveToTargetPos()
    {
        if (stateCurrent != GeckoPartState.MOVE_TARGET_POS)
        {
            return;
        }

        if (following == null)
            return;

        Vector3 dir = posTarget - transform.position;

        float distance = dir.magnitude;

        if (distance > 0.2f)
        {
            dir.Normalize();
            float speedMove = GameConfig.speed * Time.fixedDeltaTime;
            float moveDistance = Mathf.Min(speedMove, distance);
            transform.position += dir * moveDistance;

        }

    }

    public void StartMoveToFollowing()
    {
        if (following == null)
        {
            DebugCustom.ShowDebugColorRed(index + " following is null");
            return;
        }
        stateCurrent = GeckoPartState.MOVE_TARGET_TRANSFORM;
    }

    private void MoveToFollowing()
    {
        if (stateCurrent != GeckoPartState.MOVE_TARGET_TRANSFORM)
        {
            return;
        }

        if (following == null)
            return;

        Vector3 dir = following.transform.position - transform.position;

        float distance = dir.magnitude;

        if (distance > 0.2f)
        {
            dir.Normalize();
            float speedMove = GameConfig.speed * Time.fixedDeltaTime;
            float moveDistance = Mathf.Min(speedMove, distance);
            transform.position += dir * moveDistance;
        }
    }
}
