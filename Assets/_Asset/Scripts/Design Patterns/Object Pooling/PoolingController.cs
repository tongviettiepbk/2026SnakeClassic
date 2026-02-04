using System.Collections.Generic;
using UnityEngine;

public enum TypeObject
{
    FX_STUN_GECKO = 0,
    FX_ICE_BREAK =1,
    FX_ICE_CRACKED =2,
    FX_HEART_BREAK =3,
}

public class PoolingController : Singleton<PoolingController>
{
    public Dictionary<TypeObject, ObjectPooling<GameObject>> poolGameObject = new Dictionary<TypeObject, ObjectPooling<GameObject>>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    #region Object

    public GameObject GetGameObjectFromPool(TypeObject idObject, GameObject prefab)
    {
        if (!poolGameObject.ContainsKey(idObject))
        {
            poolGameObject[idObject] = new ObjectPooling<GameObject>();
        }

        GameObject obj = poolGameObject[idObject].New();

        if (obj == null)
        {
            obj = Instantiate(prefab);
        }

        return obj;
    }

    public void StoreGameObj(TypeObject id, GameObject obj)
    {
        if (poolGameObject.ContainsKey(id))
        {
            if (!poolGameObject[id].Contains(obj))
                poolGameObject[id].Store(obj);

            return;
        }

        DebugCustom.LogError("Gameobj type not found=" + id);
    }

    #endregion
}