using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

public class DoorHandleRattle : MonoBehaviour
{
    // HOW THIS WORKS:
    // Create a Grab Interactable cube.
    // Place it where you would want your door handle and disable (uncheck) its mesh renderer.
    // Add this script to it. 
    //
    // Make an empty game object and call it DoorPivot
    // Place it on the edge of the door, where you would want/expect it to pivot from.
    // Make the door object and the handle object children of that DoorPivot by dragging them inside of it.
    // You should see them nested under in the hierarchy!
    //
    // In the inspector, set Door To Rattle as your DoorPivot object.
    
    [Header("Door Reference")]
    [Tooltip("Drag the door GameObject here")]
    public Transform doorToRattle;
    
    [Header("Rattle Settings")]
    [Tooltip("How much the door shakes (in degrees)")]
    public float rattleIntensity = 2f;
    
    [Tooltip("How fast the door rattles")]
    public float rattleSpeed = 10f;
    
    [Tooltip("How long the rattling lasts")]
    public float rattleDuration = 0.5f;
    
    private Grabbable grabbable;
    private bool isRattling = false;
    private Quaternion originalRotation;
    
    void Start()
    {
        // Get the Grabbable component on this handle
        grabbable = GetComponent<Grabbable>();
        
        if (grabbable == null)
        {
            Debug.LogError("No Grabbable component found on " + gameObject.name);
            return;
        }
        
        if (doorToRattle == null)
        {
            Debug.LogError("No door assigned! Drag the door into the Inspector.");
            return;
        }
        
        // Store the door's original rotation
        originalRotation = doorToRattle.rotation;
        
        // Listen for grab events
        grabbable.WhenPointerEventRaised += OnPointerEvent;
    }
    
    void OnDestroy()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }
    }
    
    private void OnPointerEvent(PointerEvent pointerEvent)
    {
        // When the handle is grabbed, rattle the door
        if (pointerEvent.Type == PointerEventType.Select)
        {
            if (!isRattling)
            {
                StartCoroutine(RattleDoor());
            }
        }
    }
    
    private IEnumerator RattleDoor()
    {
        isRattling = true;
        float elapsedTime = 0f;
        
        // Rattle the door back and forth (left to right)
        while (elapsedTime < rattleDuration)
        {
            // Create a side-to-side shake offset using the Z-axis
            float randomShake = Mathf.Sin(elapsedTime * rattleSpeed) * rattleIntensity;
            Quaternion rattleRotation = originalRotation * Quaternion.Euler(0, 0, randomShake);
            
            doorToRattle.rotation = rattleRotation;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Snap back to original position
        doorToRattle.rotation = originalRotation;
        isRattling = false;
    }
}