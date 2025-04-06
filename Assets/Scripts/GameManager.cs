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
        _input.Drone.Disable();
        _input.Forklift.Disable();
    }

    public void EnableDrone()
    {
        _input.Drone.Enable();
        _input.Player.Disable();
        _input.Forklift.Disable();
    }

    public void EnableForklift()
    {
        _input.Forklift.Enable();
        _input.Drone.Disable();
        _input.Player.Disable();
    }
}
