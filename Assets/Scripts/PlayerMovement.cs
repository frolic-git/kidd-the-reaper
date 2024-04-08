using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;
    public bool locked;

    [Header("Movement")]
    public float moveSpeed;
    public float groundDrag;
    public float walkSpeed;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float fallMultiplier;

    public float dashForce;
    public float dashUpForce;
    public float dashSpeed;
    public float dashSpeedChangeFactor;
    public float dashCD;
    public float dashDuration;
    private float dashCDTimer;
    private bool dashing;

    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey;
    public KeyCode lightAttack;
    public KeyCode heavyAttack;
    public KeyCode lockTarget;
    public KeyCode dashKey;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;
    public Transform currentLockOn;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public enum MovementState
    {
        walking,
        dashing,
        air
    }
    public MovementState state;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        locked = false;
    }

    // Update is called once per frame
    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        if (grounded) { state = MovementState.walking; }
        else { state = MovementState.air; }
        //print(grounded);

        MyInput();
        StateHandler();
        //SpeedControl();

        // handle drag
        if (state == MovementState.walking)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;

    private void StateHandler()
    {
        if (dashing) 
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
            //moveSpeed = walkSpeed;
        }
        else if (state == MovementState.walking) 
        {
            //moveSpeed = dashSpeed;
            desiredMoveSpeed = walkSpeed;
        }
        else 
        {
            state = MovementState.air;

            desiredMoveSpeed = walkSpeed;
        }

        bool desieredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing) keepMomentum = true;

        if (desieredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
        if (Input.GetKeyDown(lockTarget))
        {
            if (locked == false)
            {
                ClosestTarget();
                locked = true;
            }
            else
            {
                locked = false;
                currentLockOn = null;
            }
        }
        if (Input.GetKey(dashKey))
        {
            Dash();
        }
        if (dashCDTimer > 0 ) { dashCDTimer -= Time.deltaTime; }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
   
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.x, limitedVel.z);
        }

        if (state == MovementState.air)
        {
            float currentYVel = rb.velocity.y;
            if (currentYVel < 0f)
            {
        
                rb.velocity += Vector3.Scale(Physics.gravity, Vector3.up) * Time.deltaTime * (fallMultiplier - 1);
            }
        }
    }

    private float speedChangeFactor;

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float diff = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < diff)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / diff);

            time += Time.deltaTime * boostFactor;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }


    private void Jump()
    {
        // Reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void ClosestTarget()
    {
        GameObject[] gameObjects;
        gameObjects = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float distance = 1000f;
        //float currAngle = 45f;
        Vector3 position = transform.position;

        foreach (GameObject go in gameObjects)
        {
            Vector3 diff = go.transform.position - position;
            float currDistance = diff.magnitude;
            if (currDistance < distance)
            {
                closest = go;
            }
        }

        currentLockOn = closest.transform;
    }

    private void Dash()
    {
        if (dashCDTimer > 0) return;
        else dashCDTimer = dashCD;

        dashing = true;
        rb.useGravity = false;

        //Vector3 straight = new Vector3(orientation.forward.x, 0.0f, orientation.forward.z);
        //Vector3 forceToApply = straight * dashForce + orientation.up * dashUpForce;
        Vector3 forceToApply = orientation.forward * dashForce + orientation.up * dashUpForce;

        print(forceToApply);
        delayedForceToApply = forceToApply;


        Invoke(nameof(DelayedDashForce), 0.025f);
        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    {
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        dashing = false;
        rb.useGravity = true;
    }
}
