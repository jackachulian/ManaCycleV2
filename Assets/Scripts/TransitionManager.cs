using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Unity.Netcode;

public class TransitionManager : MonoBehaviour
{
    private string nextScene;
    private string nextTransition;
    [SerializeField] private Animator animator;

    public static TransitionManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Duplicate TransitionManager spawned, destroying the new one");
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this); 
    }

    public void TransitionToScene(string scene, string transitionName = "Wipe")
    {
        nextScene = scene;
        nextTransition = transitionName;
        animator.SetBool("sceneReady", false);
        animator.Play(transitionName);
        Debug.Log("Transisition to " + transitionName);
    }

    // Called from animation event
    void OnTransitionAnimationFinished()
    {
        if (NetworkManager.Singleton && NetworkManager.Singleton.IsListening && NetworkManager.Singleton.IsServer) {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnNetworkSceneLoaded;
            NetworkManager.Singleton.SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        } else {
            SceneManager.LoadScene(nextScene);
            AnimateOut();
        }
    }

    private void OnNetworkSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnNetworkSceneLoaded;
        AnimateOut();
    }
   
    void AnimateOut() {
        animator.SetBool("sceneReady", true);
    }
}
