using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimatorController : MonoBehaviour
{
    public static AnimatorController animController;
    private float x_Vel;
    private float y_Vel;
    private bool isGrounded;
    [SerializeField] Animator anim;

    private void Awake()
    {
        animController = this;
        x_Vel = Animator.StringToHash("X_Velocity");
        y_Vel = Animator.StringToHash("Y_Velocity");
        
    }
    public void JumpTrigger() 
    {
        anim.SetTrigger("Jump");
    }
    public void FireTrigger() 
    {
        anim.SetTrigger("Firing");
    }
    public void ReloadTrigger()
    {
        anim.SetTrigger("Reloading");
    }
    public void GroundCheck(bool groundCheck) 
    {
        anim.SetBool("IsGrounded", groundCheck);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
