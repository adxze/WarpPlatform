using UnityEngine;
using System.Collections;

public class TransitionController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private string animationTrigger = "Entry";
    [SerializeField] private float destroyDelay = 0.5f;
    
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogWarning("No Animator found on Transition canvas!");
            Destroy(gameObject);
            return;
        }
        
        StartTransition();
    }
    
    private void StartTransition()
    {
        animator.SetTrigger(animationTrigger);
        
        StartCoroutine(WaitForAnimationComplete());
    }
    
    private IEnumerator WaitForAnimationComplete()
    {
        yield return null;
        
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(destroyDelay);
        
        Destroy(gameObject);
        
    }
}