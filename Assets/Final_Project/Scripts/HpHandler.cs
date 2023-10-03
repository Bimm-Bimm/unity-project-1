using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class HpHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnHPChanged))]
    [SerializeField]int _Hp { get; set; }

    [Networked(OnChanged = nameof(OnStageChanged))]
    public NetworkBool _isDead { get; set; }
    bool _isInitialized = false;
    public int _maxHp;
    
    PlayerController _playerController;
    HitboxRoot _hitboxRoot;
    bool _isHit;
    
    public TMP_Text _hpTextInHpHandler;
    private void Awake()
    {
        _playerController = GetComponentInChildren<PlayerController>();
        _hitboxRoot = GetComponentInChildren<HitboxRoot>();
  
    }
    void Start()
    {
        //_playerController._playerAllMesh = GetComponents<MeshRenderer>();
        _isDead = false; 
        _Hp = _maxHp;
    }
    public override void Spawned()
    {

        if (!Object.HasInputAuthority)
        {
            _hpTextInHpHandler.enabled = false;

        }
        else
            _hpTextInHpHandler.enabled = true;
    }
        public void OnTakeDmg(int dmg) 
    {
        if (_isDead)
            return;

         _Hp = _Hp - dmg;
        if (_Hp <= 0) 
        {
            _Hp = 0;
            _hpTextInHpHandler.text = _Hp.ToString();
            StartCoroutine(ServerReviveCD());
            _isDead = true;
        }
   
    }
   
    IEnumerator ServerReviveCD() 
    {
        _Hp = _maxHp;
        _isDead = true;
        Debug.Log(_isDead);
        _playerController.RequestRespawn();
        for (int i = 0; i < _playerController._playerAllMesh.Length; i++)
        {
            _playerController._playerAllMesh[i].enabled = false;

        }
        //Camera.main.gameObject.SetActive(true);
        OnDead();
        yield return new WaitForSeconds(3.0f);
        _isDead = false;
        _Hp = _maxHp;
        OnRevive();
        OnRespawn();
    }



    static void OnHPChanged(Changed<HpHandler> changed) 
    {

        int _curHp = changed.Behaviour._Hp;
        changed.LoadOld();
        int _oldHp = changed.Behaviour._Hp;
        if (_curHp < _oldHp)
            changed.Behaviour._hpTextInHpHandler.text = _curHp.ToString();
    }
    private void OnHpReduce() 
    {
        _hpTextInHpHandler.text = _Hp.ToString();
        if (_isInitialized)
            return;
    }
    static void OnStageChanged(Changed<HpHandler> changed)
    {
        bool isDeadCurrent = changed.Behaviour._isDead;
        changed.LoadOld();
        bool isDeadOld = changed.Behaviour._isDead;
        if (isDeadCurrent && !isDeadOld)
            changed.Behaviour.OnDead();
        else if (!isDeadCurrent && isDeadOld)
            changed.Behaviour.OnRevive();
    }
    private void OnDead() 
    {
      
        _hitboxRoot.HitboxRootActive = false;
        _Hp = _maxHp;
        _hpTextInHpHandler.text = _maxHp.ToString();
        //_playerControl.SetPlayerControllerEnable(false);
        _playerController.RequestRespawn();
        for (int i = 0; i < _playerController._playerAllMesh.Length; i++)
        {
            _playerController._playerAllMesh[i].enabled = false;

        }
    }
    private void OnRevive() 
    {
        _hpTextInHpHandler.text = _Hp.ToString();
        _hitboxRoot.HitboxRootActive = true;
       // _playerControl.SetPlayerControllerEnable(true);
    }

    public void OnRespawn() 
    {
        _isDead = false;
        _Hp = _maxHp;
        _hpTextInHpHandler.text = _maxHp.ToString();

    }
}
