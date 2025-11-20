public class LightTrigger : MonoBehaviour
{
    [Header("Light Settings")]
    [Tooltip("The spotlight to turn on when player enters")]
    public Light spotLight;
    [Header("Optional Settings")]
    [Tooltip("Tag to identify the player (default: 'Player')")]
    public string playerTag = "Player";
    [Tooltip("Turn off light when player exits? (default: false)")]
    public bool turnOffOnExit = false;
    private void Start()
    {
        // Make sure the trigger collider is set to trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("LightTrigger: No collider found on " + gameObject.name);
        }
        // Check if spotlight is assigned
        if (spotLight == null)
        {
            Debug.LogWarning("LightTrigger: No spotlight assigned on " + gameObject.name);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering has the player tag
        if (other.CompareTag(playerTag))
        {
            // Turn on the spotlight
            if (spotLight != null)
            {
                spotLight.enabled = true;
                Debug.Log("Spotlight turned ON");
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        // Optionally turn off light when player exits
        if (turnOffOnExit && other.CompareTag(playerTag))
        {
            if (spotLight != null)
            {
                spotLight.enabled = false;
                Debug.Log("Spotlight turned OFF");
            }
        }
    }
}