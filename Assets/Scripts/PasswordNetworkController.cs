using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PasswordNetworkController : MonoBehaviour
{
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private GameObject passwordEntryUI;
    [SerializeField] private GameObject leaveButton;

    public void Host() { }

    public void Client() { }

    public void Leave() { }
}
