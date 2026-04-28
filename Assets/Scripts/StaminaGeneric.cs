using UnityEngine;

public class StaminaGeneric : MonoBehaviour
{
    public event System.Action<float> OnStaminaChanged; 
    public event System.Action OnStaminaZero;

    public AudioSource audioSource;
    public AudioClip TimeStopSound; //Effect sound to play when stamina reaches zero

    public float stamina, maxStamina, StartReturnStamina;
    float _returnStaminaTimer;

    public void LooseStamina(float amount) //Function called to reduce stamina, receives the amount to reduce as a parameter
    {
        stamina -= amount;
        float percent = (float)stamina / maxStamina;
        OnStaminaChanged?.Invoke(percent);
        _returnStaminaTimer = StartReturnStamina;

        if (stamina == 0)//When stamina reaches zero
        {
            OnStaminaZero?.Invoke();
            audioSource.PlayOneShot(TimeStopSound);
        }
    }
    private void Update()
    {
        if (stamina < 0) { stamina = 0; }

        TimerToReturnStamina();
    }

    public void RegenStamina(float amountToRegen) //fuction called to regenerate stamina from everywhere
    {
        float total = amountToRegen;
        stamina += total;
        float percent = (float)stamina / maxStamina;
        OnStaminaChanged?.Invoke(percent);
    }

    void TimerToReturnStamina() //Regenerates stamina after a certain time without losing stamina, this is called in the update function
    {
        if (_returnStaminaTimer <= 0 && stamina < maxStamina)
        {
            RegenStamina(maxStamina);
            _returnStaminaTimer = StartReturnStamina;
        }
        else
        {
            _returnStaminaTimer -= Time.deltaTime;
        }

        if (_returnStaminaTimer < 0){ _returnStaminaTimer = 0;}

        if (stamina > maxStamina) { stamina = maxStamina; }
    }
}
