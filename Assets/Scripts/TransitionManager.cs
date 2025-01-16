using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
            
    }

    public void TransitionToScene(string scene, string transitionName = "Wipe")
    {
        nextScene = scene;
        nextTransition = transitionName;
        animator.Play(transitionName);
        Debug.Log("Transisition to " + transitionName);
    }

    // Called from animation event
    void OnTransitionAnimationFinished()
    {
        SceneManager.LoadScene(nextScene);
        animator.Play(nextTransition + "Out");
    }
   
}
