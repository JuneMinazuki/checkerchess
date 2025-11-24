using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class SlidingPanel : MonoBehaviour
{
    [Header("Settings")]
    public Vector2 hiddenPosition;
    public Vector2 shownPosition;
    public float slideSpeed = 5000f;

    public bool isShown = false;
    private RectTransform rectTransform;
    private Coroutine currentAnimation;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Set initial state
        rectTransform.anchoredPosition = hiddenPosition;
        gameObject.SetActive(false);
    }

    public void TogglePanel()
    {
        if (isShown)
            ClosePanel();
        else
            OpenPanel();

        isShown = !isShown;
    }

    private void OpenPanel()
    {
        gameObject.SetActive(true);
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateSlide(shownPosition, false));
    }

    private void ClosePanel()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateSlide(hiddenPosition, true));
    }

    IEnumerator AnimateSlide(Vector2 target, bool disableOnFinish)
    {
        while (Vector2.Distance(rectTransform.anchoredPosition, target) > 0.1f)
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(rectTransform.anchoredPosition, target, slideSpeed * Time.deltaTime);
            yield return null;
        }
        
        rectTransform.anchoredPosition = target;

        // Disable the object if we finished closing
        if (disableOnFinish)
           gameObject.SetActive(false);
    }
}