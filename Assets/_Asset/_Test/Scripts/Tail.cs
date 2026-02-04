using System.Net.NetworkInformation;
using UnityEngine;

public class Tail : MonoBehaviour
{
    public enum Directions
    {
        UP,
        DOWN, 
        LEFT, 
        RIGHT
    }

    public GameObject tailEnd;
    public GameObject straight;
    public GameObject cornerUR;
    public GameObject cornerUL;

    public Directions currentDirection;

    public void SetCorrectPiece(Directions childDir)
    {
        if (currentDirection == Directions.UP) { 

           if(childDir == Directions.UP){
                SetGrafic("staight");
           }
           else if (childDir == Directions.LEFT){
                SetGrafic("upRight");
           }
           else if (childDir == Directions.RIGHT){
                SetGrafic("upLeft");
           }
        }

        if (currentDirection == Directions.DOWN)
        {
            if (childDir == Directions.DOWN)
            {
                SetGrafic("staight");
            }
            else if (childDir == Directions.LEFT)
            {
                SetGrafic("upLeft");
            }
            else if (childDir == Directions.RIGHT)
            {
                SetGrafic("upRight");
            }

        }

        if (currentDirection == Directions.LEFT)
        {
            if (childDir == Directions.LEFT)
            {
                SetGrafic("staight");
            }
            else if (childDir == Directions.UP)
            {
                SetGrafic("upLeft");
            }
            else if (childDir == Directions.DOWN)
            {
                SetGrafic("upRight");
            }
        }

        if (currentDirection == Directions.RIGHT)
        {
            if (childDir == Directions.RIGHT)
            {
                SetGrafic("staight");
            }
            else if (childDir == Directions.UP)
            {
                SetGrafic("upLeft");
            }
            else if (childDir == Directions.DOWN)
            {
                SetGrafic("upRight");
            }
        }
    }

    private void SetGrafic(string pieceName)
    {
        switch (pieceName)
        {
            case "end":
                DeactiveTailAll();
                tailEnd.SetActive(true);
           
                break;

            case "staight":
                DeactiveTailAll();
                straight.SetActive(true);
                break;

            case "upRigt":
                DeactiveTailAll();
                cornerUR.SetActive(true);
                break;

            case "upLeft":
                DeactiveTailAll();
                cornerUL.SetActive(true);
                break;
        }
    }

    private void DeactiveTailAll()
    {
        tailEnd.SetActive(false);
        straight.SetActive(false);
        cornerUL.SetActive(false);
        cornerUR.SetActive(false);
    }

    public Directions GetDirection()
    {
        return currentDirection;
    }

    public void SetDirection()
    {
        if (transform.forward == Vector3.forward)
        {
            currentDirection = Directions.UP;
        }

        if (transform.forward == Vector3.back)
        {
            currentDirection= Directions.DOWN;
        }

        if (transform.forward == Vector3.left)
        {
            currentDirection = Directions.LEFT;
        }

        if (transform.forward == Vector3.right)
        {
            currentDirection = Directions.RIGHT;
        }
    }
}
