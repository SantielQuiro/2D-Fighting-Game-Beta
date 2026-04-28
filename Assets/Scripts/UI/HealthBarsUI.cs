using UnityEngine;
using UnityEngine.UI;

public class HealthBarsUI : MonoBehaviour
{
    public Image fillImage;
    public HealthGeneric health;
    public float targetFill, currentFill, smoothSpeed;

    private void OnEnable()
    {
        health.OnHealthChanged += UpdateBar;
    }

    private void OnDisable()
    {
        health.OnHealthChanged -= UpdateBar;
    } //Suscriptions to react when the health changes, 
    private void Update()
    {
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed); //Interpolation to animate the change of the health bar
        fillImage.fillAmount = currentFill;
    }

    void UpdateBar(float percent)
    {
        targetFill = percent; //refresh the target value of the health bar
    }
}
