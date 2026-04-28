using System.Collections;
using UnityEngine;

public class EnemyDefence : MonoBehaviour, IDamageable, IParryable
{
    public event System.Action Parried; //Event to activate the consecuences of a successful parry in the EnemyAI script, check EnemyAI.cs for more info
    public event System.Action hasParried;

    public Animator anim;
    public HealthGeneric health;
    public StaminaGeneric stamina;
    public GameObject pointCritic, player; //PointCritic is a visual indicator that the enemy is in a critical state
    public ParticleSystem blood, parry;
    public EnemyAI enemyAi; //reference to the EnemyAI functions script
    public EnemyContext context; //reference to the context script
    public AudioSource audioSource;//own audio source
    public AudioClip parrySound, blockSound, StunnedCutSound;
    public bool isBlocking, Hurted, StunnedCritic, isInvulnerable;
    public float stunTime, staminaToTake, StartCriticTime, stamiToRecover, StartParryTime;
    float _parryTimer, _criticTimer;
    IParryable enemyToParry; //Variable to save the enemy that is attacking to parry, check AttackFramePlayer.cs / StrongFramePlayer.cs for more info

    public void TryTakeDamage(int damage, string type)
    {
        if(isInvulnerable) return; //little checker to avoid taking damage when the enemy is invulnerable

        if (_parryTimer > 0 && isBlocking) //parry condition
        {
            if (context.CanParry) // condition to check if the enemy can parry, check EnemyContext.cs for more info
            {
                parry.Play();
                audioSource.PlayOneShot(parrySound);
                hasParried?.Invoke(); //Invoke the hasParried event, which is used in the PlayerAttack script to activate the cooldown for being parried, check PlayerAttack.cs for more info
                ReadAttackerToParry(enemyToParry);    //Reads the player that is attacking to parry
                enemyToParry.OnParried(staminaToTake);// and takes stamina from it
            }
            else
            {
                audioSource.PlayOneShot(blockSound);
                TakeStamina(); //if the parry timer is active but the enemy can't parry, it just blocks and takes stamina
            }
            return;
        }
        else if (_parryTimer == 0 && isBlocking) //if its a normal block and takes stamina
        {
            audioSource.PlayOneShot(blockSound);
            TakeStamina();
            return;
        }
        anim.ResetTrigger("1attack");
        anim.ResetTrigger("2attack");
        anim.ResetTrigger("3attack");
        if (!StunnedCritic)
        {
            anim.SetTrigger("hurt"); //activate the hurt animation if the enemy is not in critical state
        }

        Hurted = true; //Hurted state bool, it deactivates itself automatically from the Update after a few ms

        float finalDamage; //this will be the comprobation and result of the damage that the enemy will receive, it could be modified by the type of hit and the state of the enemy
        if (!StunnedCritic && type == "strong")
        {
            damage *= 2; //multiplication for a strong hit, 2 by default, but it could be a variable to set in the inspector
        }
        else if (StunnedCritic && type == "strong" || StunnedCritic && type == "light") // if the enemy receives a hit while in critical state, it stands up again
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
            TakeStamina();
            health.TakeDamage(finalDamage);
        }
    }

    private void Update()
    {
        isBlocking = enemyAi.blocking; //takes the value of the enemy's blocking state to use it in defense

        if (Hurted)
        {
            StartCoroutine(HurtTime()); //Coroutine to deactivate the Hurted state after a few seconds, it can be set in the inspector with the stunTime variable
        }

        ParryTimer(); //handles the parry timer and conditions
        //Timer to recover from the critical state
        #region CriticTime 
        if (StunnedCritic)
        {
            _criticTimer -= Time.deltaTime;
        }

        if (_criticTimer <= 0.5f)
        {
            anim.SetBool("criticStun", false);
        }

        if (_criticTimer <= 0) //if the timer reaches 0, it recovers from the critical state and stands up again, recovering stamina in the process
        {
            StunnedCritic = false;
            pointCritic.SetActive(false);
            stamina.RegenStamina(stamiToRecover); //StaminaToRecover can be set in the inspector, its set to 7 by default (7 is the maximum stamina)
            _criticTimer = StartCriticTime;

            if (player.transform.position.x > transform.position.x) //turns the enemy to face the player when it stands up again 
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (player.transform.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }

        #endregion 
    }

    public void ParryTimer() 
    {
        if (isBlocking && _parryTimer <= 0 ) //When the enemy starts blocking, the parry timer starts
        {
            _parryTimer = StartParryTime;
        }
        else
        {
            _parryTimer -= Time.deltaTime;
        }

        while (_parryTimer < 0)
        {
             _parryTimer = 0;
        }
    }

    public void TakeStamina()
    {
        stamina.LooseStamina(staminaToTake);
    } //Little Function to take stamina easily from everywhere

    public void OnParried(float stamiTake)
    {
        Hurted = true; //Hurted state, it deactivates itself automatically from the Update after a few ms
        Parried?.Invoke();
        anim.ResetTrigger("1attack");
        anim.ResetTrigger("2attack");
        anim.ResetTrigger("3attack");
        anim.SetTrigger("hurt");
        stamina.LooseStamina(staminaToTake);
    } //Activates when the enemy is parryed
    
    void CriticState()
    {
        StunnedCritic = true;
        pointCritic.SetActive(true);
        anim.ResetTrigger("1attack");
        anim.ResetTrigger("2attack");
        anim.ResetTrigger("3attack");
        anim.SetTrigger("hurt");
        StartCoroutine(HitStop());
    } //Function that is activated from the StaminaZero event, check StaminaGeneric.cs 

    public void ReadAttackerToParry(IParryable attacker)
    {
        if (attacker != null)
        {
            enemyToParry = attacker;
        }
    } //little function that reads the attacker to parry and stores it in enemyToParry

    #region Suscriptions and Coroutines 
    public IEnumerator HurtTime()
    {
        yield return new WaitForSeconds(stunTime);
        Hurted = false;
    } //Simple Coroutine to deactivate the Hurted state after a few seconds, it can be set in the inspector with the stunTime variable
    public IEnumerator HitStop() //Start of the critical state, with a hit stop effect to make it more impactful
    {
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(1f); //Hit stop time, could be a variable to set in the inspector, it is set to 1 second by default
        Time.timeScale = 1;
        anim.SetBool("criticStun", true);
    }
    public IEnumerator HitOnCritic() //Executed when the enemy receives a hit while in critical state
    {
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1;
    }

    private void OnEnable()
    {
        stamina.OnStaminaZero += CriticState;
    }
    private void OnDisable()
    {
        stamina.OnStaminaZero -= CriticState;
    } //Subscriptions to the StaminaZero event, which is activated when the enemy's stamina reaches 0, check StaminaGeneric.cs for more info

    #endregion
}
