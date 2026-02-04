using System;
using System.Collections.Generic;


[Serializable]
public class NodeMap
{
    public TypeNodeValueInMap typeNodeValueInMap { get; private set; } = TypeNodeValueInMap.EMPTY;
    public int indexNodeInMap;

    private List<TypeNodeValueInMap> listValueAdd = new List<TypeNodeValueInMap>();

    public NodeMap(int indexNode, TypeNodeValueInMap typeNodeValue)
    {
        typeNodeValueInMap = typeNodeValue;
        indexNodeInMap = indexNode;
    }

    public void SetValueNote(TypeNodeValueInMap typeNode)
    {
        this.typeNodeValueInMap = typeNode;
    }

    public void UpdateNote(TypeNodeValueInMap typeNode)
    {
        // Nếu là node stop thì có thể thêm giá trị của node
        if (typeNodeValueInMap == TypeNodeValueInMap.OBSTALCE_STOP)
        {
            if (typeNode == TypeNodeValueInMap.NODE_GECKO)
            {
                listValueAdd.Add(typeNode);
                DebugCustom.ShowDebugColorRed("Add them gia tri cua node stop co chua them thanh phan");
            }
            else if (typeNode == TypeNodeValueInMap.EMPTY)
            {

                listValueAdd.Clear();
            }
        }
        else if (typeNodeValueInMap == TypeNodeValueInMap.EMPTY || typeNodeValueInMap == TypeNodeValueInMap.NODE_GECKO
            || typeNodeValueInMap == TypeNodeValueInMap.OBSTACLE_BRICK)
        {
            this.typeNodeValueInMap = typeNode;
        }

    }

    public List<TypeNodeValueInMap> GetValueNoteBonus()
    {
        return listValueAdd;
    }

    public void SetValueNodeBonus(TypeNodeValueInMap typeAddBonus)
    {
        this.listValueAdd.Add(typeAddBonus);
    }

    public void RemoveBrick()
    {
        if(typeNodeValueInMap == TypeNodeValueInMap.OBSTACLE_BRICK)
        {
            typeNodeValueInMap = TypeNodeValueInMap.EMPTY;
        }
        else
        {
            DebugCustom.ShowDebugColorRed("Error");
        }
    }

    public void RemoveBarrier()
    {
        if (typeNodeValueInMap == TypeNodeValueInMap.OBSTALCE_BARRIER)
        {
            typeNodeValueInMap = TypeNodeValueInMap.EMPTY;
        }
        else
        {
            DebugCustom.ShowDebugColorRed("Error");
        }
    }
}
