using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    float speed = 5;
    FacingDirection facing;
    float apexHeight = 2;
    float apexTime = 0.5f;
    bool didWeJump = false;
    float gravity;
    public float terminalSpeed = -30;
    float coyoteTime = 0.5f;
    float coyoteTimeTimer = 0;
    public LayerMask ground;

    public float dashspeed = 1.5f;
    bool didWeDash = false;
    bool endDash = false;
    bool didWeShortJump = false;
    public float dashTimer = 0f;
    public float dashCooldown = 5f;
    float bounceHeight = 0.5f;
    float bounceTime = 0.2f;
    //bool recoil = false;
    public enum FacingDirection
    {
        left, right
    }
    public enum CharacterState
    {
        idle, walk, jump, die
    }
    public CharacterState currentstate;
    public CharacterState perviousstate;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
    }
    private void Update()
    {
        perviousstate = currentstate;

        if (OnDeath())
        {
            currentstate = CharacterState.die;
        }

        switch (currentstate)
        {
            case CharacterState.idle:
                if (IsWalking())
                {
                    currentstate = CharacterState.walk;
                }
                if (!IsGrounded())
                {
                    currentstate = CharacterState.jump;
                }
                break;

            case CharacterState.walk:
                if (!IsWalking())
                {
                    currentstate = CharacterState.idle;
                }
                if (!IsGrounded())
                {
                    currentstate = CharacterState.jump;
                }
                break;
            case CharacterState.jump:
                if (IsWalking())
                {
                    currentstate = CharacterState.walk;
                }
                else
                {
                    currentstate = CharacterState.idle;
                }
                break;

        }

        if (IsGrounded())
        {
            coyoteTimeTimer = 0;
        }
        else
        {
            coyoteTimeTimer += Time.deltaTime;
        }
        if (coyoteTimeTimer < coyoteTime  && Input.GetKeyDown(KeyCode.Space))
        {
            didWeJump = true;
            coyoteTimeTimer = 0;
        }
        if (Input.GetKeyUp(KeyCode.Space) && rb.velocity.y > 0)
        {
            didWeShortJump = true;
        }
        if (Input.GetKey(KeyCode.LeftShift) && dashTimer == 0f)
        {
            didWeDash = true;
            
            
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) || dashTimer >= 0.2f)
        {
            endDash = true;
            
        }

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.

        Vector2 playerInput = new Vector2();

        gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        rb.AddForce(new Vector2(0, gravity));

        MovementUpdate(playerInput);
        //Debug.Log("dashtime" + dashTimer);
        //Debug.Log("velocity=" + rb.velocity.y);
        //Debug.Log("moving" + IsWalking());
        //Debug.Log("grounded" + IsGrounded());
        //Debug.Log("right hit" + RightCollision());
        //Debug.Log("left hit" + LeftCollision());
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        playerInput.x = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(playerInput.x * speed, rb.velocity.y);

        if (didWeDash)
        {
            dashTimer += Time.deltaTime;
            
            float savedVelocity = rb.velocity.x;
            //rb.velocity =new Vector2(rb.velocity.x * dashspeed, rb.velocity.x);
            
            if(facing == FacingDirection.left)
            {
                rb.AddForce(Vector2.left * dashspeed, ForceMode2D.Impulse);
                Debug.Log("hehe");
            }
            if (facing == FacingDirection.right)
            {
                rb.AddForce(Vector2.right * dashspeed, ForceMode2D.Impulse);
            }
            StartCoroutine(endingDash(savedVelocity));
        }

        if(didWeJump)
        {
            float currentTime = Time.deltaTime;

            float jumpVelocity = 2 * apexHeight / apexTime;

            float velocity = gravity * currentTime + jumpVelocity;
            rb.velocity = new Vector2(rb.position.x, velocity);

            didWeJump = false;
        }
        //high jump and low jump mechanic
        if (didWeShortJump)
        {
            rb.velocity = new Vector2(rb.position.x, rb.velocity.y * 0.7f);
            Debug.Log("LongJump");
            didWeShortJump = false;
        }

        if (rb.velocity.y < terminalSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, terminalSpeed);
        }
        //dashing mechanic
        if (RightCollision() && didWeDash)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(Vector2.left * 100);
            Debug.Log("boing");
        }
        if (LeftCollision() && didWeDash)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(Vector2.right * 100);
            Debug.Log("boing");
        }
        //bounce after terminal speed mechanic
        StartCoroutine(Bounce());
        
    }

    public bool IsWalking()
    {
        if (rb.velocity.magnitude > 0.1f)
        {
            return true;
        }
        else
        {
            return false;
        }
            
    }
    public bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, 0.6f, ground);
        Debug.DrawRay(rb.position, Vector2.down * 0.6f);

        return hit.collider != null;
        
    }

    public FacingDirection GetFacingDirection()
    {
        
        if (rb.velocity.x < 0)
        {
            facing = FacingDirection.left;
        }
        if(rb.velocity.x > 0)
        {
            facing = FacingDirection.right;
        }
        return facing;
    }
    public bool RightCollision()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.right, 0.6f, ground);
        Debug.DrawRay(rb.position, Vector2.right * 0.6f);
        return hit.collider != null;

    }
    public bool LeftCollision()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.left, 0.6f, ground);
        Debug.DrawRay(rb.position, Vector2.left * 0.6f);
        return hit.collider != null;

    }
    IEnumerator Bounce()
    {
        if (IsGrounded() && rb.velocity.y == terminalSpeed)
        {
            float currentTime = Time.deltaTime;
            float prevPos = rb.position.y;
            float jumpVelocity = 2 * bounceHeight / bounceTime;

            float velocity = gravity * currentTime + jumpVelocity;
            rb.velocity = new Vector2(rb.position.x, velocity);
            yield return new WaitForSeconds(0.65f);
            //rb.velocity = Vector2.zero;
            rb.position = new Vector2(rb.position.x, prevPos);
        }
    }
    IEnumerator endingDash(float savVelocity)
    {
        if (endDash)
        {
            rb.velocity = new Vector2(savVelocity, rb.velocity.y);
            Debug.Log("enddash");
            endDash = false;
            didWeDash = false;
            yield return new WaitForSeconds(dashCooldown);
            dashTimer = 0;


        }
    }
}
