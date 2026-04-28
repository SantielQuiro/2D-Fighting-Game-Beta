using UnityEngine;
using System.Collections;
using static EnemyAI;

public class BrainEnemy : MonoBehaviour
{
    public EnemyContext context;
    public EnemyAI States;

    public float decisionCooldown, blockChance, strongAttackChance, strongAttackDuration, guardTime, RetireChance;
    float _decisionTimer, _strongattacktimer, _guardtimer;
    //StrongAttackDuration is the time that the strong attack state will last,
    //to avoid the AI switching to another state before the strong attack animation is finished

    bool dashExecuted, gotLowHealth = false; //flag bools to avoid repeating actions 

    bool CouldBlock(float chance) //method to calculate the probability of blocking based on a random generation
    {
        return Random.value < chance;
    }
    bool CouldStrongAttack(float chance) //method to calculate the probability of a strong attack based on a random generation
    {
        return Random.value < chance;
    }
    bool CouldRetire(float chance) //method to calculate the probability of retiring based on a random generation
    {
        return Random.value < chance;
    }

    private void Update()
    {
        _decisionTimer += Time.deltaTime;
        if (_decisionTimer < decisionCooldown) return; //timer to control how often the AI takes decisions,
                                                       //to avoid it being too reactive and to give the player a chance to react to the enemy's actions.
        _decisionTimer = 0f;

        if (context.selfCanAct)
        {
            TakeDecisions();
        }

        TimersAndProbabilities();//function to update the timers and probabilities based on the current state of the enemy and the player,
                                 //to ensure that the AI's decisions are based on the most up-to-date information and to create a more dynamic and responsive AI behavior.
    }
    void TimersAndProbabilities()
    {
        if (_strongattacktimer <= 0)
        {
            _strongattacktimer = 0;
        }
        else
        {
            _strongattacktimer -= Time.deltaTime;
        }

        if (States.Hurted)
        {
            _strongattacktimer = 0; //if the enemy is hurt, it will cancel the strong attack timer,
                                    //to avoid the AI being stuck in the strong attack state if it gets interrupted by the player
        }


        if (_guardtimer <= 0) { _guardtimer = 0; }
        else { _guardtimer -= Time.deltaTime; }

        if (context.selfLowHealth && !gotLowHealth) //if the enemy is in low health and it hasn't already activated the low health behavior,
                                                    //it will increase the chances to retire and to block, making the AI more defensive
        {
            RetireChance += 0.5f;
            blockChance += 0.6f;
            gotLowHealth = true;
        }
    }
    void TakeDecisions() //Main function of the AI, where conditions are evaluated and decisions are made
    {                    //based on the current state of the enemy and the player,
                         //as well as the probabilities defined for each action.
                         //Thanks to the EnemyContext, the AI can make informed decisions based on the player's position,
                         //actions, and the enemy's own status, besides the code being organized in a way that is easy to read and understand
                         //(Dont forget that the random values (CouldBlock, CouldStrongAttack, CouldRetire) are setted here, from the inspector).
        switch (States.currentState)
        {
            case EnemyAI.EnemyState.Idle:
                if (context.playerInVisionRange)
                {
                    States.ChangeState(EnemyAI.EnemyState.Chase);
                }
                if (context.playerInVisionRange && context.selfLowStamina && CouldRetire(RetireChance))
                {
                    States.ChangeState(EnemyAI.EnemyState.Retire);
                }
               break;

            case EnemyAI.EnemyState.Chase:
                if (!context.playerInVisionRange)
                {
                    States.ChangeState(EnemyAI.EnemyState.Idle);
                }
                if (context.playerInAttackRange)
                {
                    if (CouldStrongAttack(strongAttackChance))
                    {
                            _strongattacktimer = strongAttackDuration;
                            States.ChangeState(EnemyAI.EnemyState.StrongAttack);
                    }
                    else
                    {
                            States.ChangeState(EnemyAI.EnemyState.Attack);
                    }
                }
                if (context.selfInDanger)
                {
                    States.ChangeState(EnemyAI.EnemyState.Block);
                }
                break;

            case EnemyAI.EnemyState.Attack:
                if (context.playerInAttackRange && CouldStrongAttack(strongAttackChance))
                {
                    _strongattacktimer = strongAttackDuration;
                    States.ChangeState(EnemyAI.EnemyState.StrongAttack);
                }
                if (context.selfLowStamina && CouldRetire(RetireChance))
                {
                    States.ChangeState(EnemyAI.EnemyState.Retire);
                }
                if (context.selfInDanger && CouldBlock(blockChance))
                {
                    States.ChangeState(EnemyAI.EnemyState.Block);
                }
                else if (context.selfInDanger && !CouldBlock(blockChance)) //if cannot block, try to dashback, if it cannot dashback, it will try to block again on the next decision cycle
                {
                    States.ChangeState(EnemyAI.EnemyState.Dashback);
                }
                if (!context.playerInAttackRange)
                {
                    States.ChangeState(EnemyAI.EnemyState.Chase);
                }
                break;

            case EnemyAI.EnemyState.StrongAttack:

                if (context.selfLowStamina && CouldRetire(RetireChance))
                {
                    States.ChangeState(EnemyAI.EnemyState.Retire);
                }
                if (context.selfInDanger && CouldBlock(blockChance))
                {
                    States.ChangeState(EnemyAI.EnemyState.Block);
                }
                else if (context.selfInDanger && !CouldBlock(blockChance))
                {
                    States.ChangeState(EnemyAI.EnemyState.Dashback);
                }
                if (context.playerInAttackRange && !CouldStrongAttack(strongAttackChance) && _strongattacktimer == 0)
                {
                    States.ChangeState(EnemyAI.EnemyState.Attack);
                }
                if (context.playerStrongAttacking && context.playerInAttackRange && _strongattacktimer == 0)
                {
                    States.ChangeState(EnemyAI.EnemyState.Attack);
                }
                if (!context.playerInAttackRange)
                {
                    States.ChangeState(EnemyAI.EnemyState.Chase);
                }
                break;

            case EnemyAI.EnemyState.Retire:
                if (!context.playerInVisionRange)
                {
                    States.ChangeState(EnemyAI.EnemyState.Idle);
                }
                if (!context.playerInVisionRange && !context.selfLowStamina)
                {
                    States.ChangeState(EnemyAI.EnemyState.Idle);
                }
                if (context.playerInVisionRange && !context.selfLowStamina)
                {
                    States.ChangeState(EnemyAI.EnemyState.Chase);
                }
                if (context.selfInDanger && CouldBlock(blockChance))
                {
                    States.ChangeState(EnemyAI.EnemyState.Block);
                }
                else if (context.selfInDanger && !CouldBlock(blockChance))
                {
                    States.ChangeState(EnemyAI.EnemyState.Dashback);
                }
                if (context.playerStunned)
                {
                    States.ChangeState(EnemyAI.EnemyState.Chase);
                }
                break;

            case EnemyAI.EnemyState.Block:
                if (!context.selfInDanger && !context.selfLowStamina && _guardtimer <= 0)
                {
                    if (context.playerStrongAttacking)
                    {
                        States.ChangeState(EnemyAI.EnemyState.Attack);
                    }
                    else
                    {
                        States.ChangeState(EnemyAI.EnemyState.StrongAttack);
                    }
                }
                if (!context.playerInAttackRange && context.playerInVisionRange && _guardtimer <= 0)
                {
                    States.ChangeState(EnemyAI.EnemyState.Chase);
                }
                if (!context.playerInVisionRange)
                {
                    States.ChangeState(EnemyAI.EnemyState.Idle);
                }
                if (context.selfLowStamina && CouldRetire(RetireChance))
                {
                    States.ChangeState(EnemyAI.EnemyState.Retire);
                }
                break;

            case EnemyAI.EnemyState.Hurt:
                if (!States.Hurted && !context.selfLowStamina)
                {
                    States.ChangeState(EnemyAI.EnemyState.Chase);
                }
                if (!States.Hurted && context.selfLowStamina && CouldRetire(RetireChance))
                {
                    States.ChangeState(EnemyAI.EnemyState.Retire);
                }
                if (context.playerInAttackRange)
                {
                    if (CouldStrongAttack(strongAttackChance))
                    {
                        _strongattacktimer = strongAttackDuration;
                        States.ChangeState(EnemyAI.EnemyState.StrongAttack);
                    }
                    else
                    {
                        States.ChangeState(EnemyAI.EnemyState.Attack);
                    }
                }
                break;

            case EnemyAI.EnemyState.Dashback:
                if (!dashExecuted)
                {
                    States.DashingBack();
                    dashExecuted = true;
                }

                if (!States.isDashing)
                {
                    dashExecuted = false;

                    if (context.selfLowStamina && CouldRetire(RetireChance))
                    {
                        States.ChangeState(EnemyAI.EnemyState.Retire);
                    }
                    else
                    {
                        _guardtimer = guardTime;
                        States.ChangeState(EnemyAI.EnemyState.Block);
                    }
                }
                break;
        }

    }


}
