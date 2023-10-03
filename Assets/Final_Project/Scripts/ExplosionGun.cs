/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ExplosionGun : NetworkBehaviour
{
   
   [SerializeField] private ExplosionGun _explosionGun;
    [SerializeField] private NetworkObject _explosionBullet;
    public float _fireForce;
    public float _dmg;
    public Transform _firePoint;
    TickTimer _fireDelay = TickTimer.None;
    public GameObject _playerHolder;
    [SerializeField] public NetworkBool _isLaunching;
    Camera _myCam;

    private void Start()
    {
        _myCam = _playerHolder.GetComponentInChildren<Camera>();
    }
    private void Awake()
    {
        _explosionGun = GetComponent<ExplosionGun>();
       

    }
    public void Shoot() 
    {
        if (_fireDelay.ExpiredOrNotRunning(Runner)) 
        {

            _fireDelay = TickTimer.CreateFromSeconds(Runner , 4.0f);
        }
       
        
        Runner.Spawn(_explosionBullet,
                 _playerHolder.transform.forward ,
                Quaternion.LookRotation(_myCam.transform.forward),
                Object.InputAuthority,
          (runner, spawn) =>
          {
              spawn.GetComponent<ExplosionBullet>().Init();
          });

    }
    public override void FixedUpdateNetwork() 
    {
        if (GetInput(out NetworkInputData data)) 
        {
            if (data._isShooting) 
            {
                Shoot();
            }
        }
    }
    
    }
*/