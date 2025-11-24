using UnityEngine;

public class MenuButton : MonoBehaviour
{
    [Header("UI")]
    public GameObject mainMenuUI;
    public GameObject midGameUI;

    public void OnStartButton()
    {
        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);
        if (midGameUI != null)
            midGameUI.SetActive(true);
    }

    public void OnShowMenuButton()
    {
        if (mainMenuUI != null)
            mainMenuUI.SetActive(true);
        if (midGameUI != null)
            midGameUI.SetActive(false);
    }
}
