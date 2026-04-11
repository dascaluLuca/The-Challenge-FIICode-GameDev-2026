using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Transform respawnPoint;
    private PlayerMotor playerMotor;
    private CharacterController controller;

    void Awake()
    {
        Instance = this;
        playerMotor = FindObjectOfType<PlayerMotor>();
        controller = playerMotor.GetComponent<CharacterController>();
    }

    public void RespawnPlayer()
    {
        // Must disable CharacterController before teleporting or Unity ignores it
        controller.enabled = false;
        playerMotor.transform.position = respawnPoint.position;
        controller.enabled = true;
    }
}