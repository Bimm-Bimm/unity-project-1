using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Characters : NetworkBehaviour
{
    public float _curHp;
    public float _maxHp;
   /* public override void FixedUpdateNetwork()
    {
        if(_curHp <= 0) 
        {
            
        }
    }*/
    public void Dead() 
    {
        _curHp = 0;
        //Debug.Log("player dead");

    }
    public void Respawm() 
    {
        
    }
    public void TakeDmg(float dmg) 
    {
        _curHp -= dmg;
    }
}
