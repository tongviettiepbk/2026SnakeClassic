using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PopUpBundle : MonoBehaviour
{
    public List<BoxBundle> boxs;

    public List<GameObject> dotsBlack;
    public List<GameObject> dotsWhite;
    private int index;

    private void OnEnable()
    {
        index = 0;
        Reload();
    }

    public void Reload()
    {
        CheckActiveBundle();
        for (int i = 0; i < boxs.Count; i++)
        {
            if (boxs[i] != null)
                boxs[i].gameObject.SetActive(false);
        }

        if (boxs == null || boxs.Count == 0)
        {
            return;
        }

        if (index < 0)
        {
            index = 0;
        }

        if (index >= boxs.Count)
        {
            index = boxs.Count - 1;
        }

        var box = boxs[index];
        box.Initialize(index);
        CheckDot();
    }

    public void ShowNext()
    {
        index = (index + 1) % boxs.Count;
        Reload();
    }

    public void ShowPrev()
    {
        index = (index - 1 + boxs.Count) % boxs.Count;
        Reload();
    }

    private void CheckDot()
    {
        int count = boxs.Count;
        for (int i = 0; i < dotsBlack.Count; i++)
        {
            bool isActive = (i < count);
            dotsBlack[i].SetActive(isActive);
        }

        if (dotsWhite == null || dotsWhite.Count == 0)
        {
            return;
        }

        for (int i = 0; i < dotsWhite.Count; i++)
        {
            bool isActive = (i != index);
            dotsWhite[i].SetActive(isActive);
        }
    }

    public void CheckActiveBundle()
    {
        if (boxs == null || boxs.Count == 0)
            return;

        var uShop = GameData.userData.shops;
        if (uShop == null)
            return;

        for (int i = boxs.Count - 1; i >= 0; i--)
        {
            var box = boxs[i];
            if (box == null)
            {
                continue;
            }

            if (uShop.IsPurchased((int)box.shopPackageData.id))
            {
                box.gameObject.SetActive(false);
                boxs.RemoveAt(i);
            }
        }

        if (boxs.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        index = Mathf.Clamp(index, 0, boxs.Count - 1);
    }
}

