using UnityEngine;

public class GameManager : MonoBehaviour
{
    private InputActions _input;
    public InputActions Input { get => _input;}
    
    private void Awake()
    {
        _input = new InputActions();
        _input.Enable();
    }

    public void EnablePlayer()
    {
        _input.Player.Enable();
    }

    public void DisablePlayer()
    {
        _input.Player.Disable();
    }

    public void EnableDrone()
    {
        _input.Drone.Enable();
    }

    public void DisableDrone()
    {
        _input.Drone.Disable();
    }

}
