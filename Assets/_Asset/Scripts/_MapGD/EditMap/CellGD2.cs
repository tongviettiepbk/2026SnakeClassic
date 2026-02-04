using System.Collections.Generic;
using UnityEngine;

public class CellGD2 : MonoBehaviour
{
    [Space(20)]
    public GameObject objHead;
    public bool isHead = false;
    public GameObject objSecond;

    [Space(20)]
    public List<Material> listMaterialColor;



    public void Reset()
    {
        objHead.gameObject.SetActive(false);
        objSecond.gameObject.SetActive(false);
        isHead = false;
    }

    public void ChoseHead()
    {
        isHead = true;
        objHead.gameObject.SetActive(true);
    }

    public void ChoseSecond()
    {
        objSecond.gameObject.SetActive(true);
    }

    public void ChangeMaterialColor(int idColor)
    {
        var mat = listMaterialColor[idColor];
        this.GetComponent<MeshRenderer>().material = mat;
    }
}
