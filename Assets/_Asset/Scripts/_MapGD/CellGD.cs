using TMPro;
using UnityEngine;

public class CellGD : MonoBehaviour
{
    public int indexInmap;
    public TypeNodeValueInMap typeNode = TypeNodeValueInMap.EMPTY;
    public TMP_Text txtGecko;

    public int idCell;
    public int valueCell;

    [Space(20)]
    public GameObject objHead;
    public bool isHead = false;
    public GameObject objSecond;

    private GenMapFollowInput genMapController;

    void Start()
    {

    }

    void Update()
    {

    }

    public void Init(GenMapFollowInput gen)
    {
        txtGecko.text = "";
        this.genMapController = gen;
        objHead.gameObject.SetActive(false);
        isHead = false;
    }

    public void Reset()
    {
        txtGecko.text = "";
        objHead.gameObject.SetActive(false);
        objSecond.gameObject.SetActive(false);
        isHead = false;
    }

    public void ChangeMaterialColor(Material mat)
    {
        this.GetComponent<MeshRenderer>().material = mat;
    }

    public void SetNameGecko(int idName, int idColor)
    {
        typeNode = TypeNodeValueInMap.NODE_GECKO;

        this.idCell = idName;
        txtGecko.text = idName.ToString();
        this.valueCell = idColor;

        ChangeMaterialColor(this.genMapController.listMaterialColor[idColor]);
    }

    public void Remove()
    {
        typeNode = TypeNodeValueInMap.EMPTY;
        this.idCell = -1;
        this.valueCell = -1;

        txtGecko.text = "";
        objHead.gameObject.SetActive(false);
        objSecond.gameObject.SetActive(false);
        isHead = false;

        ChangeMaterialColor(this.genMapController.listMaterialColor[this.genMapController.listMaterialColor.Count - 1]);
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

    public void SetBrick(int hp)
    {
        this.valueCell = hp;

        txtGecko.text = "B";
        typeNode = TypeNodeValueInMap.OBSTACLE_BRICK;

    }

    public void SetBarrier(int open)
    {
        this.valueCell = open;

        txtGecko.text = "L";
        typeNode = TypeNodeValueInMap.OBSTALCE_BARRIER;
    }

    public void SetStop()
    {
        this.valueCell = -1;
        txtGecko.text = "S";
        typeNode = TypeNodeValueInMap.OBSTALCE_STOP;
    }

    public void SetHoleOut(int colorHole)
    {
        this.valueCell = colorHole;

        txtGecko.text = "H";
        typeNode = TypeNodeValueInMap.OBSTALCE_EXIT_HOLE;
    }


}
