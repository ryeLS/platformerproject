using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    float speed = 5;
    FacingDirection facing;
    public enum FacingDirection
    {
        left, right
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.
        Vector2 playerInput = new Vector2();
        MovementUpdate(playerInput);
        //Debug.Log("moving" + IsWalking());
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        playerInput.x = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(playerInput.x * speed, rb.velocity.y);
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
        return true;
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
