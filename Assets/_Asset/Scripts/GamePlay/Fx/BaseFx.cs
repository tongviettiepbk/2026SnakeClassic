using UnityEngine;

public class BaseFx : MonoBehaviour
{
    public TypeObject typeObj;

    protected virtual void OnEnable()
    {
        
    }

    protected virtual void OnDisable()
    {
        try
        {
            this.transform.SetParent(null);
        }
        catch
        {

        }
        
        if(PoolingController.Instance != null)
        {
            PoolingController.Instance.StoreGameObj(typeObj,this.gameObject);
        }
    }
}
