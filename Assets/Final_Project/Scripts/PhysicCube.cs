using UnityEngine;
using Fusion;

public class PhysicCube : NetworkBehaviour
{
    
    
    [Networked] private TickTimer life { get; set; }
    public void Init(Vector3 forward)
    {

        life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        GetComponent<Rigidbody>().velocity = forward;
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
            Runner.Despawn(Object);
    }
    public void Explosion(Vector3 dir) 
    {
        Rigidbody _rb = gameObject.GetComponent<Rigidbody>();
       
      //  _rb.AddForce(transform.forward * _fireForce, ForceMode.Impulse);
    }

}
