using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class ToastIngame : MonoBehaviour
{
    public TMP_Text txtToastIngame;
    private float timeShow = 3f;

    private IEnumerator ieAutohide;

    private void OnEnable()
    {
        this.transform.localScale = Vector3.zero;
        this.transform.DOScale(Vector3.one, 0.5f);
    }

    private void OnDisable()
    {
        txtToastIngame.text = "";
    }

    public void ShowToast(string content)
    {
        txtToastIngame.text = content;
        this.gameObject.SetActive(true);

        if (ieAutohide != null)
        {
            StopCoroutine(ieAutohide);
        }

        ieAutohide = IEAutoHide();
        StartCoroutine(ieAutohide);

    }

    private IEnumerator IEAutoHide()
    {
        yield return new WaitForSeconds(timeShow);
        this.transform.DOScale(Vector3.zero, 0.3f);
        yield return new WaitForSeconds(0.3f);
        this.gameObject.SetActive(false);
    }
}
