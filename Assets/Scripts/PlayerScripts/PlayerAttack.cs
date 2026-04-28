using System;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    float timeBtwAttack, comboTimeBtw, _cooldownparried; //Timers 
    public float startTimeBtwAttack, startComboTimer, CoolDownBeParried; //Time between attacks,combo duration, and cooldown to be parried
    public int Damage;
    public EnemyDefence enemydeff;
    public Animator animator;
    public PlayerDefence deff;
    public GameObject AttackPos, StrongAttackPos; //GameObjects that represent the position of the hitboxes for the light and strong attacks, they have a BoxCollider2D that is turned on and off in the animation events of the attack animations, check the animations for more info
    public bool Blocking, Hurted, stunnedCritic, isAttacking, isStrongAttacking, CoolDownToBeParried, isParried;
    [SerializeField] IParryable owner;

    void Update()
    {
        Blocking = deff.isBlocking;
        Hurted = deff.Hurted;
        stunnedCritic = deff.StunnedCritic;

        ComboAttack();  //General function for the light attack combo, it checks the conditions to start each attack and handles the timers for the combo
        StrongAttack(); //same as ComboAttack but for the strong attack
        
        if (Hurted)
        {
            StrongAttackPos.GetComponent<BoxCollider2D>().enabled = false;//little condition to avoid problems with the strong attack hitbox
        }

        owner = deff; //this is because the parry function is in the PlayerDefence script

        AttackBufferTime(); //a little buffer for the isAttacking bool, important for the enemy behavior, check EnemyContext.cs for more info
        ControlBeParried(); //Controls the cooldown for being parried, pretty important for the gameplay and the difficulty of the enemy
    }

    #region Light Attack Logic // Parried Cooldown

    public bool second, third;
    public float _attackBuffer, _strongattackBuffer;
    public float StartAttackBuffer, StrongAttackBuffer;

    void ControlBeParried() //Controls the cooldown for being parried
                            //If the player is parried, it starts a cooldown during which it cannot be parried again
                            //Pretty important for the gameplay and the difficulty of the enemy
                            //check EnemyContext.cs and EnemyDefence.cs for more info on how the enemy uses this cooldown to decide when to attack and when to parry
    {
        if (isParried)
        {
            _cooldownparried = CoolDownBeParried;
            isParried = false;
        }
        else
        {
            _cooldownparried -= Time.deltaTime;
        }
        

        while (_cooldownparried < 0)
        {
            _cooldownparried = 0;
        }

        if (_cooldownparried <= 0)
        {
           CoolDownToBeParried  = true;
        }
        else
        {
           CoolDownToBeParried  = false;
        }
    }
    void Parried() //Function that is called when the player is parried, it is subscribed to the Parried event from the EnemyDefence script
    {
        isParried = true;
    }
    void AttackBufferTime() //Pretty simple function that handles the buffer time for the isAttacking bool 
                            //isAttacking is used to control Enemy behavior, check EnemyContext.cs and BrainEnemy.cs for more info
    {
        if (Input.GetButtonDown("Fire1") && !stunnedCritic)
        {
            _attackBuffer = StartAttackBuffer;
        }
        else if (Input.GetButtonDown("Fire3") && !stunnedCritic)
        {
            _strongattackBuffer = StrongAttackBuffer;
        }
        else
        {
            _attackBuffer -= Time.deltaTime;
            _strongattackBuffer -= Time.deltaTime;
        }

        if (_attackBuffer > 0)
        {
            isAttacking = true;
        }
        else
        {
            isAttacking = false;
            AttackPos.GetComponent<BoxCollider2D>().enabled = false; //disables the attack hitbox to avoid overlaps between the collider and the animations
            StrongAttackPos.GetComponent<BoxCollider2D>().enabled = false;
        }

        if (_strongattackBuffer > 0)
        {
            isStrongAttacking = true;
        }
        else
        {
            isStrongAttacking = false;
            AttackPos.GetComponent<BoxCollider2D>().enabled = false; //disables the attack hitbox to avoid overlaps between the collider and the animations
            StrongAttackPos.GetComponent<BoxCollider2D>().enabled = false;
        }

        if (_attackBuffer <= 0)
        {
            _attackBuffer = 0;
        }
        if (_strongattackBuffer <= 0)
        {
            _strongattackBuffer = 0;
        }
    } 
    void ComboAttack()//General function for the attacks 
    {
        if (timeBtwAttack <= 0 && comboTimeBtw <= 0 && !second && !third) //first attack, allows to start the combo and gives access to the second attack
        {
            if (Input.GetButtonDown("Fire1") && !Blocking && !Hurted && !stunnedCritic)
            {
                animator.SetTrigger("1attack"); //The attack animations have events that turn on the hitbox in the right frames, check the animation for more info
                timeBtwAttack = startTimeBtwAttack;
                comboTimeBtw = startComboTimer; 
                second = true;
            }
        }
        else
        {
            timeBtwAttack -= Time.deltaTime;
            comboTimeBtw -= Time.deltaTime;
        }

        if(timeBtwAttack <= 0 && comboTimeBtw >= 0 && second && !third) // second attack, allows to start the third attack
        {
            if (Input.GetButtonDown("Fire1") && !Blocking && !Hurted && !stunnedCritic)
            {
                animator.SetTrigger("2attack");
                timeBtwAttack = startTimeBtwAttack;
                comboTimeBtw = startComboTimer;
                second = false;
                third = true;
            }
        }
        else
        {
            timeBtwAttack -= Time.deltaTime;
            comboTimeBtw -= Time.deltaTime;
        }

        if (timeBtwAttack <= 0 && comboTimeBtw >= 0 && third && !second) // third attack, the last one in the combo, after this it resets to the first attack
        {
            if (Input.GetButtonDown("Fire1") && !Blocking && !Hurted && !stunnedCritic)
            {
                animator.SetTrigger("3attack");
                timeBtwAttack = startTimeBtwAttack;
                third = false;
            }
        }
        else
        {
            timeBtwAttack -= Time.deltaTime;
            comboTimeBtw -= Time.deltaTime;
        }

        if(comboTimeBtw <= 0) { second = false; third = false; } //if the combo time runs out, it resets to the first attack
    }
    #endregion

    public void AttackFrameON()
    {
        AttackPos.GetComponent<BoxCollider2D>().enabled = true;
    } //turns the light attack hitbox on
    public void AttackFrameOFF()
    {
        AttackPos.GetComponent<BoxCollider2D>().enabled = false;
    } //turns the light attack hitbox off

    #region Strong Attack Logic

    public bool secondStrong = false;
    float _timeStrongAttack, _comboTimeStrongAttack;
    public float StartTimerStrongAttack, StartComboTimerStrongAttack;//time between strong attacks, time to reset the strong attack combo
    void StrongAttack()
    {
        
        if (_timeStrongAttack <= 0 && _comboTimeStrongAttack <= 0 && !secondStrong) //strong attack 1 that allows to start the second strong attack and the combo timer
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && !Blocking && !Hurted && !stunnedCritic)
            {
                animator.SetTrigger("strong1");
                _timeStrongAttack = StartTimerStrongAttack;
                _comboTimeStrongAttack = StartComboTimerStrongAttack;
                secondStrong = true;
            }
        }
        else
        {
            _timeStrongAttack -= Time.deltaTime;
            _comboTimeStrongAttack -= Time.deltaTime;
        }

        if (_timeStrongAttack <= 0 && _comboTimeStrongAttack >= 0 && secondStrong) // second strong attack, the last one in the combo, after this it resets to the first strong attack
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && !Blocking && !Hurted && !stunnedCritic)
            {
                animator.SetTrigger("strong2");
                _timeStrongAttack = StartTimerStrongAttack;
                _comboTimeStrongAttack = StartComboTimerStrongAttack;
                secondStrong = false;
            }
        }
        else
        {
            _timeStrongAttack -= Time.deltaTime;
            _comboTimeStrongAttack -= Time.deltaTime;
        }

        if (_comboTimeStrongAttack <= 0) { secondStrong = false; } //if the combo time runs out, it resets to the first strong attack

    }



    #endregion

    public void StrongAttackFrameON()
    {
        StrongAttackPos.GetComponent<BoxCollider2D>().enabled = true;
    } //turns the strong attack hitbox on 
    public void StrongAttackFrameOFF()
    { 
        StrongAttackPos.GetComponent<BoxCollider2D>().enabled = false;
    } //turns the strong attack hitbox off

    private void OnEnable()
    {
       enemydeff.hasParried += Parried; //Subscribes to the Parried event in the EnemyDefence script, so when the player is parried,
                                        //the Parried function is called, check EnemyDefence.cs for more info
    }
    private void OnDisable()
    {
        enemydeff.hasParried -= Parried; //Unsubscribes from the Parried event 
    }

}
