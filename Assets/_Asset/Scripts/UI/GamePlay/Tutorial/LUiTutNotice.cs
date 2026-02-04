using UnityEngine;

public class LUiTutNotice : BaseUI
{
    public GameObject objTutMove;
    public GameObject objTutZoomOut;

    public void OpenTutMove()
    {
        objTutMove.SetActive(true);
        objTutZoomOut.SetActive(false);
    }

    public void OpenTutZoomOut()
    {
        objTutMove.SetActive(false);
        objTutZoomOut.SetActive(true);
    }
}
