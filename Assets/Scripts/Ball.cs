using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    [SerializeField] private Renderer ballRenderer;

    private NetworkVariable<Color> ballColour = new NetworkVariable<Color>();

    public override void OnNetworkSpawn()
    {
        if(!IsServer) { return; }

        ballColour.Value = Random.ColorHSV();
    }

    private void Update()
    {
        if (!IsOwner) { return; }

        if (!Input.GetKeyDown(KeyCode.Space)) { return; }

        DestroyBallServerRpc();
    }

    [ServerRpc]
    private void DestroyBallServerRpc()
    {
        //GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }

    private void OnEnable()
    {
        ballColour.OnValueChanged += OnBallCOlourChanged;
    }

    private void OnDisable()
    {
        ballColour.OnValueChanged -= OnBallCOlourChanged;
    }

    private void OnBallCOlourChanged(Color oldBallColour, Color newBallColour)
    {
        if (!IsClient) { return; }

        ballRenderer.material.SetColor("_BaseColor", newBallColour);
    }
}
