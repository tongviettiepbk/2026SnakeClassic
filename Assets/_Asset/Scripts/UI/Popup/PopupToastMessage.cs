using TMPro;
using UnityEngine.UI;

public enum ToastMessageType
{
    DEFAULT_ERROR,
    NETWORK_CONNECTION_FAILED,
    INSUFFICIENT_RESOURCES,
    SUSPENDED_ACCOUNT,
    SUSPENDED_ACCOUNT_AND_BLOCK_DEVICE,
    UPDATE_NEW_VERSION,
    SERVER_MAINTANCE,

    PURCHASE_SUCCESS,
    COME_BACK_TOMORROW,
    COMING_SOON,
    HEART_FULL,
}

public class PopupToastMessage : BaseUI
{
    public TMP_Text textContent;

    private bool isAnimating = false;

    public void Show(string content)
    {
        if (isAnimating)
        {
            return;
        }

        isAnimating = true;
        textContent.text = content;
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public override void EndClose()
    {
        textContent.text = string.Empty;
        isAnimating = false;
    }
}
