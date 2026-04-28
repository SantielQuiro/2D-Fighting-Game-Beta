using UnityEngine;
using UnityEngine.SceneManagement;

public class CamBehaviour : MonoBehaviour
{
    public Transform player;
    public float speedCam;
    public Vector2 currentVel;
    void Update()
    {
        Vector2.SmoothDamp(transform.position, player.position,ref currentVel, speedCam);
        transform.position = new Vector3(currentVel.x, currentVel.y, -10); // The camera follows the player within a range limited by speedCam
    }

    public void RestartButton() // Restarts the current scene when the restart button is clicked
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenuButton() // Returns to the main menu when the back to menu button is clicked
    { 
        SceneManager.LoadScene("Main Menu");
    }
}
