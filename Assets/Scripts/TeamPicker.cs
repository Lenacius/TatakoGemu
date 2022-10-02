using Unity.Netcode;
using UnityEngine;

public class TeamPicker : MonoBehaviour
{
    public void SelectTeam(int teamIndex)
    {
        //ulong localClientId = NetworkManager.Singleton.LocalClientId;

        //if(!NetworkManager.Singleton.ConnectedClients.TryGetValue(localClientId, out NetworkClient networkClient))
        //{
        //    return;
        //}

        if(!NetworkManager.Singleton.LocalClient.PlayerObject.TryGetComponent<TeamPlayer>(out TeamPlayer teamPlayer)) { return; }
        
        //if(!networkClient.PlayerObject.TryGetComponent<TeamPlayer>(out TeamPlayer teamPlayer)){
        //    return;
        //}

        teamPlayer.SetTeamServerRpc((byte)teamIndex);
    }

}
