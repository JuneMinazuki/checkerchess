using UnityEngine;
using System;

public class SlidingSelector : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform highlighter;
    public RectTransform[] options;
    public int defaultOption;

    [Header("Settings")]
    public float slideSpeed = 10f;

    public int selectedIndex = 0;
    private RectTransform currentTargetButton;
    public event Action<int> OnSelectionChanged;

    void Start()
    {
        // Initialize to the first option and set position
        if(options.Length > 0)
        {
            //Force Unity to calculate the layouts
            Canvas.ForceUpdateCanvases();

            SetOption(defaultOption);
            if (currentTargetButton != null)
                highlighter.position = currentTargetButton.position;
        }
    }

    void Update()
    {
        if (currentTargetButton == null) return;

        // Smoothly move the highlighter to the target position
        highlighter.position = Vector3.Lerp(highlighter.position, currentTargetButton.position, Time.deltaTime * slideSpeed);
    }

    public void OnButtonClick(int index)
    {
        SetOption(index);
    }

    private void SetOption(int index)
    {
        selectedIndex = index;
        currentTargetButton = options[index];

        // Send signal to other game object
        OnSelectionChanged?.Invoke(index);
    }
}
