using System;
using UnityEngine;

public class AnimationStateChanger : MonoBehaviour
{
    public Animator animator;
    public string currentAnimation = "";
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ChangeAnimationState(String newAnimationState) {
        animator.Play(newAnimationState);
    }
    public void StopAnimation() {
        animator.StopPlayback();
    }
}
