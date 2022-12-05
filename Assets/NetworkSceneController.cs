using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
public class NetworkSceneController : NetworkBehaviour
{
    public void GotoStage1()
    {
        if (IsHost)
        {
            ResetClientReadyClientRpc();
            DeleteLevelSpecificObjectsClientRpc();
            NetworkManager.SceneManager.LoadScene("Stage1", LoadSceneMode.Additive);
        }
    }

    public void GotoStage2()
    {
        if (IsHost)
        {
            ResetClientReadyClientRpc();
            DeleteLevelSpecificObjectsClientRpc();
            NetworkManager.SceneManager.LoadScene("Stage2", LoadSceneMode.Additive);
        }
    }

    public void GotoEnding()
    {
        if (IsHost)
        {
            ResetClientReadyClientRpc();
            DeleteLevelSpecificObjectsClientRpc();
            NetworkManager.SceneManager.LoadScene("Ending", LoadSceneMode.Single);
        }
    }

    public void GotoLobby()
    {
        SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    [ClientRpc]
    public void DeleteLevelSpecificObjectsClientRpc()
    {
        GameObject.Destroy(GameObject.Find("Level"));
    }

    [ClientRpc]
    public void ResetClientReadyClientRpc()
    {
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>().SetPlayerReadyServerRpc(false);
    }

//#if UNITY_EDITOR
//    public UnityEditor.SceneAsset SceneAsset;
//    private void OnValidate()
//    {
//        if (SceneAsset != null)
//        {
//            m_SceneName = SceneAsset.name;
//        }
//    }
//#endif
//    [SerializeField]
//    private string m_SceneName;

//    public override void OnNetworkSpawn()
//    {
//        if (IsHost && !string.IsNullOrEmpty(m_SceneName))
//        //if(IsHost)
//        {
//            var status = NetworkManager.SceneManager.LoadScene(m_SceneName, LoadSceneMode.Additive);
//            Debug.Log(status);
//            if (status != SceneEventProgressStatus.Started)
//            {
//                Debug.LogWarning($"Failed to load {m_SceneName} " +
//                      $"with a {nameof(SceneEventProgressStatus)}: {status}");
//            }
//        }
//    }
}
