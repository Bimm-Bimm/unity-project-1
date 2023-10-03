using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{

    void Start()
    {
        StartCoroutine(DestroyObj());
    }
    IEnumerator DestroyObj() 
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
        
    }
    
    void Update()
    {
        
    }
}
