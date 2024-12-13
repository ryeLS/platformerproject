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
    public float bounceHeight = 0.5f;
    public float bounceTime = 0.2f;

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
        switch (currentstate)
        {//code for idle, jump and walking states
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
        //coyote time only counts when the player isn't grounded
        if (IsGrounded())
        {
            coyoteTimeTimer = 0;
        }
        else
        {
            coyoteTimeTimer += Time.deltaTime;
        }// allows input for jumping. lets player jump when coyote timer is less than coyote timer
        if (coyoteTimeTimer < coyoteTime  && Input.GetKeyDown(KeyCode.Space))
        {
            didWeJump = true;
            coyoteTimeTimer = 0;
        }
        if (Input.GetKeyUp(KeyCode.Space) && rb.velocity.y > 0)
        {//when space is let go and player is jumping
            didWeShortJump = true;
        }
        if (Input.GetKey(KeyCode.LeftShift) && dashTimer == 0f)
        {//lets player dash when left shift is pressed.
            didWeDash = true;
            
            
        }//when left shift is lifted or timer ends, dash ends.
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
        //calculates gravity
        gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        //applies gravity to player
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

        //if player dashes
        if (didWeDash)
        {
            //start timer
            dashTimer += Time.deltaTime;
            //stores previous velocity
            float savedVelocity = rb.velocity.x;
            
            if(facing == FacingDirection.left)//dash when facing left
            {
                rb.AddForce(Vector2.left * dashspeed, ForceMode2D.Impulse);
                Debug.Log("hehe");
            }
            if (facing == FacingDirection.right)//dash when facing right
            {
                rb.AddForce(Vector2.right * dashspeed, ForceMode2D.Impulse);
            }
            StartCoroutine(endingDash(savedVelocity));
        }
        //if player jumps
        if(didWeJump)
        {
            //gets time
            float currentTime = Time.deltaTime;
            //finds jump velocity
            float jumpVelocity = 2 * apexHeight / apexTime;
            //finds y velocity of player
            float velocity = gravity * currentTime + jumpVelocity;
            rb.velocity = new Vector2(rb.position.x, velocity);
            //resets boolean to false
            didWeJump = false;
        }
        //high jump and low jump mechanic
        if (didWeShortJump)
        {
            rb.velocity = new Vector2(rb.position.x, rb.velocity.y * 0.7f);//lowers y velocity
            Debug.Log("LongJump");
            didWeShortJump = false;//resets boolean
        }
        // Fixes terminal speed. makes player not gain speed when falling after a certain speed.
        if (rb.velocity.y < terminalSpeed) 
        {
            rb.velocity = new Vector2(rb.velocity.x, terminalSpeed);
        }
        //dashing mechanic's recoil
        if (RightCollision() && didWeDash)//bounces left
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(Vector2.left * 100);
            Debug.Log("boing");
        }
        if (LeftCollision() && didWeDash)//bounces right
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
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, 0.2f, ground);//casts raycast down
        Debug.DrawRay(rb.position, Vector2.down * 0.01f);

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
        Debug.DrawRay(rb.position, Vector2.right * 0.6f);//ray to the right
        return hit.collider != null;

    }
    public bool LeftCollision()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.left, 0.6f, ground);
        Debug.DrawRay(rb.position, Vector2.left * 0.6f);//ray to the left
        return hit.collider != null;

    }
    IEnumerator Bounce()//bouncing mechanic coroutine
    {
        if (IsGrounded() && rb.velocity.y == terminalSpeed)//if grounded and at max fall speed
        {
            //jump logic
            float currentTime = Time.deltaTime;
            float prevPos = rb.position.y;
            float jumpVelocity = 2 * bounceHeight / bounceTime;

            float velocity = gravity * currentTime + jumpVelocity;
            rb.velocity = new Vector2(rb.position.x, velocity);
            yield return new WaitForSeconds(0.65f);//wait out bounces
            //rb.velocity = Vector2.zero;
            rb.position = new Vector2(rb.position.x, prevPos);//makes player grounded again
        }
    }
    IEnumerator endingDash(float savVelocity)//ends dash (takes the velocity of player before dashing)
    {
        if (endDash)
        {// makes velocity the velocity of player before dashing
            rb.velocity = new Vector2(savVelocity, rb.velocity.y);
            Debug.Log("enddash");
            endDash = false;//resets both booleans
            didWeDash = false;
            yield return new WaitForSeconds(dashCooldown);//waits out cooldown
            dashTimer = 0;// resets timer


        }
    }
}
