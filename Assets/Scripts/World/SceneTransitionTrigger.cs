using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Tooltip("Name of the scene to load when the player enters this trigger.")]
    public string sceneToLoad;

    [Tooltip("Optional: Set a delay before the scene loads.")]
    public float transitionDelay = 0f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            if (transitionDelay > 0f)
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
        SceneManager.LoadScene(sceneToLoad);
    }
}
