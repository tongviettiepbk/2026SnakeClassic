using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class StaticItemData : List<ItemData>
{
    public StaticItemData()
    {
        List<ItemData> data = Resources.LoadAll<ItemData>("Scriptable Objects/Items").ToList();
        data = data.OrderBy(x => x.type).ToList();
        AddRange(data);
    }

    public ItemData GetData(int id)
    {
        for (int i = 0; i < this.Count; i++)
        {
            ItemData data = this[i];

            if ((int)data.type == id)
            {
                return data;
            }
        }

        //DebugCustom.Log("[StaticItemData] Not found=" + id);
        return null;
    }

    public ItemData GetData(ItemType type)
    {
        return GetData((int)type);
    }
}
