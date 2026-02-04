using System.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/Items/ItemBase")]
public class ItemData : ScriptableObject
{
    public ItemType type;
    public string Name;
    public Sprite icon;
    public Sprite iconLocked;
    public string description;
    public string shortDescription;
    public int value;
    public int priceByGold;
    public bool showInInventory = true;
    public bool isUsable;

    [Space(10)]
    public Sprite iconTut;
    public string strTut;

    public virtual string GetName()
    {
        return LocalizeManager.Instance.GetLocalizeText(Name);
    }

    public virtual string GetDescription()
    {
        string desc = string.Empty;
        string format = LocalizeManager.Instance.GetLocalizeText(description);

        if (value > 0)
        {
            try
            {
                desc = string.Format(format, value);
            }
            catch
            {
                desc = format;
            }
        }
        else
        {
            desc = format;
        }

        return desc;
    }

    public virtual string GetDesTut()
    {
        string desc = string.Empty;
        string format = LocalizeManager.Instance.GetLocalizeText(strTut);

        if (value > 0)
        {
            try
            {
                desc = string.Format(format, value);
            }
            catch
            {
                desc = format;
            }
        }
        else
        {
            desc = format;
        }

        return desc;
    }
}
