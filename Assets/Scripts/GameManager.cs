using UnityEngine;

public class GameManager : MonoBehaviour
{
    private InputActions _input;
    public InputActions Input { get => _input;}
    
    private void Awake()
    {
        _input = new InputActions();
    }
}
