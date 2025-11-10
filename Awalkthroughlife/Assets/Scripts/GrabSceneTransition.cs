using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus.Interaction;

public class GrabSceneTransition : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad = "NextScene";
    [SerializeField] private float transitionDelay = 0.5f;
    
    [Header("Optional Transition Effects")]
    [SerializeField] private bool fadeTransition = true;
    [SerializeField] private float fadeTime = 1f;
    
    private Grabbable grabbable;
    private bool hasTriggered = false;
    
    void Start()
    {
        // Get the Grabbable component
        grabbable = GetComponent<Grabbable>();
        
        if (grabbable == null)
        {
            Debug.LogError("GrabSceneTransition: No Grabbable component found on " + gameObject.name);
            return;
        }
        
        // Subscribe to grab events
        grabbable.WhenPointerEventRaised += OnGrabEvent;
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= OnGrabEvent;
        }
    }
    
    private void OnGrabEvent(PointerEvent pointerEvent)
    {
        // Check if this is a grab event (not release)
        if (pointerEvent.Type == PointerEventType.Select && !hasTriggered)
        {
            hasTriggered = true;
            Debug.Log("Object grabbed! Transitioning to scene: " + sceneToLoad);
            
            // Validate scene name before loading
            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogError("Scene name is empty! Please set the scene name in the inspector.");
                return;
            }
            
            if (transitionDelay > 0)
            {
                Invoke(nameof(LoadScene), transitionDelay);
            }
            else
            {
                LoadScene();
            }
        }
    }
    
    private void LoadScene()
    {
        // Disable GrabAndLocate components to prevent null reference errors
        var grabAndLocates = FindObjectsOfType<Meta.XR.MRUtilityKit.BuildingBlocks.GrabAndLocate>();
        foreach (var component in grabAndLocates)
        {
            if (component != null)
            {
                component.enabled = false;
            }
        }
        
        if (fadeTransition)
        {
            StartCoroutine(FadeAndLoadScene());
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
    
    private System.Collections.IEnumerator FadeAndLoadScene()
    {
        // You can implement a fade effect here
        // For now, just wait for fade time then load
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(sceneToLoad);
    }
}