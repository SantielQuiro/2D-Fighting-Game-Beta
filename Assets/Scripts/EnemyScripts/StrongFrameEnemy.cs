using UnityEngine;

public class StrongFrameEnemy : MonoBehaviour
{
    public EnemyAI enemy;
    public int damage;
    public IParryable owner;
    void Update()
    {
        damage = enemy.damage;
        owner = enemy.GetComponent<IParryable>(); //takes the IParryable component
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponent<IDamageable>();

        if (damageable != null)
        {
            collision.GetComponent<PlayerDefence>().ReadAttackerToParry(owner); //sends the IParryable component taken previously to make parry possible
            damageable.TryTakeDamage(damage, "strong"); //sends damage and type of attack (could be "light" or "strong")
        }
    }
}
