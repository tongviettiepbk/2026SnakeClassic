using UnityEngine;
using System.Collections;

public enum RotateAxis
{
    X,
    Y,
    Z
}

public class AutoRotate : MonoBehaviour
{
    public RotateAxis axis;
    public float speed = 100f;
    public bool isClockwise;

    private Vector3 angle;

    private void Update()
    {
        Vector3 angle = transform.eulerAngles;
        float angleRotated = isClockwise ? Time.deltaTime * -speed : Time.deltaTime * speed;

        if (axis == RotateAxis.X)
        {
            angle.x += angleRotated;
        }
        else if (axis == RotateAxis.Y)
        {
            angle.y += angleRotated;

        }
        else if (axis == RotateAxis.Z)
        {
            angle.z += angleRotated;
        }

        transform.eulerAngles = angle;
    }
}
