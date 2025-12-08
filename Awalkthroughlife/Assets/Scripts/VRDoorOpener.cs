using System.Collections;
using UnityEngine;
using Oculus.Interaction;

public class VRDoorOpener : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("The door pivot point (parent object at the hinge location)")]
    public Transform doorPivot;
    
    [Tooltip("Which side does the door open? (Left = positive rotation, Right = negative rotation)")]
    public DoorSide doorSide = DoorSide.Left;
    
    [Tooltip("How long the door takes to open (in seconds)")]
    public float openDuration = 1f;
    
    [Tooltip("Animation curve for smooth door motion (optional)")]
    public AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio (Optional)")]
    [Tooltip("Sound to play when door starts opening")]
    public AudioSource doorOpenSound;
    
    [Tooltip("Sound to play when door finishes opening")]
    public AudioSource doorCompleteSound;
    
    [Header("Auto-Setup")]
    [Tooltip("Automatically find the door pivot in parent hierarchy")]
    public bool autoFindPivot = true;
    
    private Grabbable grabbable;
    private bool isOpen = false;
    private bool isAnimating = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    
    public enum DoorSide
    {
        Left,   // Door opens away from you (positive Y rotation)
        Right   // Door opens toward you (negative Y rotation)
    }
    
    void Start()
    {
        // Get Grabbable component from this handle
        grabbable = GetComponent<Grabbable>();
        
        if (grabbable == null)
        {
            Debug.LogError("[VRDoorOpener] No Grabbable component found on " + gameObject.name);
            enabled = false;
            return;
        }
        
        // Auto-find door pivot if needed
        if (autoFindPivot && doorPivot == null)
        {
            FindDoorPivot();
        }
        
        if (doorPivot == null)
        {
            Debug.LogError("[VRDoorOpener] No door pivot assigned! Please assign manually or enable auto-find.");
            enabled = false;
            return;
        }
        
        // Store the closed rotation
        closedRotation = doorPivot.rotation;
        
        // Calculate open rotation (90 degrees based on door side)
        float openAngle = doorSide == DoorSide.Left ? 90f : -90f;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        
        // Subscribe to touch/hover events (triggers on touch, not just grab)
        grabbable.WhenPointerEventRaised += OnPointerEvent;
        
        Debug.Log("[VRDoorOpener] Setup complete. Door will open " + openAngle + " degrees.");
    }
    void Update()
{
    // Press T key to test rotation manually
    if (Input.GetKeyDown(KeyCode.T))
    {
        Debug.Log("Testing rotation - current: " + doorPivot.rotation.eulerAngles);
        doorPivot.Rotate(0, 10, 0);
        Debug.Log("After rotation: " + doorPivot.rotation.eulerAngles);
    }
}
    
    void OnDestroy()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }
    }
    
    private void FindDoorPivot()
    {
        // Search up the hierarchy for a pivot object
        Transform current = transform.parent;
        
        while (current != null)
        {
            // Look for objects named with "Pivot", "Hinge", or "Door"
            if (current.name.ToLower().Contains("pivot") || 
                current.name.ToLower().Contains("hinge") ||
                current.name.ToLower().Contains("door"))
            {
                doorPivot = current;
                Debug.Log("[VRDoorOpener] Auto-found door pivot: " + doorPivot.name);
                return;
            }
            current = current.parent;
        }
        
        // If nothing found, use immediate parent
        if (transform.parent != null)
        {
            doorPivot = transform.parent;
            Debug.Log("[VRDoorOpener] Using parent as pivot: " + doorPivot.name);
        }
    }
    
    private void OnPointerEvent(PointerEvent pointerEvent)
{
    Debug.Log("[VRDoorOpener] POINTER EVENT: " + pointerEvent.Type); // ADD THIS LINE
    
    if (pointerEvent.Type == PointerEventType.Select && !isOpen && !isAnimating)
    {
        OpenDoor();
    }
}
    
    private void OpenDoor()
    {
        if (isOpen || isAnimating) return;
        
        StartCoroutine(OpenDoorCoroutine());
    }
    
    private IEnumerator OpenDoorCoroutine()
    {
        isAnimating = true;
        
        // Play opening sound
        if (doorOpenSound != null)
        {
            doorOpenSound.Play();
        }
        
        float elapsed = 0f;
        
        // Animate door opening
        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / openDuration;
            
            // Apply animation curve for smooth motion
            float curveValue = openCurve.Evaluate(t);
            
            // Smoothly rotate door
            doorPivot.rotation = Quaternion.Slerp(closedRotation, openRotation, curveValue);
            
            yield return null;
        }
        
        // Ensure door reaches exact final position
        doorPivot.rotation = openRotation;
        
        // Play completion sound
        if (doorCompleteSound != null)
        {
            doorCompleteSound.Play();
        }
        
        isOpen = true;
        isAnimating = false;
        
        Debug.Log("[VRDoorOpener] Door opened successfully!");
    }
    
    // Optional: Method to close the door (call this if you want to make it closeable)
    public void CloseDoor()
    {
        if (!isOpen || isAnimating) return;
        
        StartCoroutine(CloseDoorCoroutine());
    }
    
    private IEnumerator CloseDoorCoroutine()
    {
        isAnimating = true;
        
        float elapsed = 0f;
        
        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / openDuration;
            float curveValue = openCurve.Evaluate(t);
            
            doorPivot.rotation = Quaternion.Slerp(openRotation, closedRotation, curveValue);
            
            yield return null;
        }
        
        doorPivot.rotation = closedRotation;
        
        isOpen = false;
        isAnimating = false;
        
        Debug.Log("[VRDoorOpener] Door closed.");
    }
    
    // Debug visualization in Scene view
    void OnDrawGizmosSelected()
    {
        if (doorPivot == null) return;
        
        // Draw pivot point
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(doorPivot.position, 0.05f);
        
        // Draw door swing arc
        Gizmos.color = Color.green;
        Vector3 direction = doorPivot.right * 0.5f;
        
        float angle = doorSide == DoorSide.Left ? 90f : -90f;
        int segments = 20;
        
        for (int i = 0; i < segments; i++)
        {
            float currentAngle = (angle / segments) * i;
            float nextAngle = (angle / segments) * (i + 1);
            
            Vector3 current = doorPivot.position + Quaternion.Euler(0, currentAngle, 0) * direction;
            Vector3 next = doorPivot.position + Quaternion.Euler(0, nextAngle, 0) * direction;
            
            Gizmos.DrawLine(current, next);
        }
    }
}