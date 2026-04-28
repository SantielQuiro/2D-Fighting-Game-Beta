using UnityEngine;

public class EnemyContext : MonoBehaviour
{
    [Header("REFERENCES")]
    public Transform player;
    public Transform self;

    public HealthGeneric selfHealth;
    public StaminaGeneric selfStamina;
    public EnemyDefence selfDefence;

    public PlayerAttack playerAttack;
    public PlayerDefence playerDefence;
    public HealthGeneric playerHealth;
    public StaminaGeneric playerStamina;
    

    [Header("DISTANCE / POSITION")]
    public float distanceToPlayer;
    public Vector2 directionToPlayer;
    public bool playerInAttackRange;
    public bool playerInVisionRange;

    [Header("SELF STATE")]
    public float selfHealthNormalized; // health actual / health max, useful to take the health percentage directly
    public float selfStaminaNormalized;// stamina actual / stamina max, same as above but for stamina
    public bool selfLowHealth; // true if health is below the lowHealthThreshold
    public bool selfLowStamina; // true if stamina is below the lowStaminaThreshold
    public bool selfCanAct; // bool that indicates if the enemy can perform actions, false if it's stunned, hurted or in any other state that should prevent it from acting

    [Header("PLAYER STATE")]
    public bool playerAttacking;
    public bool playerStrongAttacking;
    public bool playerBlocking;
    public bool playerParrying;
    public bool playerStunned;
    public bool playerRecovering;

    [Header("PLAYER EVENT MEMORY")]
    public float dangerMemoryTime = 0.2f; // time that the enemy will remember that the player is attacking after it stops
    float _dangerTimer; // timer that counts down from dangerMemoryTime when the player is attacking,
                        // used to keep the enemy in a "danger" state for a short time after the player stops attacking,
                        // check BrainEnemy.cs for more info


    [Header("COMBAT WINDOWS")]
    public bool playerVulnerable; //bool that indicates if the player is not doing any ofensive action, check UpdateCombatLogic() for more info
    public bool CanParry; //bool that conects with the parry window of the player in PlayerAttack.cs 
    public bool selfInDanger => _dangerTimer > 0f && playerInAttackRange; //controls the defensive behavior, pretty useful thanks to the danger memory, check BrainEnemy.cs for more info

    [Header("CONFIG")]
    public float attackRange = 1.5f;
    public float visionRange = 8f;
    public float lowHealthThreshold = 0.3f; //percentage of health below which the enemy considers itself in low health, check UpdateSelfState() for more info
    public float lowStaminaThreshold = 0.3f;//same as above but for stamina

    void Update()
    {
        UpdateSpatialData();
        UpdateSelfState();
        UpdatePlayerState();
        UpdateCombatLogic();

        selfCanAct = !selfDefence.Hurted && !selfDefence.StunnedCritic;

        if(_dangerTimer > 0)
        {
            _dangerTimer -= Time.deltaTime;
        }
        if (playerAttacking || playerStrongAttacking) //if the player attacks, the enemy considers itself in danger for a short time
        {
            _dangerTimer = dangerMemoryTime;
        }

        CanParry = playerAttack.CoolDownToBeParried; //conection between the parry window of the player and the enemy context,
                                                     //the enemy will only try to parry if this is true, check EnemyDefence.cs for more info
    }

    void UpdateSpatialData()
    {
        if (player == null) return;

        Vector2 diff = player.position - self.position; //difference vector between the player and the enemy, used to calculate distance and direction

        distanceToPlayer = diff.magnitude;
        directionToPlayer = diff.normalized;

        playerInAttackRange = distanceToPlayer <= attackRange;
        playerInVisionRange = distanceToPlayer <= visionRange;
    }
    void UpdateSelfState()
    {
        if (selfHealth != null)
            selfHealthNormalized = selfHealth.health / selfHealth.maxHealth;

        if (selfStamina != null)
            selfStaminaNormalized = selfStamina.stamina / selfStamina.maxStamina;

        selfLowHealth = selfHealthNormalized <= lowHealthThreshold; //low health means that the enemy is below the lowHealthThreshold percentage of health
        selfLowStamina = selfStaminaNormalized <= lowStaminaThreshold;//same as above but for stamina

        selfCanAct = true;
    }
    void UpdatePlayerState()
    {
        if (playerDefence == null) return;

        playerBlocking = playerDefence.isBlocking;
        playerParrying = playerDefence.isParrying;
        playerStunned = playerDefence.StunnedCritic;

        playerAttacking = playerAttack.isAttacking;
        playerStrongAttacking = playerAttack.isStrongAttacking;
        playerRecovering = playerDefence.isRecovering;
    }
    void UpdateCombatLogic()
    {
        //if the player is not parrying, not stunned, not attacking and not strong attacking, then it's vulnerable
        playerVulnerable =
            !playerParrying &&
            !playerStunned &&
            !playerAttacking &&
            !playerStrongAttacking;
    }
}

