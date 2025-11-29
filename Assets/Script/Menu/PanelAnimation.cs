using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class SlidingPanel : MonoBehaviour
{
    [Header("Settings")]
    public Vector2 hiddenPosition;
    public Vector2 shownPosition;
    public float slideSpeed = 5000f;
    public CanvasGroup backgroundDimmer;
    public float dimSpeed = 2f;

    public bool isShown = false;
    private RectTransform rectTransform;
    private Coroutine currentAnimation;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Set initial state
        rectTransform.anchoredPosition = hiddenPosition;
        gameObject.SetActive(false);
        if (backgroundDimmer != null) 
        {
            backgroundDimmer.alpha = 0f;
            backgroundDimmer.blocksRaycasts = false;
        }
    }

    void OnDisable()
    {
        // Reset panel state
        isShown = false; 
        currentAnimation = null;
        
        // Snap positions just in case it was disabled mid-animation
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        rectTransform.anchoredPosition = hiddenPosition;
        
        if (backgroundDimmer != null)
        {
            backgroundDimmer.alpha = 0f;
            backgroundDimmer.blocksRaycasts = false;
        }
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

        currentAnimation = StartCoroutine(AnimateSlide(shownPosition, 1f, false));
    }

    private void ClosePanel()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateSlide(hiddenPosition, 0f, true));
    }

    IEnumerator AnimateSlide(Vector2 targetPos, float targetAlpha, bool disableOnFinish)
    {
        while (Vector2.Distance(rectTransform.anchoredPosition, targetPos) > 0.1f)
        {
            // Move the panel
            rectTransform.anchoredPosition = Vector2.MoveTowards(rectTransform.anchoredPosition, targetPos, slideSpeed * Time.deltaTime);

            // Fade the background
            if (backgroundDimmer != null)
                backgroundDimmer.alpha = Mathf.MoveTowards(backgroundDimmer.alpha, targetAlpha, dimSpeed * Time.deltaTime);
            
            yield return null;
        }
        
        // Snap final values to be clean
        rectTransform.anchoredPosition = targetPos;
        if (backgroundDimmer != null)
        {
            backgroundDimmer.alpha = targetAlpha;
            backgroundDimmer.blocksRaycasts = targetAlpha > 0.5f;
        }

        // Disable the object if we finished closing
        if (disableOnFinish)
           gameObject.SetActive(false);
    }
}