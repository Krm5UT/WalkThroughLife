using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

public class DoorHandleTeleport : MonoBehaviour
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
    // Set Player Rig to your OVRCameraRig or XR Origin object.
    
    [Header("Door Reference")]
    [Tooltip("Drag the door GameObject here")]
    public Transform doorToOpen;
    
    [Header("Player Reference")]
    [Tooltip("Drag your OVRCameraRig or XR Origin here")]
    public Transform playerRig;
    
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public float doorTime = 2f;
    
    [Header("Teleport Settings")]
    [Tooltip("Delay after door opens before teleporting back")]
    public float teleportDelay = 1f;
    
    [Header("Movement Freeze Settings")]
    [Tooltip("Freeze player movement when door is grabbed")]
    public bool freezeMovement = true;
    
    private Grabbable grabbable;
    private bool isOpen = false;
    private bool isOpening = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    
    // Store the rig's initial position and rotation
    private Vector3 initialRigPosition;
    private Quaternion initialRigRotation;
    
    // Components to freeze
    private CharacterController characterController;
    private OVRPlayerController ovrPlayerController;
    private Rigidbody rigidBody;
    
    // Store enabled states
    private bool wasCharacterControllerEnabled;
    private bool wasOVRPlayerControllerEnabled;
    private bool wasRigidbodyKinematic;
    
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
        
        if (playerRig == null)
        {
            Debug.LogError("No player rig assigned! Drag your OVRCameraRig/XR Origin into the Inspector.");
            return;
        }
        
        // Set up door rotations
        closedRotation = doorToOpen.rotation;
        openRotation = Quaternion.Euler(doorToOpen.eulerAngles + new Vector3(0, openAngle, 0));
        
        // Store the rig's initial position and rotation
        initialRigPosition = playerRig.position;
        initialRigRotation = playerRig.rotation;
        
        // Find movement components on the player rig
        characterController = playerRig.GetComponent<CharacterController>();
        ovrPlayerController = playerRig.GetComponent<OVRPlayerController>();
        rigidBody = playerRig.GetComponent<Rigidbody>();
        
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
    
    private void FreezePlayerMovement()
    {
        if (!freezeMovement) return;
        
        // Disable CharacterController
        if (characterController != null)
        {
            wasCharacterControllerEnabled = characterController.enabled;
            characterController.enabled = false;
        }
        
        // Disable OVRPlayerController
        if (ovrPlayerController != null)
        {
            wasOVRPlayerControllerEnabled = ovrPlayerController.enabled;
            ovrPlayerController.enabled = false;
        }
        
        // Make Rigidbody kinematic to prevent physics movement
        if (rigidBody != null)
        {
            wasRigidbodyKinematic = rigidBody.isKinematic;
            rigidBody.isKinematic = true;
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }
        
        Debug.Log("Player movement frozen");
    }
    
    private void UnfreezePlayerMovement()
    {
        if (!freezeMovement) return;
        
        // Re-enable CharacterController
        if (characterController != null)
        {
            characterController.enabled = wasCharacterControllerEnabled;
        }
        
        // Re-enable OVRPlayerController
        if (ovrPlayerController != null)
        {
            ovrPlayerController.enabled = wasOVRPlayerControllerEnabled;
        }
        
        // Restore Rigidbody state
        if (rigidBody != null)
        {
            rigidBody.isKinematic = wasRigidbodyKinematic;
        }
        
        Debug.Log("Player movement unfrozen");
    }
    
    private IEnumerator OpenDoor()
    {
        isOpening = true;
        isOpen = true;
        
        // Freeze player movement immediately when door is grabbed
        FreezePlayerMovement();
        
        float elapsedTime = 0f;
        
        // Animate the door opening
        while (elapsedTime < doorTime)
        {
            doorToOpen.rotation = Quaternion.Slerp(closedRotation, openRotation, elapsedTime / doorTime);
            elapsedTime += Time.deltaTime * openSpeed;
            yield return null;
        }
        
        doorToOpen.rotation = openRotation;
        
        // Teleport the player back to their original position
        if (characterController != null)
        {
            // For CharacterController, disable it briefly for teleport
            bool wasEnabled = characterController.enabled;
            characterController.enabled = false;
            playerRig.position = initialRigPosition;
            playerRig.rotation = initialRigRotation;
            yield return null;
            characterController.enabled = wasEnabled;
        }
        else
        {
            playerRig.position = initialRigPosition;
            playerRig.rotation = initialRigRotation;
        }
        
        // Reset the door back to closed position
        doorToOpen.rotation = closedRotation;
        
        // Unfreeze player movement after teleport
        UnfreezePlayerMovement();
        
        isOpen = false;
        isOpening = false;
    }
}