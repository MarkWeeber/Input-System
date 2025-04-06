using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Game.Scripts.UI;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }

        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;
        

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        private InputActions _input;
        private GameManager _gameManager;
        private Vector2 _tiltInput;
        private float _steerInput;
        private float _verticalInput;

        private void OnEnable()
        {
            InitializeInputs();
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                _gameManager.EnableDrone();
                _gameManager.DisablePlayer();
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {            
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);
            _gameManager.DisableDrone();
            _gameManager.EnablePlayer();
        }

        private void Update()
        {
            if (_inFlightMode)
            {
                CalculateTilt();
                CalculateMovementUpdate();
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {
            _steerInput = _input.Drone.Steer.ReadValue<float>();
            var tempRot = transform.localRotation.eulerAngles;
            tempRot.y -= (_speed / 3) * _steerInput;
            transform.localRotation = Quaternion.Euler(tempRot);
        }

        private void CalculateMovementFixedUpdate()
        {
            _verticalInput = _input.Drone.Vertical.ReadValue<float>();

            _rigidbody.AddForce(transform.up * _speed * _verticalInput, ForceMode.Acceleration);
        }

        private void CalculateTilt()
        {
            _tiltInput = _input.Drone.Tilt.ReadValue<Vector2>();
            transform.rotation = Quaternion.Euler(
                    _tiltInput.y * 30,
                    transform.localRotation.eulerAngles.y,
                    _tiltInput.x * 30 * -1f
                );
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
        }

        private void InitializeInputs()
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            _input = _gameManager.Input;
            _input.Drone.Return.performed += ReturnPerformed;
        }

        private void ReturnPerformed(InputAction.CallbackContext context)
        {
            _inFlightMode = false;
            onExitFlightmode?.Invoke();
            ExitFlightMode();
        }
    }
}
