using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

public class DoorHandle : MonoBehaviour
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
    // In the inspector, set Door To Open as your DoorPivot object.


    [Header("Door Reference")]
    [Tooltip("Drag the door GameObject here")]
    public Transform doorToOpen;

    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public float doorTime = 2f;

    private Grabbable grabbable;
    private bool isOpen = false;
    private bool isOpening = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        // Get the Grabbable component on this handle
        grabbable = GetComponent<Grabbable>();

        if (grabbable == null)
        {
            Debug.LogError("No Grabbable component found on " + gameObject.name);
            return;
        }

        if (doorToOpen == null)
        {
            Debug.LogError("No door assigned! Drag the door into the Inspector.");
            return;
        }

        // Set up door rotations
        closedRotation = doorToOpen.rotation;
        openRotation = Quaternion.Euler(doorToOpen.eulerAngles + new Vector3(0, openAngle, 0));

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
        isOpen = true;
        float elapsedTime = 0f;

        while (elapsedTime < doorTime)
        {
            doorToOpen.rotation = Quaternion.Slerp(closedRotation, openRotation, elapsedTime / doorTime);
            elapsedTime += Time.deltaTime * openSpeed;
            yield return null;
        }

        doorToOpen.rotation = openRotation;
        isOpening = false;
    }
}