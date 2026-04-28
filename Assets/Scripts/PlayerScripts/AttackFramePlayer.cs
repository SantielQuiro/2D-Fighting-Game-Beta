using UnityEngine;

public class AttackFramePlayer : MonoBehaviour
{
    public PlayerAttack pAttack;
    public int damage;
    public IParryable owner;
    void Update()
    {
        damage = pAttack.Damage;
        owner = pAttack.GetComponent<IParryable>(); //takes the IParryable component from the object that has the PlayerAttack script
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
        {
            collision.GetComponent<EnemyDefence>().ReadAttackerToParry(owner); //sends the IParryable component to make parry
            damageable.TryTakeDamage(damage, "light"); //sends damage and type of attack ("strong" && "light")
        }
    }
}
