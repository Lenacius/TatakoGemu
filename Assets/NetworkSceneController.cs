using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
public class NetworkSceneController : NetworkBehaviour
{
    public void GotoStage1()
    {
        GameObject.Destroy(GameObject.Find("Level"));
        SceneManager.LoadScene("Stage1", LoadSceneMode.Additive);
    }
}
