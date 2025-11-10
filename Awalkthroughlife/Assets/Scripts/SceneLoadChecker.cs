using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadChecker : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);
        // You can activate your VR rig here if needed
        // Example: vrRig.SetActive(true);
    }

    public void LoadVRScene()
    {
        SceneManager.LoadScene("VRWorld");
    }
}
