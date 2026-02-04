using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TypeIcon
{
    Normal = 0,
    HardMode = 1,
}

public class ImageUtils : MonoBehaviour
{
    public Image imageIcon;
    public List<Sprite> listSpriteChange =  new List<Sprite>();

    private void OnEnable()
    {
        SimpleEventManager.Instance.Register(EventIDSimple.hardModeStart, ChangeSpriteHard);
    }

    private void OnDisable()
    {
        SimpleEventManager.Instance.Unregister(EventIDSimple.hardModeStart, ChangeSpriteHard);
    }

    private void Start()
    {
        imageIcon = GetComponent<Image>();
    }

    private void ChangeSpriteHard(object objTemp)
    {
        if (listSpriteChange == null || listSpriteChange.Count < 2)
        {
            return;
        }

        imageIcon.sprite = listSpriteChange[(int)TypeIcon.HardMode];
    }


}
