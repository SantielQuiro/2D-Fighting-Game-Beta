using System.Collections;
using UnityEngine;

public class PlayerDefence : MonoBehaviour, IDamageable, IParryable
{

    public Animator anim;
    public HealthGeneric health;
    public ParticleSystem particParry, blood;
    public StaminaGeneric stamina;
    public GameObject pointCritic; //this is the red point that appears when the player is in critical state, it is a child of the player and is deactivated until the player enters critical state
    public AudioSource audioSource; //own audio source
    public AudioClip parrySound, BlockSound, StunnedCutSound;
    public bool isBlocking, Hurted, StunnedCritic, isParrying, isRecovering;
    public float StunTime, StartParryTime, StaminaToTake, StartCriticTime, StaminaToRecover;
    float _parryTimer, _criticTimer;
    IParryable enemyToParry; //Variable to save the enemy that is attacking to parry, check AttackFrameEnemy.cs / StrongFrameEnemy.cs for more info
    public void TryTakeDamage(int damage, string type)
    {
        
        if (_parryTimer > 0 && isBlocking) //Parry Condition
        {
            particParry.Play();
            audioSource.PlayOneShot(parrySound);
            ReadAttackerToParry(enemyToParry);     //Reads the enemy that is attacking to parry 
            enemyToParry.OnParried(StaminaToTake); // and takes stamina from it   
            return;
        }
        else if (_parryTimer == 0 && isBlocking) //Condición si es bloqueo normal y quitar estamina
        {
            audioSource.PlayOneShot(BlockSound);
            TakingStamina(StaminaToTake); //StaminaToTake can be set in the inspector, its set to 1 by default
            return;
        }

        anim.ResetTrigger("1attack");
        anim.ResetTrigger("2attack");
        anim.ResetTrigger("3attack");
        if (!StunnedCritic)
        {
            anim.SetTrigger("hurt"); 
        }

        Hurted = true; //Hurted state bool, it deactivates itself automatically from the Update after a few ms

        float finalDamage; //this will be the comprobation and result of the damage that the player will receive, it could be modified by the type of hit and the state of the player
        if (!StunnedCritic && type == "strong")
        {
            damage *= 2; //multiplication for a strong hit, 2 by default, but it could be a variable to set in the inspector
        }
        else if (StunnedCritic && type == "strong" || StunnedCritic && type == "light") //if the player recieves a hit while in critical state, it stands up again
        {
            blood.Play();
            audioSource.PlayOneShot(StunnedCutSound);
            damage *= 4;
            StartCoroutine(HitOnCritic());
            anim.SetBool("criticStun", false);
            anim.SetTrigger("hurt");
            _criticTimer = 0; //if the critic timer reaches 0 it returns to normal state, the boolean is deactivated in the update
            
        }

        finalDamage = damage;
        if (health != null)
        {
            TakingStamina(StaminaToTake);
            health.TakeDamage(finalDamage);
        }
    }

    private void Update()
    {
        if (Input.GetButton("Fire2") && !Hurted && !StunnedCritic) //Checks the block input
        {
            anim.SetBool("block", true);
            isBlocking = true;
        }
        else
        {
            anim.SetBool("block", false);
            isBlocking = false;
        }

        ParryTimer();//Handles the parry timer and conditions

        if (Hurted)
        {
            StartCoroutine(HurtedStunTime());
        }  //Start of the hurted state  


        // Timer to recover from the critical state 
        #region CriticTime 
        if (StunnedCritic)
        {
            _criticTimer -= Time.deltaTime;
        }

        if (_criticTimer <= 0.5f)
        {
            isRecovering = true;
            anim.SetBool("criticStun", false);
        }

        if (_criticTimer <= 0) //if the critic timer reaches 0, the player stands up again and recovers stamina
        {
            StunnedCritic = false;
            isRecovering = false;
            pointCritic.SetActive(false);
            stamina.RegenStamina(StaminaToRecover); //StaminaToRecover can be set in the inspector, its set to 7 by default (7 is the maximum stamina)
            _criticTimer = StartCriticTime;
        }

        #endregion  
    }

    void ParryTimer()
    {
        if (Input.GetButtonDown("Fire2")) //When the player presses the block button, the parry timer starts, allowing the player to parry for a short time
        {
            _parryTimer = StartParryTime;
        }
        else
        {
            _parryTimer -= Time.deltaTime;
        } 

        if(_parryTimer < 0) { _parryTimer = 0; }
        if (_parryTimer > 0) {isParrying = true; }
        else { isParrying = false; }
    } 

    public void OnParried(float stamiTake)
    {
        Hurted = true;
        anim.ResetTrigger("1attack");
        anim.ResetTrigger("2attack");
        anim.ResetTrigger("3attack");
        anim.SetTrigger("hurt");
        TakingStamina(stamiTake); //takes 1 stamina by default
    } //Public callable function that reacts when the player is parried

    public void ReadAttackerToParry(IParryable attacker)
    {
        if (attacker != null)
        {
            enemyToParry = attacker;
        }
    } //Public callable function to read the attacker and save it in a variable to parry it later, check AttackFrameEnemy.cs / StrongFrameEnemy.cs for more info
    public void TakingStamina(float stamiTake)//simple function to take stamina, makes it easier to call it from other functions and modify the amount of stamina taken in one place
    {
        stamina.LooseStamina(stamiTake);
    }

    public void CriticState() //Function that activates the critical state and continues starting the hit stop Coroutine
    {
        StunnedCritic = true;
        pointCritic.SetActive(true);
        anim.SetBool("block", false);
        anim.ResetTrigger("1attack");
        anim.ResetTrigger("2attack");
        anim.ResetTrigger("3attack");
        anim.SetTrigger("hurt");
        StartCoroutine(HitStop());
    }

    #region Subscriptions and Coroutines
    private void OnEnable()
    {
        stamina.OnStaminaZero += CriticState; //StaminaGeneric has an event that is called when the stamina reaches 0, activating the critical state
    }
    private void OnDisable()
    {
        stamina.OnStaminaZero -= CriticState;
    }
    public IEnumerator HurtedStunTime()
    {
        yield return new WaitForSeconds(StunTime); //Waits for the stun time before ending the hurted state
        Hurted = false;
    }
    public IEnumerator HitStop() //Hit Stop effect when the player enters critical state
    {
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(1f); //Hit stop time, could be a variable to set in the inspector, it is set to 1 second by default
        Time.timeScale = 1;
        anim.SetBool("criticStun", true);
    }
    public IEnumerator HitOnCritic() //Hit Stop effect when the player is hit during the critical state
    {
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(0.5f); //Hit stop time, could be a variable to set in the inspector, it is set to 0.5 seconds by default
        Time.timeScale = 1;
    }

    #endregion
}
