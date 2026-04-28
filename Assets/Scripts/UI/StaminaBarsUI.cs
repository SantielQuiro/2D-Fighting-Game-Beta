using UnityEngine;
using UnityEngine.UI;

public class StaminaBarsUI : MonoBehaviour
{
    public StaminaGeneric stamina;
    public Image fillImage;
    public float targetFill, currentFill, smoothSpeed;
    private void OnEnable()
    {
        stamina.OnStaminaChanged += UpdateBar;
    }

    private void OnDisable()
    {
        stamina.OnStaminaChanged -= UpdateBar;
    } //suscriptions that react to the event of the generic stamina script


    private void Update()
    {
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed); //Interpolation to animate the change of the stamina bar
        fillImage.fillAmount = currentFill;
    }

    void UpdateBar(float percent)
    {
        targetFill = percent; //refresh the target value of the stamina bar
    }
}
