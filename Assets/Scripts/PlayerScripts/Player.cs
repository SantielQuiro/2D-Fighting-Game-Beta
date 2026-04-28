using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.Windows;

public class Player : MonoBehaviour
{
    public float speed = 5f; 
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    public Animator animator;
    public PlayerDefence deff;
    public AudioSource audioSource;
    public AudioClip JumpSound, DashSound;

    public float lastTapTime, lastTapDirection, doubleTapWindow, dashSpeed, dashDuration;
    public bool isDashing;

    public bool isGrounded; //bool to verify if the player is touching the ground
    Vector2 movementToView; //player's movement direction (turns left or right)

    bool KeySpace;
    float moveInput, jumpBufferTime = 0.15f, jumpBufferCounter, targetVelocityX;
    void Update()
    {
        if (deff.Hurted || deff.StunnedCritic) // sin movimiento si está aturdido o recibiendo daño
        {
            targetVelocityX = 0f;
            return;
        }
        else if (!isDashing) // normal movement if not dashing
        {
            targetVelocityX = moveInput * speed;
        }
        moveInput = Input.GetAxisRaw("Horizontal"); //takes horizontal input for movement

        if (Input.GetButtonDown("Jump"))
        {
            KeySpace = true;
            jumpBufferCounter = jumpBufferTime;// buffer time for the jump, allows jumping even if not touching the ground in the same frame
        }
        if (jumpBufferCounter > 0) //resets buffer counter if the player doesn't jump within the buffer time
        {
            jumpBufferCounter -= Time.deltaTime;
        }

       if (Input.GetKeyDown(KeyCode.A))
        {
            HandleDash(-1);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            HandleDash(1);
        }

        #region Player's direction to view  
        movementToView.x = Input.GetAxisRaw("Horizontal");
        if (movementToView.x > 0)
        {
            if (isGrounded)
            {
                animator.SetBool("walking", true);
            }
            transform.localScale = new Vector3(1, 1, 1);   // mirando derecha
        }
        else if (movementToView.x < 0)
        {
            if (isGrounded)
            {
                animator.SetBool("walking", true);
            }
            transform.localScale = new Vector3(-1, 1, 1);  // mirando izquierda
        }
        else
        {
            animator.SetBool("walking", false);
        }
        #endregion
    }

    void FixedUpdate()
    {
        ApplyMovement(); //Aplies movement every fixed frame after checking for input in the update


        if (jumpBufferCounter > 0 && isGrounded) //jump condition
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("jumping");
            audioSource.PlayOneShot(JumpSound);
            jumpBufferCounter = 0;
        }
    }
    void HandleDash(float dir) //Handle the dash input
    {
        float currentTime = Time.time;
        bool withinTime = currentTime - lastTapTime <= doubleTapWindow;
        bool sameInput = dir == lastTapDirection;

        if(withinTime && sameInput)
        {
            StartCoroutine(StartDash(dir)); //corrutina del dash
            lastTapTime = 0f;
        }
        else //guardar el último toque como si fuera el primero
        {
            lastTapDirection = dir;
            lastTapTime = currentTime;
        }

    }
    IEnumerator StartDash(float dir) //Handle the dash time and animation, sets the general velocity for the dash and resets it after the dash duration is over
    {
        isDashing = true;
        audioSource.PlayOneShot(DashSound);
        targetVelocityX = dir * dashSpeed;

        animator.SetTrigger("dash");
        animator.SetBool("walking", false);
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        animator.ResetTrigger("dash");
    }
    void ApplyMovement()
    {
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
        rb.velocity = new Vector2(targetVelocityX * speed, rb.linearVelocity.y);
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos

    } //basic movement for velocity

    // Verify if the player is touching the ground
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "floor")
        {
            animator.SetTrigger("onGround");
            isGrounded = true;
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.tag == "floor")
        {
            isGrounded = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.tag == "floor")
        {
            animator.SetBool("walking", false);
            isGrounded = false;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
}

