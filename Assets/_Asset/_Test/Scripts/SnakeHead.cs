using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SnakeHead : MonoBehaviour
{
    [SerializeField]
    private float stepSize = 1.0f;
    [SerializeField]
    private float stepSpeed = 1.0f;

    public GameObject tailPrefab;
    public List<GameObject> tailList = new List<GameObject>();


    private bool alive = true;

    private Vector3 direction = Vector3.zero;
    private Vector3 left = new Vector3(0, -90, 0);
    private Vector3 right = new Vector3(0, 90, 0);
    private Vector3 nextDirection = new Vector3();

    private void Start()
    {
        AddTailPiece();
        StartCoroutine(MakeAStep());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddTailPiece();
        }
    }

    IEnumerator MakeAStep()
    {
        float timeToWait = 0.1f;
        while (alive)
        {
            timeToWait += Time.deltaTime;
            if (timeToWait >= stepSpeed)
            {
                transform.localRotation = Quaternion.Euler(nextDirection);

                Vector3 lastHeadPosition = transform.position;
                Vector3 lastHeadRotation = transform.rotation.eulerAngles;

                //lastHeadPosition.x = 0;
                //lastHeadRotation.z = 0;

                //move head
                transform.position += transform.forward * stepSize;

                //move tailpiece
                if (tailList.Count > 0)
                {
                    Vector3 lastTailPos = lastHeadPosition;
                    Quaternion lastTailRot = Quaternion.Euler(lastHeadRotation);

                    for (int i = 0; i < tailList.Count; i++)
                    {
                        Vector3 myLastPos = tailList[i].transform.position;
                        Quaternion myLastRot = tailList[i].transform.rotation;

                        tailList[i].transform.position = lastTailPos;
                        tailList[i].transform.rotation = lastTailRot;

                        tailList[i].GetComponent<Tail>().SetDirection();

                        lastTailPos = myLastPos;
                        lastTailRot = myLastRot;
                    }

                    UpdateAllTailPieces();

                    timeToWait = 0;
                }
            }

            yield return null;
        }
    }

    private void AddTailPiece()
    {
        if (tailList.Count > 0)
        {
            GameObject newTail = Instantiate(tailPrefab,
                                            tailList[tailList.Count - 1].transform.position - (tailList[tailList.Count - 1].transform.forward * stepSize).normalized,
                                             tailList[tailList.Count - 1].transform.rotation);
            tailList.Add(newTail);
        }
        else
        {
            GameObject newTail = Instantiate(tailPrefab,
                                            transform.position - (transform.forward * stepSize).normalized,
                                            transform.rotation);
            tailList.Add(newTail);
        }

        UpdateAllTailPieces();
    }

    private void UpdateAllTailPieces()
    {
        // update all grafics
        for (int i = 0; i < tailList.Count; i++)
        {
            if (i + 1 < tailList.Count)
            {
                Tail currentTail = tailList[i].GetComponent<Tail>();
                Tail childTail = tailList[i + 1].GetComponent<Tail>();
                currentTail.SetCorrectPiece(childTail.GetDirection());
            }
            //tail end

            if(i == tailList.Count - 1)
            {
                //Tail currentTail = tailList[i].GetComponent<Tail>();
                //currentTail.SetGrafic("end");
            }
        }
    }

    public void MoveLeft()
    {
        nextDirection = transform.localEulerAngles + left;
    }

    public void MoveRight()
    {
        nextDirection = transform.localEulerAngles + right;
    }
}
