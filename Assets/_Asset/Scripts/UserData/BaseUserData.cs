using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

public class BaseUserData
{
    [JsonIgnore] public bool isDataChanged;

    protected virtual string GetDataKey()
    {
        return string.Empty;
    }

    public virtual bool Save(bool forceSave)
    {
        if (forceSave)
        {
            isDataChanged = true;
        }

        if (isDataChanged)
        {
            PlayerPrefs.SetString(GetDataKey(), JsonConvert.SerializeObject(this));
            isDataChanged = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    public virtual void ValidateData() { }

    public virtual void RefreshNewDay() { isDataChanged = true; }

    public virtual void RefreshNewWeek() { isDataChanged = true; }

    public virtual void RefreshNewMonth() { isDataChanged = true; }

    public virtual void RefreshNewYear() { isDataChanged = true; }

    public virtual bool IsNotify() { return false; }
}
