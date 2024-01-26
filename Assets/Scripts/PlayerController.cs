using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Attributes")]
    [Header("Movement")]
    [SerializeField] private float walkMaxSpeed;
    [SerializeField] private float runMaxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private bool conserverMomentum;

    [Header("Jump")]
    [SerializeField] private float coyoteTime;
    [SerializeField] private float jumpInputBufferTime;
    [SerializeField] private float jumpForce;

    [Header("Gravity")]
    [SerializeField] private float gravityScale;

    [Header("Fast Fall Gravity")]
    [Tooltip("This is For Making it come down Fast")]
    [SerializeField] private float fastFallGravityMult;
    [SerializeField] private float maxFastFallSpeed;

    [Header("FreeFall Gravity")]

    [Tooltip("This Time is for Free Fall")]
    [SerializeField] private float fallGravityMult;
    [SerializeField] private float maxFallSpeed;

    [Header("Jump Gravity")]

    [Tooltip("This is for Jump")]
    [SerializeField] private float jumpHangTimeThreshold;
    [SerializeField] private float jumpHangGravityMult;


    [SerializeField]private PlayerAnimation _playerAnimations;
    
    [SerializeField] private Rigidbody2D RB;
    
    private Vector2 _moveInput;


    [field: Header("PlayerStates")]
    [field: SerializeField]
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool isGrounded { get; private set; }
    
    //Jump
    private bool _isJumpCut;
    private bool _isJumpFalling;
    public bool _isRunning { get; private set; }


    //Timers
    public float LastOnGroundTime { get; private set; }
    public float LastPressedJumpTime { get; private set; }

    
    //Set all of these up in the inspector
    [Header("Checks")]
    [SerializeField] private Transform _groundCheckPoint;
    //Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);

    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer;
    
    // Start is called before the first frame update
    void Start()
    {
        IsJumping = false;
    }

    // Update is called once per frame
    void Update()
    {
        LastOnGroundTime -= Time.deltaTime;
        LastPressedJumpTime -= Time.deltaTime;

        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        if (_moveInput.x != 0)
            CheckDirectionToFace(_moveInput.x > 0);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJumpInput();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Running(true);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        { 
            Running(false);
        }

        if (!IsJumping)
        {
            if (isGrounded && !IsJumping)
            {
                LastOnGroundTime = coyoteTime;
            }
        }

        #region JUMP CHECKS

        if (IsJumping && RB.velocity.y < 0)
        {
            IsJumping = false;
            _isJumpFalling = true;
            
        }

        if (LastOnGroundTime > 0 && !IsJumping)
        {
            _playerAnimations.UpdateGrounded(true);
            if (!IsJumping)
                _isJumpFalling = false;
        }

        //Jump
        if (CanJump() && LastPressedJumpTime > 0)
        {
            IsJumping = true;
            _isJumpCut = false;
            _isJumpFalling = false;
            Jump();
        }
        #endregion

        #region GRAVITY

        if (RB.velocity.y < 0 && _moveInput.y < 0)
        {
            //Much higher gravity if holding down
            SetGravityScale(gravityScale * fastFallGravityMult);
            //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
            RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -maxFastFallSpeed));
        }
        else if ((IsJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < jumpHangTimeThreshold)
        {
            SetGravityScale(gravityScale * jumpHangGravityMult);
        }
        else if (RB.velocity.y < 0)
        {
            //Higher gravity if falling
            //SetGravityScale(gravityScale * fallGravityMult);
            //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
            //RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -maxFallSpeed));
            
        }
        else
        {
            //Default gravity if standing on a platform or moving upwards
            SetGravityScale(gravityScale);
        }

        _playerAnimations.UpdateGrounded(isGrounded);
        #endregion
    }
    private void FixedUpdate()
    { 
        Move(1);
        CheckGround();
    }

    private void Running(bool isRun)
    {
        _isRunning = isRun;
        _playerAnimations.SetRunBool(isRun);
        
    }
    private void Move(float lerpAmount)
    {
        float targetSpeed;
        
        if(_isRunning)
            targetSpeed = _moveInput.x * runMaxSpeed;
        else 
            targetSpeed = _moveInput.x * walkMaxSpeed;
        
        targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);

        float accelRate;

        accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        if (conserverMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) &&
            Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && 
            Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
        {
            accelRate = 0;
        }

        float speedDif = targetSpeed - RB.velocity.x;
        float movement= speedDif * accelRate;
    
        RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
        _playerAnimations.SetRunningSpeed((int)RB.velocity.x);
    }
    
    public void OnJumpInput()
    {
        LastPressedJumpTime = jumpInputBufferTime;
    }
    private void Jump()
    {
        //Ensures we can't call Jump multiple times from one press
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;

        #region Perform Jump
        //We increase the force applied if we are falling
        //This means we'll always feel like we jump the same amount 
        //(setting the player's Y velocity to 0 beforehand will likely work the same,but I find this more elegant :D)
        float force = jumpForce;
        if (RB.velocity.y < 0)
            force -= RB.velocity.y;

        
        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        _playerAnimations.SetJumpTrigger();
        #endregion
    }

    private void CheckGround()
    {
        if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
        
    }
    public void SetGravityScale(float scale)
    {
        RB.gravityScale = scale;
    }
    private bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Turn();
    }

    private void GoIdel()
    {
        _playerAnimations.SetIdelTrigger();
    }
    private void Turn()
    {
        //stores scale and flips the player along the x axis, 
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        IsFacingRight = !IsFacingRight;
    }
    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        // Gizmos.color = Color.blue;
        // Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
        //    / Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
    }
    #endregion
}
