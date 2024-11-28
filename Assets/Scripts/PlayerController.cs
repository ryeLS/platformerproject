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
    public enum FacingDirection
    {
        left, right
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
    }
    private void Update()
    {
        if (IsGrounded() && Input.GetKeyDown(KeyCode.Space))
        {
            didWeJump = true;
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

        //Debug.Log("moving" + IsWalking());
        //Debug.Log("grounded" + IsGrounded());
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        playerInput.x = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(playerInput.x * speed, rb.velocity.y);

        if(didWeJump)
        {
            float currentTime = Time.deltaTime;

            Debug.Log("gravity:"+ gravity);
            float jumpVelocity = 2 * apexHeight / apexTime;

            float velocity = gravity * currentTime + jumpVelocity;
            rb.velocity = new Vector2(rb.position.x, velocity);

            didWeJump = false;
        }
    }

    public bool IsWalking()
    {
        if (rb.velocity.magnitude > 0.001f)
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
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, 0.01f);
        if (hit)
        {
            return true;
        }
        else
        {
            return false;
        }
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
}
