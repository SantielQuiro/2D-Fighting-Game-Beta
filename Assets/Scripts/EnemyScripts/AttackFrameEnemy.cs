using UnityEngine;

public class AttackFrameEnemy : MonoBehaviour
{
    public EnemyAI enemy;
    public int damage;
    public IParryable owner;
    void Update()
    {
        damage = enemy.damage;
        owner = enemy.GetComponent<IParryable>(); //Takes the IParryable component from the enemy
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponent<IDamageable>(); 
        
        if (damageable != null)
        {
            collision.GetComponent<PlayerDefence>().ReadAttackerToParry(owner); //sends the IParryable component taken previously to make parry possible
            damageable.TryTakeDamage(damage, "light"); //sends damage and type of attack ("strong" or "light" in this case)
        }
    }
}
