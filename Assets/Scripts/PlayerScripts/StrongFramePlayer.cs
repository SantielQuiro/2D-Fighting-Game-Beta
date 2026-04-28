using UnityEngine;

public class StrongFramePlayer : MonoBehaviour
{
    public PlayerAttack pAttack;
    public int damage;
    public IParryable owner;
    void Update()
    {
        damage = pAttack.Damage;
        owner = pAttack.GetComponent<IParryable>(); //takes the IParryable component from the object with the PlayerAttack script
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
        {
            collision.GetComponent<EnemyDefence>().ReadAttackerToParry(owner);//sends the IParryable component taken to make parry
            damageable.TryTakeDamage(damage, "strong"); //sends damage and type of attack ("strong" && "light")
        }
    }
}
