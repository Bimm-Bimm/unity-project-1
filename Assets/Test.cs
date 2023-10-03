using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] ParticleSystem _p;
     
    void Start()
    {
        _p.GetComponent<ParticleSystem>();
        _p.Play();
    }

    
    void Update()
    {
        _p.Play();
    }
}
