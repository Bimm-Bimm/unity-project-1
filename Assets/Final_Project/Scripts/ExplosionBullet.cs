using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ExplosionBullet : NetworkBehaviour
{
    [SerializeField] GameObject _explosionEffect;
    [SerializeField] float _explosionRad;
    [SerializeField] float _explosionForce;
    [SerializeField] float _fireForce;
    [SerializeField] float _upwardForce;
    [SerializeField] int _explosionDmg;
    bool _hitOnce;
    Rigidbody _rb;
    TickTimer _lifeTime = TickTimer.None;
    private readonly List<LagCompensatedHit> _hits = new List<LagCompensatedHit>();
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip _explosionSound;

    private void OnCollisionEnter(Collision collision)
    {
        if (_hitOnce) 
        {
            return;
           
        }
        this.GetComponentInChildren<MeshRenderer>().enabled = false;
        var hits = Physics.OverlapSphere(transform.position, 1.5f * GridMetrics.ChunkScale);
        foreach (var hit in hits)
        {
            if(hit.gameObject.TryGetComponent<Chunk>(out var hitChunk))
                hitChunk.EditWeights(transform.position, 1.5f * GridMetrics.ChunkScale, false);
        }
            Explode();
        //Runner.Despawn(Object);
        //_isFlying = false;
    }

    public void Explode() 
    {
        _hitOnce = true;
        audioSource.PlayOneShot(_explosionSound);
        Instantiate(_explosionEffect, transform.position, Quaternion.identity);

        Runner.LagCompensation.OverlapSphere(transform.position,_explosionRad, Object.InputAuthority , _hits, -1);
       
        foreach ( var hit in _hits)
        {

            if (hit.Hitbox != null)
            {

                    hit.Hitbox.transform.root.GetComponent<HpHandler>().OnTakeDmg(_explosionDmg);
                hit.Hitbox.transform.root.GetComponent<NetworkRigidbody>().Rigidbody.AddExplosionForce(_explosionForce, transform.position, _explosionRad,
                1f, ForceMode.Impulse);

            }
        }

    }
   


    public void Init()
    {
        _hitOnce = false;
        _rb = gameObject.GetComponent<Rigidbody>();
        _lifeTime = TickTimer.CreateFromSeconds(Runner, 3.0f);
        _rb.AddForce(transform.forward * _fireForce, ForceMode.Impulse);
   

    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority) 
        {
            if (_lifeTime.Expired(Runner)) 
            {
                if (_hitOnce == false) 
                {
                    Instantiate(_explosionEffect, transform.position, Quaternion.identity);
                }
                Runner.Despawn(Object);

                _lifeTime = TickTimer.None;
            }
        }
    }

}
