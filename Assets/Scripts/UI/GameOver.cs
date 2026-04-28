using UnityEngine;

public class GameOver : MonoBehaviour
{
    [SerializeField] Player player; 
    [SerializeField] EnemyAI enemy;
    [SerializeField] PlayerAttack pAttack;
    [SerializeField] PlayerDefence pDefence;
    private void OnEnable() //Deactivates the player, enemy, attack and defence scripts when the game over panel is enabled, freezing the game
    {
        player.enabled = false;
        enemy.enabled = false;
        pAttack.enabled = false;
        pDefence.enabled = false;
    }
}
