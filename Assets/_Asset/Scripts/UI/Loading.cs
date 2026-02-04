using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Image imgPrecess;

    private string nameSceneNext;

    private float countTime = 0;
    private float timeFake = 2;
    private bool isCheck = false;

    private void Start()
    {
        nameSceneNext = GameConfig.sceneNext;

        if (!MediationAds.Instance.isHackAds)
        {
            MediationAds.Instance.HideBannerAd();
            MediationAds.Instance.ShowMRec();
        }

        isCheck = false;
        countTime = 0;
    }

    private void Update()
    {
        if (countTime < timeFake && !isCheck)
        {
            countTime += Time.deltaTime;
            imgPrecess.fillAmount = countTime / timeFake;


            if (countTime > timeFake)
            {
                isCheck = true;
                SceneManager.LoadScene(nameSceneNext);
            }
        }
    }
}
