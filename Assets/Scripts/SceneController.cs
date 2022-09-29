using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void GoToLobby()
    {
        SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }
}
