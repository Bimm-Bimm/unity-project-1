using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using TMPro;
using UnityEngine.Animations.Rigging;


public class PlayerController : NetworkBehaviour
{
    public Animator animator;
    public GameObject head;
    public TwoBoneIKConstraint leftHandRig;

    [SerializeField] private Camera myCam;
    [SerializeField] private float camSensitivity;

    [SerializeField] private float moveForce;
    [SerializeField] private float jumpForce;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float verticalSpeed;

    [SerializeField] private float groundDrag;
    [SerializeField] private float frictionFactor;

    [SerializeField] private float gravity;
    [SerializeField] private float gravityMul;

    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private float suctionFactor;

    [SerializeField] private float shotDelay;
    [SerializeField] private float maxRopeLength;
    [Networked] float camRotX { get; set; }
    [Networked] float playerRotY { get; set; }

    LineRenderer lr;
    [SerializeField] private LineRenderer shootingLr;
    [SerializeField] private Transform lrStartPoint;
    [SerializeField] private float yOverShoot;
    [Networked(OnChanged = nameof(OnGrappleGunShoot))] public NetworkBool isGrappleGunShooting { get; set; }
    [Networked] Vector3 lrEndPoint { get; set; }
    [Networked] private TickTimer delay1 { get; set; }
    [Networked] private TickTimer delay2 { get; set; }

    private NetworkBool isGrounded;
    private NetworkBool airTime;
    private NetworkBool disableMovement;
    private Vector3 groundNormal;
    private Rigidbody rb;

    public MeshRenderer[] _playerAllMesh;

    //Shooting
    TickTimer mouse0PressDelay = TickTimer.None;
    TickTimer mouse1PressDelay = TickTimer.None;
    [Networked(OnChanged = nameof(OnFireChanged))] public NetworkBool isShooting { get; set; }
    [SerializeField] private NetworkObject mouse1Bullet;
    [SerializeField] private float maxQRange;
    [SerializeField] private float maxMouse0Range;

    //HP
    HpHandler hpHandler;
    NetworkBool isRespawnRequest;
    public NetworkBool isDead;

    // Ammo 
    // int curAmmo;
    public int maxRightMouseAmmo;
    public int maxLeftMouseAmmo;
    NetworkBool isReloading;
    private TickTimer reloadDelayPress = TickTimer.None;
    [SerializeField] private int mouse0Dmg;
    //[SerializeField] private float mouse1Dmg;

    //Player UI
    public TMP_Text leftMouseAmmoText;
    public TMP_Text rightMouseAmmoText;

    int curRMAmmo;
    int curLMAmmo;
    //Audio
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip mouse_0_Sound;
    [SerializeField] private AudioClip mouse_1_Sound;
    [SerializeField] private AudioClip qPressSound;
    [SerializeField] private AudioClip reloadSound;

    //DelayTiming
    [SerializeField] private float mouse0ShotSpeed;
    [SerializeField] private float mouse1ShotSpeed;
    //Particle
    //[SerializeField] private ParticleSystem mouse0Effect;
    //[SerializeField] private ParticleSystem mouse1Effect;

    //Player Name 
    public TMP_Text playerNickNameTM;
    [Networked(OnChanged = nameof(OnNickNameChanged))]
    public NetworkString<_16> nickName { get; set; }



    private void OnCollisionEnter(Collision collision)
    {
        if (airTime)
        {
            airTime = false;
            lr.enabled = false;
        }
    }

    private void Start()
    {
   
        _playerAllMesh = GetComponents<MeshRenderer>();
        curLMAmmo = maxLeftMouseAmmo;
        curRMAmmo = maxRightMouseAmmo;
        leftMouseAmmoText.text = curLMAmmo.ToString();
        rightMouseAmmoText.text = curRMAmmo.ToString();
        isReloading = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
       
    }
    public override void Spawned()
    {
        _playerAllMesh = GetComponents<MeshRenderer>();
        rb = GetComponentInChildren<Rigidbody>();
        rb.freezeRotation = true;
        lr = GetComponentInChildren<LineRenderer>();
        lr.enabled = false;
        leftMouseAmmoText.text = curLMAmmo.ToString();
        rightMouseAmmoText.text = curRMAmmo.ToString();
        camRotX = 0;
        playerRotY = 0;
        airTime = false;
        isDead = false;
        disableMovement = false;
        hpHandler = GetComponent<HpHandler>();
        if (!Object.HasInputAuthority)
        {
           // 
            myCam.enabled = false;
            leftMouseAmmoText.enabled = false;
            rightMouseAmmoText.enabled = false;
            hpHandler._hpTextInHpHandler.enabled = false;

        }
        else 
        {
            RPC_SetNickName(PlayerPrefs.GetString("PlayerName"));
        }
       
    }
    private void addGravity()
    {
        if (rb.velocity.y < 0f) rb.AddForce(transform.up * gravity * rb.mass * -1 * gravityMul);
        else rb.AddForce(transform.up * gravity * rb.mass * -1);
    }
    private void groundCheck()
    {
        isGrounded = false;
        groundNormal = transform.up;

        Runner.LagCompensation.Raycast(
                    transform.position,
                    transform.up * -1,
                    1.2f,
                    player: Object.InputAuthority,
                    out var hit,
                    -1,
                    HitOptions.IncludePhysX);
        if (hit.Collider != null)
        {
            isGrounded = true;
            groundNormal = hit.Normal;
            delay1 = TickTimer.CreateFromSeconds(Runner, 0.05f);
        }
    }
    private void rotateFromMouseInput(Vector2 input)
    {
        camRotX += input.y;
        camRotX = Mathf.Clamp(camRotX, -85 / camSensitivity, 85 / camSensitivity);
        myCam.transform.localRotation = Quaternion.Euler(camRotX * camSensitivity, 0, 0);

        playerRotY += input.x;
        transform.rotation = Quaternion.Euler(0, playerRotY * camSensitivity, 0);
    }
    private void moveFromInput(Vector2 input, bool jump)
    {
        // move
        if (!disableMovement)
        {
            Vector3 apMoveForce = (transform.forward * input.y + transform.right * input.x).normalized;
            if (Vector3.Angle(transform.up, groundNormal) < maxSlopeAngle)
            {
                apMoveForce = Vector3.ProjectOnPlane(apMoveForce, groundNormal);
            }
            float moveFactor = (isGrounded) ? 1f : 0.2f;
            rb.AddForce(apMoveForce * moveForce * rb.mass * moveFactor);

            // jump
            if (jump && (isGrounded || !delay1.ExpiredOrNotRunning(Runner)))
            {
                animator.SetTrigger("Jump");
                rb.AddForce(transform.up * jumpForce * rb.mass, ForceMode.Impulse);
            }
        }
        
        // speed control
        if (isGrounded)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else if (!airTime)
        {
            Vector3 cappedXZVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            Vector3 cappedYVel = new Vector3(0, rb.velocity.y, 0);
            if (cappedXZVel.magnitude > moveSpeed)
            {
                cappedXZVel = cappedXZVel.normalized * moveSpeed;
            }
            if (cappedYVel.magnitude > verticalSpeed)
            {
                cappedYVel = cappedYVel.normalized * verticalSpeed;
            }

            rb.velocity = cappedXZVel + cappedYVel;
        }

        // friction
        if (input.magnitude < 0.01f && isGrounded)
        {
            Vector2 holVel = new Vector2(rb.velocity.x, rb.velocity.z);
            float amount = Math.Min(holVel.magnitude, frictionFactor);
            Vector2 friction = amount * holVel;
            rb.AddForce(new Vector3(friction.x, 0, friction.y) * -5f * rb.mass);
        }
    }
    private Vector3 calculateGrappleVel(Vector3 currentPos, Vector3 endPos, float maxHeight)
    {
        Debug.Log(currentPos);
        Debug.Log(endPos);
        Debug.Log(maxHeight);
        float yDiff = endPos.y - currentPos.y;
        Debug.Log(yDiff);
        Vector3 xzDiff = new Vector3(endPos.x - currentPos.x, 0f, endPos.z - currentPos.z);
        
        float tUp = Mathf.Sqrt(2 * maxHeight / gravity);
        float tDown = Mathf.Sqrt(-2 * (yDiff - maxHeight) / (gravity * gravityMul));
        Vector3 vY = Vector3.up * Mathf.Sqrt(2 * gravity * maxHeight);
        Debug.Log(vY);
        Vector3 vXZ = xzDiff / (tUp + tDown);
        return vY + vXZ;
    }
    private void grappleGunShoot()
    {
        audioSource.PlayOneShot(qPressSound);
        if (Runner.LagCompensation.Raycast(
                    myCam.transform.position,
                    myCam.transform.forward,
                    150f,
                    player: Object.InputAuthority,
                    out var hit,
                    -1,
                    HitOptions.IncludePhysX))
        {
            lrEndPoint = myCam.transform.position + myCam.transform.forward * hit.Distance;
            Vector3 playerFeet = transform.position - transform.up * 0.5f;
            StartCoroutine(GrappleStateCO());
            if (playerFeet.y < lrEndPoint.y) rb.velocity = calculateGrappleVel(playerFeet, lrEndPoint, lrEndPoint.y - playerFeet.y + yOverShoot);
            else rb.velocity = calculateGrappleVel(playerFeet, lrEndPoint, yOverShoot);
            airTime = true;
        }
    }
    IEnumerator GrappleStateCO()
    {
        isGrappleGunShooting = true;
        yield return new WaitForSeconds(0.02f);
        isGrappleGunShooting = false;
    }
    /*    IEnumerator GrappleEffectCO()
        {
            Runner.LagCompensation.Raycast(
                        myCam.transform.position,
                        myCam.transform.forward,
                        50f,
                        player: Object.InputAuthority,
                        out var hit,
                        -1,
                        HitOptions.IncludePhysX);
            lrEndPoint = myCam.transform.position + myCam.transform.forward * hit.Distance;
            lr.SetPosition(1, lrEndPoint);
            lr.enabled = true;
            yield return new WaitForSeconds(shotDelay);
            lr.enabled = false;
        }*/
    void grappleEffectWrarperFunc()
    {
        /*StartCoroutine(GrappleEffectCO());*/
        airTime = true;
        Runner.LagCompensation.Raycast(
                    myCam.transform.position,
                    myCam.transform.forward,
                    maxQRange,
                    player: Object.InputAuthority,
                    out var hit,
                    -1,
                    HitOptions.IncludePhysX);
        lrEndPoint = myCam.transform.position + myCam.transform.forward * hit.Distance;
        lr.SetPosition(1, lrEndPoint);
        lr.enabled = true;
    }
    // Shooting
    static void OnFireChanged(Changed<PlayerController> changed)
    {
        bool isFiringCurrent = changed.Behaviour.isShooting;
        changed.LoadOld();
        bool isFiringOld = changed.Behaviour.isShooting;
        if (isFiringCurrent && !isFiringOld)
        {
            changed.Behaviour.OnFireRemote();
        }
    }

    IEnumerator GunStateCO()
    {
        isShooting = true;
        yield return new WaitForSeconds(0.02f);
        isShooting = false;
    }
    IEnumerator RaycastThing(Vector3 origin, Vector3 end)
    {
        var castUntil = DateTime.Now.AddSeconds(mouse0ShotSpeed * 0.9f);
        //Debug.Log("Raycastthing");
        shootingLr.enabled = true;
        disableMovement = true;
        Debug.Log("IS Shooting");
        while(DateTime.Now < castUntil)
        {
            shootingLr.SetPosition(0, lrStartPoint.position);
            Runner.LagCompensation.Raycast(myCam.transform.position,
                   myCam.transform.forward,
                   maxMouse0Range,
                   player: Object.InputAuthority,
                   out var hit,
                   -1,
                   HitOptions.IncludePhysX);
            if (hit.Hitbox != null)
            {
                hit.Hitbox.transform.root.GetComponent<HpHandler>().OnTakeDmg(mouse0Dmg);
            }
            if (hit.Distance == 0)
            {
                lrEndPoint = myCam.transform.position + myCam.transform.forward * maxMouse0Range;
                shootingLr.SetPosition(1, lrEndPoint);
            }
            else
            {
                lrEndPoint = myCam.transform.position + myCam.transform.forward * hit.Distance;
                shootingLr.SetPosition(1, lrEndPoint);
            }
            yield return null;
        }
        /*shootingLr.SetPosition(0, lrStartPoint.position);*/
        //shootingLr.SetPosition(1, end);
        disableMovement = false;
        shootingLr.enabled = false;
    }
    void OnFireRemote()
    {
        //Debug.Log("OnFireRemote");
        StartCoroutine(RaycastThing(myCam.transform.position + myCam.transform.forward + myCam.transform.right / 2 , 
            myCam.transform.position + myCam.transform.forward + myCam.transform.right / 2 + myCam.transform.forward * maxMouse0Range));
    }
    public void Shooting()
    {
        
        if (curLMAmmo <= 0)
        {
            StartCoroutine(ReloadDelay());
            return;
        }
        curLMAmmo--;
        StartCoroutine(GunStateCO());
        audioSource.PlayOneShot(mouse_0_Sound);
     
        leftMouseAmmoText.text = curLMAmmo.ToString();
        rightMouseAmmoText.text = curRMAmmo.ToString();
        /*Runner.LagCompensation.Raycast(myCam.transform.position,
                   myCam.transform.forward,
                   maxMouse0Range,
                   player: Object.InputAuthority,
                   out var hit,
                   -1,
                   HitOptions.IncludePhysX);
        if (hit.Hitbox != null)
        {
            hit.Hitbox.transform.root.GetComponent<HpHandler>().OnTakeDmg(mouse0Dmg);
        }
        if (hit.Distance == 0)
        {
            lrEndPoint = myCam.transform.position + myCam.transform.forward * maxMouse0Range;
            shootingLr.SetPosition(1, lrEndPoint);
        }
        else
        {
            lrEndPoint = myCam.transform.position + myCam.transform.forward * hit.Distance;
            shootingLr.SetPosition(1, lrEndPoint);
        }*/


    }
    public void Launching()
    {
        if (curRMAmmo <= 0)
        {
            StartCoroutine(ReloadDelay());
            return;
        }
        curRMAmmo--;
        audioSource.PlayOneShot(mouse_1_Sound);
       // mouse1Effect.Play();

        leftMouseAmmoText.text = curLMAmmo.ToString();
        rightMouseAmmoText.text = curRMAmmo.ToString();
        /* Runner.Spawn(mouse1Bullet,
             myCam.transform.position + myCam.transform.forward + myCam.transform.right / 2,
             Quaternion.LookRotation(myCam.transform.forward),
             Object.InputAuthority,
                 (runner, spawn) =>
                 {
                     spawn.GetComponent<ExplosionBullet>().Init();
                 });*/
        Runner.Spawn(mouse1Bullet,
            lrStartPoint.position,
            Quaternion.LookRotation(myCam.transform.forward),
            Object.InputAuthority,
                (runner, spawn) =>
                {
                    spawn.GetComponent<ExplosionBullet>().Init();
                });
    }

    IEnumerator ReloadDelay()
    {
        isReloading = true;
        audioSource.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(1.5f);
        isReloading = false;
        curLMAmmo = maxLeftMouseAmmo;
        curRMAmmo = maxRightMouseAmmo;
        leftMouseAmmoText.text = curLMAmmo.ToString();
        rightMouseAmmoText.text = curRMAmmo.ToString();

    }
    public void RequestRespawn()
    {
        
        for (int i = 0; i < _playerAllMesh.Length; i++) 
        {
            Debug.Log("mesh_Hide");
            _playerAllMesh[i].gameObject.SetActive(false);
        
        }
        Debug.Log("respawn");
        isRespawnRequest = true;
    }
    void Respawn()
    {
       
        //Need spawn point
        StartCoroutine(OnReviceCD());

    }
    IEnumerator OnReviceCD() 
    {
        isRespawnRequest = false;
    
        yield return new WaitForSeconds(3f);

        transform.position = new Vector3(UnityEngine.Random.Range(-10.0f, 10.0f), 50, UnityEngine.Random.Range(-10.0f, 10.0f));
        curRMAmmo = maxRightMouseAmmo;


    }

    public void SetPlayerControllerEnable(bool isEnable)
    {
        Debug.Log("respawn 1");
        isRespawnRequest = true;

        Debug.Log("respawn 2");

       // _cc.Controller.enabled = isEnable;
    }
    static void OnNickNameChanged(Changed<PlayerController> changed)
    {
        changed.Behaviour.OnNickNameChanged();
    }
    private void OnNickNameChanged()
    {
        playerNickNameTM.text = nickName.ToString();
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickName, RpcInfo info = default)
    {
        this.nickName = nickName;
    }



    static void OnGrappleGunShoot(Changed<PlayerController> changed)
    {
        bool isShootingCurrent = changed.Behaviour.isGrappleGunShooting;
        changed.LoadOld();
        bool isShootingOld = changed.Behaviour.isGrappleGunShooting;
        if (isShootingCurrent && !isShootingOld)
            changed.Behaviour.grappleEffectWrarperFunc();
    }
    public override void FixedUpdateNetwork()
    {
        lr.SetPosition(0, lrStartPoint.position);

        addGravity();
        if (!airTime) 
        {
            groundCheck();
        }
        if (GetInput(out NetworkInputData data))
        {
            //Respawn
            if (isRespawnRequest) 
            {
                //rb.velocity = Vector3.zero;
                Respawn();
            }
            //Hp & Ammo
            if (Object.HasStateAuthority)
            {
                if (hpHandler._isDead)
                    return;
            }
            
            rotateFromMouseInput(data.mouseInput);
            if (!airTime) 
            {
                moveFromInput(data.movInput, data._isJumpPressed);
            }
            
            //Q press
            if (data._isQPress && delay2.ExpiredOrNotRunning(Runner) && !airTime)
            {
                delay2 = TickTimer.CreateFromSeconds(Runner, shotDelay + 0.2f);
                grappleGunShoot();
            }
            //Mouse 0 Press
            if (data._isMouse0Press && mouse0PressDelay.ExpiredOrNotRunning(Runner) && !isReloading && curLMAmmo > 0)
            {
                mouse0PressDelay = TickTimer.CreateFromSeconds(Runner, mouse0ShotSpeed);
                Shooting();
            }
            
            //Mouse 1 Press
            if (data._isMouse1Press && mouse1PressDelay.ExpiredOrNotRunning(Runner)  && !isReloading && curRMAmmo > 0)
            {
                mouse1PressDelay = TickTimer.CreateFromSeconds(Runner, mouse1ShotSpeed);
                Launching();
            }
            //Reload Press
            if ( data._isReloadPress && curRMAmmo < maxRightMouseAmmo && reloadDelayPress.ExpiredOrNotRunning(Runner)
                || data._isReloadPress && curLMAmmo < maxLeftMouseAmmo && reloadDelayPress.ExpiredOrNotRunning(Runner))
            {
                reloadDelayPress = TickTimer.CreateFromSeconds(Runner, 0.5f);
                StartCoroutine(ReloadDelay());
            }
        }
        
    }

}
