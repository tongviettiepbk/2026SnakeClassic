using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupReward : BaseUI
{
    [Space(20)]
    public Button btBack;
    public ScrollRect scrollRect;
    public RectTransform groupCells;
    public CellReward cellPrefab;

    public Button btClose;
    public Button btContinue;

    private List<CellReward> cells = new List<CellReward>();

    protected override void Awake()
    {
        btBack.onClick.AddListener(Back);
        btClose.onClick.AddListener(Back);
        btContinue.onClick.AddListener(Back);
    }

    protected override void OnEnable()
    {
        base.OnEnable();


        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.isIgnoreSwipe = true;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();


        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.isIgnoreSwipe = false;
        }
    }

    public void Open(List<RewardData> data, bool isPlaySfxRewards)
    {
        try
        {
            AudioManager.Instance.PlayOpenReward();
        }
        catch
        {

        }
        gameObject.SetActive(true);

        if (data.Count > 0)
        {
            btBack.enabled = false;

            // Create more cell
            if (cells.Count < data.Count)
            {
                int cellNeedMore = data.Count - cells.Count;

                for (int i = 0; i < cellNeedMore; i++)
                {
                    CellReward cell = Instantiate(cellPrefab, groupCells);
                    cells.Add(cell);
                }
            }

            // Hide all cells
            for (int i = 0; i < cells.Count; i++)
            {
                CellReward cell = cells[i];
                cell.gameObject.SetActive(false);

                if (i < data.Count)
                {
                    cell.Load(data[i]);
                }
            }

            UIManager.Instance.StartActionEndOfFrame(() =>
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    CellReward cell = cells[i];
                    if (i < data.Count)
                    {
                        cell.canvasGroup.alpha = 0f;
                        int row = i / 5;
                        float delay = 0.1f * row;

                        UIManager.Instance.StartDelayAction(delay, () =>
                        {
                            if (gameObject.activeInHierarchy)
                            {
                                cell.canvasGroup.alpha = 1f;
                                cell.anim.Play();
                            }

                        });
                    }
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(groupCells);
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

                //UIManager.Instance.StartActionEndOfFrame(() =>
                //{
                //    Vector2 v = scrollRect.content.anchoredPosition;
                //    bool scrollable = data.Count > 25;
                //    scrollRect.enabled = scrollable;
                //    scrollRect.GetComponent<Image>().raycastTarget = scrollable;
                //    scrollRect.viewport.GetComponent<Image>().raycastTarget = scrollable;

                //    if (scrollable)
                //    {
                //        float height = scrollRect.content.sizeDelta.y;
                //        v.y = -0.5f * height;
                //    }
                //    else
                //    {
                //        v.y = -400f;
                //    }

                //    scrollRect.content.anchoredPosition = v;
                //});

                if (MapController.Instance != null)
                {
                    btBack.enabled = true;
                }
                else
                {
                    UIManager.Instance.StartDelayAction(0.5f, () =>
                    {
                        btBack.enabled = true;

                    });
                }

            });

            if (isPlaySfxRewards)
            {
                //AudioManager.Instance.PlaySfx(AudioTag.SFX_REWARDS);
            }

            gameObject.SetActive(true);
        }
    }


}
