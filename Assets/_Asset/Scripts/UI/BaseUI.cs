using System;
using UnityEngine;

public class BaseUI : MonoBehaviour
{
    [Space(20)]
    [Header("Animator Popup info")]
    public Animator animatorPopup;
    //public AnimatorController animatorPopup2;
    public string strAnimOpen = "Open";
    public string strAnimClose = "Close";
    public float timeClose = 0.5f;
    public AudioClip sfxPopup;
    public bool isOverlay { get; set; }
    public bool isLoadFromResources { get; set; } = false;
    public bool isBackable { get; set; } = true;
    public bool isPoolingWhenClose { get; set; } = false;

    protected virtual void Awake() { }

    protected virtual void OnEnable()
    {
        if (sfxPopup == null)
        {
            AudioManager.Instance.PlayOpenPopup();
        }
        else
        {
            AudioManager.Instance.PlaySfx(sfxPopup);
        }
    }

    protected virtual void OnDisable()
    {
    }

    public virtual void Initialize() { }

    public virtual void ListenOnClose(object obj)
    {
        try
        {
            if (gameObject.activeSelf)
            {
                Close();
            }
        }
        catch { }
    }


    public virtual void Close()
    {
        AudioManager.Instance.ResumeMusic();
        if (this != null && this.gameObject != null)
        {
            gameObject.SetActive(false);
            if (isLoadFromResources)
            {
                UIManager.Instance.HideUI(this);
            }
            EndClose();
        }
    }


    public virtual void Back()
    {

        if (IsBackable())
        {
            PlayCloseAnim(() =>
            {
                Close();
            });

        }
    }

    public virtual bool IsBackable()
    {
        return isBackable && gameObject.activeSelf;
    }

    public virtual bool CheckNotify()
    {
        return false;
    }

    public virtual void EndClose() { }

    #region Anim 
    public void PlayOpenAnim()
    {
        PlayAnimAnimator(strAnimOpen);
    }

    public void PlayCloseAnim(Action actionClose)
    {
        AudioManager.Instance.PlayClosePopup();
        PlayAnimAnimator(strAnimClose);
        this.StartDelayAction(timeClose, () =>
        {
            if (actionClose != null)
                actionClose();
        });
    }

    private void PlayAnimAnimator(string aniTemp)
    {
        if (animatorPopup == null)
            return;

        animatorPopup.SetTrigger(aniTemp);
    }
    #endregion
}
