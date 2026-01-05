using UnityEngine;

public class DonateButton : MonoBehaviour
{
    [SerializeField] private string donateUrl = "https://qr.alipay.com/50z16100apzurumvctuila5";

    public void OnDonateClick()
    {
        if (string.IsNullOrEmpty(donateUrl)) return;
        Application.OpenURL(donateUrl);
    }
}