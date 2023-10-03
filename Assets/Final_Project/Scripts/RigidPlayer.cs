using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
public class RigidPlayer : NetworkBehaviour
{

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
    [SerializeField] private Transform lrStartPoint;
    [SerializeField] private float yOverShoot;
    [Networked(OnChanged = nameof(OnGrappleGunShoot))] public NetworkBool isGrappleGunShooting { get; set; }
    [Networked] Vector3 lrEndPoint { get; set; }
    [Networked] private TickTimer delay1 { get; set; }
    [Networked] private TickTimer delay2 { get; set; }

    private NetworkBool isGrounded;
    private Vector3 groundNormal;
    private Rigidbody rb;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    }
    public override void Spawned()
    {
        rb = GetComponentInChildren<Rigidbody>();
        rb.freezeRotation = true;
        lr = GetComponentInChildren<LineRenderer>();
        lr.enabled = false;
        if (!Object.HasInputAuthority)
        {
            myCam.enabled = false;
        }
        camRotX = 0;
        playerRotY = 0;
    }
    private void addGravity()
    {
        if(rb.velocity.y < 0f) rb.AddForce(transform.up * gravity * rb.mass * -1 * gravityMul);
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
    private void rotateFromMouseInput (Vector2 input)
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
        Vector3 apMoveForce = (transform.forward * input.y + transform.right * input.x).normalized;
        if (Vector3.Angle(transform.up, groundNormal) < maxSlopeAngle)
        {
            apMoveForce = Vector3.ProjectOnPlane(apMoveForce, groundNormal);
        }
        float moveFactor = (isGrounded) ? 1f : 0f;
        rb.AddForce(apMoveForce * moveForce * rb.mass * moveFactor);

        // jump
        if (jump && (isGrounded || !delay1.ExpiredOrNotRunning(Runner)))
        {
            rb.AddForce(transform.up * jumpForce * rb.mass, ForceMode.Impulse);
        }

        // speed control
        if (isGrounded)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
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
        float yDiff = endPos.y - currentPos.y;
        Vector3 xzDiff = new Vector3(endPos.x - currentPos.x, 0f, endPos.z - currentPos.z);
        float tUp = Mathf.Sqrt(2 * maxHeight / gravity); 
        float tDown = Mathf.Sqrt(-2 * (yDiff - maxHeight) / (gravity * gravityMul));
        Vector3 vY = Vector3.up * Mathf.Sqrt(2 * gravity * maxHeight);
        Vector3 vXZ = xzDiff / (tUp + tDown);
        return vY + vXZ;
    }
    private void grappleGunShoot()
    {
        if (Runner.LagCompensation.Raycast(
                    myCam.transform.position,
                    myCam.transform.forward,
                    50f,
                    player: Object.InputAuthority,
                    out var hit,
                    -1,
                    HitOptions.IncludePhysX))
        {
            lrEndPoint = myCam.transform.position + myCam.transform.forward * hit.Distance;
            StartCoroutine(GrappleStateCO());
            rb.velocity = calculateGrappleVel(transform.position, lrEndPoint, lrEndPoint.y + yOverShoot);
        }
    }
    IEnumerator GrappleStateCO()
    {
        isGrappleGunShooting = true;
        yield return new WaitForSeconds(0.02f);
        isGrappleGunShooting = false;
    }
    IEnumerator GrappleEffectCO()
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
    }
    void grappleEffectWrarperFunc()
    {
        StartCoroutine(GrappleEffectCO());
    }
    static void OnGrappleGunShoot(Changed<RigidPlayer> changed)
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
        groundCheck();

        if (GetInput(out NetworkInputData data))
        {
            rotateFromMouseInput(data.mouseInput);
            moveFromInput(data.movInput, data._isJumpPressed);
            if (data._isMouse0Press && delay2.ExpiredOrNotRunning(Runner))
            {
                delay2 = TickTimer.CreateFromSeconds(Runner,shotDelay + 0.2f);
                grappleGunShoot();
            }
        }
    }
}
