using System;
using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("REFERENCES")]
    public Animator anim;
    public Rigidbody2D rb;
    public ParticleSystem particle;
    public HealthGeneric health;
    public StaminaGeneric stamina;
    public EnemyDefence Deff;
    public GameObject player, AttackPos, StrongAttackPos;
    public AudioSource audioSource;
    public AudioClip DashSound;

    [Header("STATS")]
    public int damage;
    public float speed, jumpForce, dashSpeed, dashDuration, minStateDuration, InvulnerableWindow, guardDistance;
    public float disViewPlayer = 5f; //Vision range, change the state to chase if the player is in this range
    public float disAttack = 2f; //Attack range, if the player is within this range, the enemy attacks

    [Header("STATES/BOOLS")]
    public bool Hurted;
    public bool Critic;
    public bool isGrounded;
    public bool blocking;
    public bool isDashing;

    [Header("NORMAL COMBO")]
    public bool second;
    public bool third;
    public float timeBtwAttack, startTimeBtwAttack, comboTimeBtw, startComboTimer; //variables to the combat system, time between attacks, time to reset the combo

    [Header("STRONG COMBO")]
    public bool secondStrong = false;
    float _timeStrongAttack, _comboTimeStrongAttack;
    public float StartTimerStrongAttack, StartComboTimerStrongAttack;//time between strong attacks, time to reset the strong attack combo

    
    float stateTimer; //timer to control the minimum time in each state, reset when changing state
    
    public enum EnemyState { Idle, Chase, Dashback, Attack, StrongAttack, Block, Jumping, Retire, Hurt, StunnedCritic} //States enumeration, 
    public EnemyState currentState, previousState; //callable enum variables to control the current state and the previous state for the timer reset

    void Update()
    {
        Hurted = Deff.Hurted;
        Critic = Deff.StunnedCritic;

        if (currentState != previousState) //if the state has changed, reset the flags and timers, then set the previous state to the current one for the next comparison
        {
            ResetStateFlags();
            previousState = currentState; //store the current state as the previous one for the next comparison
        }

        stateTimer += Time.deltaTime; //timer to control the minimum time in each state, reset when changing state

        switch (currentState) //StateTimer working
        {
            case EnemyState.Idle:
                if (stateTimer < minStateDuration)
                    break;
                IdleState();
                break;
            case EnemyState.Chase:
                if (stateTimer < minStateDuration)
                    break;
                ChaseState();
                break;
            /*case EnemyState.Dashback:
                if (stateTimer < minStateDuration)
                    break;
                DashingBack();*/
                //break;
            case EnemyState.Attack:
                AttackState();
                break;
            case EnemyState.StrongAttack:
                if (stateTimer < minStateDuration)
                    break;
                StrongAttack();
                break;  
            case EnemyState.Block:
                BLocking();
                break;  
            /*case EnemyState.Jumping:
                if (stateTimer < minStateDuration)
                    break;
                Jumping();*/
                //break;
            case EnemyState.Retire:
                if (stateTimer < minStateDuration)
                    break;
                RetireState();
                break;
            case EnemyState.Hurt:
                HurtState();
                break;
            case EnemyState.StunnedCritic:
                StunnedCritic();
                break;                                  //States, such as Attack, Block, Hurt and StunnedCritic, that don't need a minimum time to be effective,
                                                        //can skip the timer check to be more responsive, while the other states,
                                                        //that are more about movement and positioning,
                                                        //have the timer check to avoid constant state changing and make the behavior more natural
        }
    }
    void ResetStateFlags() //function to reset the flags and timers when the state changes, this is to avoid unwanted behavior when changing states
    {
        anim.SetBool("criticStun", false);
        anim.SetBool("block", false);
        blocking = false;
        AttackPos.GetComponent<BoxCollider2D>().enabled = false; //deactivates the attack hitbox to avoid unwanted damage when changing states,
                                                                 //the same for the strong attack hitbox
        StrongAttackPos.GetComponent<BoxCollider2D>().enabled = false;
        timeBtwAttack = 0;
        _timeStrongAttack = 0;
        _comboTimeStrongAttack = 0;
        rb.mass = 1;
    }
    public void ChangeState(EnemyState newState)
    {
        currentState = newState;
        stateTimer = 0; //resets the state timer when changing state to start counting the minimum time in the new state
    } //Simple function to change the enemy state from everywhere

    //Functions for each state, containing the logic and animations, called from the update depending on the current state
    #region States Functions & Executions & Animations 
    void IdleState()
    {
        anim.SetBool("criticStun", false);
        anim.SetBool("walking", false);
    }
    void ChaseState()
    {

        float directionX = 0f;
        if (player.transform.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);   // looking right
            directionX = 1f;   // moving right
        }
        else if (player.transform.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);   // looking left
            directionX = -1f;  // moving left
        }
        if(isDashing) return;
        anim.SetBool("walking", true);
        rb.linearVelocity = new Vector2(directionX * speed, rb.linearVelocity.y);
    }
    #region Dash Logic
    public void DashingBack()
    {
        if (!isDashing)
        {
            StartCoroutine(StartDash(1, -1));
        }
    }
    IEnumerator StartDash(float dir, float toLook) //Dash corroutine, dir is the direction of the dash, toLook is the direction the enemy will look during the dash
    {
        isDashing = true;
        audioSource.PlayOneShot(DashSound);

        float timer = dashDuration;
        float _vulnerableTimer = InvulnerableWindow;
        anim.SetTrigger("dash");    
        while (timer > 0)
        {
            while (_vulnerableTimer > 0) //invulnerability window at the start of the dash
            {
                Deff.isInvulnerable = true;
                _vulnerableTimer -= Time.deltaTime;
                yield return null;
            }
            rb.linearVelocity = new Vector2(dir * dashSpeed, rb.linearVelocity.y);
            transform.localScale = new Vector3(toLook, 1, 1);
            timer -= Time.deltaTime;
            yield return null;
        }

        Deff.isInvulnerable = false;
        isDashing = false;
        transform.localScale = new Vector3(1, 1, 1); //reset to the default looking direction, can be changed to look in the direction of the player if wanted
    }
    #endregion 
    void AttackState()
    {
        anim.SetBool("walking", false);

        if (timeBtwAttack <= 0 && comboTimeBtw <= 0 && !second && !third) //first attack of the combo that allows the second attack to be executed
        {
            timeBtwAttack = startTimeBtwAttack;
            comboTimeBtw = startComboTimer;
            anim.SetTrigger("1attack"); //trigger the first attack animation, the attack hitbox will be activated with an animation event in the animation itself to sync it with the animation, check the animation for reference
            second = true;
        }
        else
        {
            timeBtwAttack -= Time.deltaTime;
            comboTimeBtw -= Time.deltaTime;
        }

        if (timeBtwAttack <= 0 && comboTimeBtw >= 0 && second && !third) // second attack that allows the third attack
        {
            timeBtwAttack = startTimeBtwAttack;
            comboTimeBtw = startComboTimer;
            anim.SetTrigger("2attack");
            second = false;
            third = true;
        }
        else
        {
            timeBtwAttack -= Time.deltaTime;
            comboTimeBtw -= Time.deltaTime;
        }

        if (timeBtwAttack <= 0 && comboTimeBtw >= 0 && third && !second) //third attack that closes the combo and goes back to the first attack
        {
            timeBtwAttack = startTimeBtwAttack;
            comboTimeBtw = startComboTimer;
            anim.SetTrigger("3attack");
            third = false;
        }
        else
        {
            timeBtwAttack -= Time.deltaTime;
            comboTimeBtw -= Time.deltaTime;
        }

        if (comboTimeBtw <= 0) { second = false; third = false; } //if the combo timer runs out, resets the combo to the first attack
    } 
    void StrongAttack()
    {
        anim.SetBool("walking", false);

        if (_timeStrongAttack <= 0 && _comboTimeStrongAttack <= 0 && !secondStrong) //strong attack 1, works the same as the normal attack but with different timers and flags to not interfere with the normal attack combo
        {
            
            
            anim.SetTrigger("strong1");
            _timeStrongAttack = StartTimerStrongAttack;
            _comboTimeStrongAttack = StartComboTimerStrongAttack;
            secondStrong = true;
            
        }
        else
        {
            _timeStrongAttack -= Time.deltaTime;
            _comboTimeStrongAttack -= Time.deltaTime;
        }

        if (_timeStrongAttack <= 0 && _comboTimeStrongAttack >= 0 && secondStrong) // strong attack 2 and the end of the strong attack combo
        {
            
            anim.SetTrigger("strong2");
            _timeStrongAttack = StartTimerStrongAttack;
            _comboTimeStrongAttack = StartComboTimerStrongAttack;
            secondStrong = false;
            
        }
        else
        {
            _timeStrongAttack -= Time.deltaTime;
            _comboTimeStrongAttack -= Time.deltaTime;
        }

        if (_comboTimeStrongAttack <= 0) { secondStrong = false; } //if the strong attack combo timer runs out, resets the strong attack combo to the first strong attack
    }
    void BLocking()
    {
        anim.SetBool("block", true);
        anim.SetBool("walking", false);
        blocking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); //stop the enemy movement when blocking, this is to avoid sliding,
                                                                 //can be changed to allow movement while blocking if wanted
    }
    void RetireState()
    {
        anim.SetBool("walking", true);
        float directionX = 0f;
        if (player.transform.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);   
            directionX = -1f;   // moves left
        }
        else if (player.transform.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);   
            directionX = 1f;  // moves right
        }
        rb.linearVelocity = new Vector2(directionX * speed, rb.linearVelocity.y); //this function works the same as the chase state but in reverse,
                                                                                  //the enemy moves away from the player instead of towards him,
                                                                                  //this is to create some distance between the enemy and the player
                                                                                  //when the enemy is recovering stamina

    }
    /*void Jumping()
    {
        if (Input.GetKeyDown(KeyCode.I) && isGrounded) //provisional input
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("jumping");
        }
    }*/
    void HurtState()
    {
        anim.ResetTrigger("1attack");
        anim.ResetTrigger("2attack");
        anim.ResetTrigger("3attack");
        anim.SetBool("walking", false); //it just handle the animations when getting hurt,
                                        //the actual damage and stun is handled in the EnemyDefence script
    }
    void StunnedCritic()
    {
        rb.mass = 1000000; //makes the enemy unmovable when in critical stun
        anim.ResetTrigger("1attack");
        anim.ResetTrigger("2attack");
        anim.ResetTrigger("3attack");
        anim.SetBool("criticStun", true);
    }
    #endregion 

    #region Callbacks, Suscriptions, Attack Frames and Ground Check
    public void OnHurted()
    {
        ChangeState(EnemyState.Hurt);
    } //function called from the EnemyDefence event called "Parried" that changes the state to hurt
    public void OnCriticStun()
    {
        ChangeState(EnemyState.StunnedCritic);
    } //function called from the StaminaGeneric event called "OnStaminaZero" that changes the state to stunned critic
    private void OnEnable()
    {
        health.OnDamaged += OnHurted;
        Deff.Parried += OnHurted;
        stamina.OnStaminaZero += OnCriticStun;
    } //suscriptions to the events that changes the state of the enemy in each situation
    private void OnDisable()
    {
        health.OnDamaged -= OnHurted;
        Deff.Parried -= OnHurted;
        stamina.OnStaminaZero -= OnCriticStun;
    } 
    
    public void AttackFrameON()
    {
        AttackPos.GetComponent<BoxCollider2D>().enabled = true;
    }
    public void AttackFrameOFF()
    {
        AttackPos.GetComponent<BoxCollider2D>().enabled = false;
    }
                                                           //Functions to activate and deactivate the attack hitbox of each attack,
                                                           //these functions are called by the animations to make damage on the correct frame
    public void StrongAttackFrameON()
    {
        StrongAttackPos.GetComponent<BoxCollider2D>().enabled = true;
    }
    public void StrongAttackFrameOFF()
    {
        StrongAttackPos.GetComponent<BoxCollider2D>().enabled = false;
    }

    // Floor verification
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "floor")
        {
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
            anim.SetBool("walking", false);
            isGrounded = false;
        }

    }
    #endregion
}
