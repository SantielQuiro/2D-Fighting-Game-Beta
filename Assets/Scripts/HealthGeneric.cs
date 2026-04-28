using UnityEngine;

public class HealthGeneric : MonoBehaviour
{
    public event System.Action<float> OnHealthChanged; //Event called when health changes, sends the percentage of health
    public event System.Action OnDamaged; //Event called when the character takes damage

    public float health, maxHealth;
    public GameObject gameOver;
    public AudioSource charSounds;
    public AudioClip GameOverSound, PunchSound;

    bool FlagSoundGameOver = false;

    public void TakeDamage(float damage) //Function called to reduce health, receives the damage to be taken as a parameter
    {
        health -= damage;
        float percent = (float)health / maxHealth;
        OnHealthChanged?.Invoke(percent);
        OnDamaged?.Invoke();
        charSounds.PlayOneShot(PunchSound);
    }
    private void Update()
    {
        if (health <= 0 && !FlagSoundGameOver) //bool flag to prevent the game over sound from repeating every frame when health is 0 or less
        { 
            gameOver.SetActive(true);
            charSounds.PlayOneShot(GameOverSound);
            FlagSoundGameOver = true;
        }
    }

}
