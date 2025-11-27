using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MuteBackground : MonoBehaviour
{
    [Header("Selectors")]
    [SerializeField] private SlidingSelector soundSelector;

    private AudioSource audioSource;

    // Get the reference to the AudioSource
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Subscribe to the signal
    void OnEnable()
    {
        if (soundSelector != null)
            soundSelector.OnSelectionChanged += HandleSelection;
    }

    // Unsubscribe to the signal
    void OnDisable()
    {
        if (soundSelector != null)
            soundSelector.OnSelectionChanged -= HandleSelection;
    }

    void HandleSelection(int index)
    {
        bool isSoundOn = soundSelector.selectedIndex == 0;

        if (audioSource != null)
            audioSource.mute = !isSoundOn;
    }
}