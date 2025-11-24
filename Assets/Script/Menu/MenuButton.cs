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
}
