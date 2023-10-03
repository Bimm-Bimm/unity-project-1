using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
public class PlayerUI : MonoBehaviour
{

    public PlayerController _playerControl;
    int _ammo;
    public GameObject _ammoText;
    private void Start()
    {
        _playerControl.GetComponentInChildren<PlayerController>();
       // _ammo = _playerControl.getCurrentAmmo();
        UpdateAmmoText();
    }

    void Update()
    {
        UpdateAmmoText();  
    }
    private void UpdateAmmoText() 
    {
        _ammoText.GetComponent<Text>().text = _ammo.ToString();   
            
    }
}
