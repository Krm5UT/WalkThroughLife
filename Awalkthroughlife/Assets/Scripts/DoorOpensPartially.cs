using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

public class DoorOpensPartially : MonoBehaviour
{
    // HOW THIS WORKS:
    // Create a Grab Interactable cube.
    // Place it where you would want your door handle and disable (uncheck) its mesh renderer.
    // Add this script to it. 
    //
    // The script will automatically detect if the door handle is a child of a DoorPivot object.
    // If it finds a DoorPivot parent, it will use that as the rotation point.
    // Otherwise, it will rotate the door you assign directly.
    //
    // RECOMMENDED SETUP:
    // Make an empty game object and call it DoorPivot
    // Place it on the edge of the door, where you would want/expect it to pivot from.
    // Make the door object and the handle object children of that DoorPivot by dragging them inside of it.
    
    [Header("Door Reference")]
    [Tooltip("Drag the door GameObject here (can be the DoorPivot or the door itself)")]
    public Transform doorToOpen;
    
    [Header("Door Settings")]
    [Tooltip("How far the door opens in degrees")]
    public float openAngle = 10f;
    
    [Tooltip("How fast the door opens")]
    public float openSpeed = 2f;
    
    [Tooltip("Time it takes for door to fully open")]
    public float doorTime = 1f;
    
    [Header("Pivot Detection")]
    [Tooltip("Automatically find DoorPivot parent (recommended)")]
    public bool autoDetectPivot = true;
    
    private Grabbable grabbable;
    private bool isOpen = false;
    private bool isOpening = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Transform doorPivot;
    
    void Start()
    {
        // Get the Grabbable component on this handle
        grabbable = GetComponent<Grabbable>();
        
        if (grabbable == null)
        {
            Debug.LogError("No Grabbable component found on " + gameObject.name);
            return;
        }
        
        // Auto-detect pivot point
        if (autoDetectPivot)
        {
            DetectDoorPivot();
        }
        else
        {
            doorPivot = doorToOpen;
        }
        
        if (doorPivot == null)
        {
            Debug.LogError("No door pivot found! Either assign a door or enable auto-detect.");
            return;
        }
        
        // Set up door rotations
        closedRotation = doorPivot.rotation;
        openRotation = Quaternion.Euler(doorPivot.eulerAngles + new Vector3(0, openAngle, 0));
        
        // Listen for grab events
        grabbable.WhenPointerEventRaised += OnPointerEvent;
        
        Debug.Log("Door pivot set to: " + doorPivot.name);
    }
    
    void OnDestroy()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }
    }
    
    private void DetectDoorPivot()
    {
        // First, check if doorToOpen is already assigned
        if (doorToOpen != null)
        {
            doorPivot = doorToOpen;
            Debug.Log("Using assigned door: " + doorPivot.name);
            return;
        }
        
        // Look for a parent named "DoorPivot"
        Transform current = transform.parent;
        while (current != null)
        {
            if (current.name.Contains("DoorPivot") || current.name.Contains("Door Pivot"))
            {
                doorPivot = current;
                Debug.Log("Auto-detected DoorPivot: " + doorPivot.name);
                return;
            }
            current = current.parent;
        }
        
        // If no DoorPivot found, use immediate parent
        if (transform.parent != null)
        {
            doorPivot = transform.parent;
            Debug.Log("No DoorPivot found, using parent: " + doorPivot.name);
        }
        else
        {
            Debug.LogError("No parent found! This handle needs to be a child of a DoorPivot object.");
        }
    }
    
    private void OnPointerEvent(PointerEvent pointerEvent)
    {
        // When the handle is grabbed, open the door
        if (pointerEvent.Type == PointerEventType.Select)
        {
            if (!isOpen && !isOpening)
            {
                StartCoroutine(OpenDoor());
            }
        }
    }
    
    private IEnumerator OpenDoor()
    {
        isOpening = true;
        
        float elapsedTime = 0f;
        
        // Animate the door opening
        while (elapsedTime < doorTime)
        {
            doorPivot.rotation = Quaternion.Slerp(closedRotation, openRotation, elapsedTime / doorTime);
            elapsedTime += Time.deltaTime * openSpeed;
            yield return null;
        }
        
        // Ensure door is at exact open position
        doorPivot.rotation = openRotation;
        
        // Mark as open and stay open
        isOpen = true;
        isOpening = false;
        
        Debug.Log("Door opened " + openAngle + " degrees and will stay open");
    }
}