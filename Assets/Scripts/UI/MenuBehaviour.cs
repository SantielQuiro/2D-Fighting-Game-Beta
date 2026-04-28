using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBehaviour : MonoBehaviour
{

    public void StartGame() //Loads the Game Scene when the user clicks start game
    {
        SceneManager.LoadScene("Game1");
    }

    public void ExitGame() //Quits the app when the user clicks the exit button (only works in built)
    {
        Application.Quit();
    }

    //Code responsible for moving the controls panel in the menu, with a smooth animation using Lerp and coroutines for the animation time
    #region Controls Panel

    public RectTransform panelcontrols;
    public float speed;
    float finalPos;
    bool isOpen;

    private void Start()
    {
        finalPos = Screen.width / 2;
        panelcontrols.position = new Vector3(-finalPos, panelcontrols.position.y, 0);
    }

    IEnumerator MovePanel(float time, Vector3 firstPos, Vector3 endPos)
    {
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            panelcontrols.position = Vector3.Lerp(firstPos, endPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        panelcontrols.position = endPos;
    }

    void Movement(float time, Vector3 firstPos, Vector3 endPos)
    {
        StartCoroutine(MovePanel(time, firstPos, endPos));
    }

    public void ButtonControls()
    {
        int sign = isOpen ? -1 : 1;
        Movement(speed, panelcontrols.position, new Vector3(sign * finalPos, panelcontrols.position.y, 0));
        isOpen = !isOpen;
    }
    #endregion 


    #region Sounds

    public AudioSource clickSound, backSound;

    public void PlayClickSound()
    {
        clickSound.Play();
    }

    public void PlayBackSound()
    {
        backSound.Play();
    }

    #endregion

    #region ...
    public GameObject crabLogo;
    public void Secret() // a little function for a little easter egg on the menu, for a hidden button 
    {
        crabLogo.SetActive(true);
    }
    #endregion
}
