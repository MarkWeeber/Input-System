using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Scripts.UI;


namespace Game.Scripts.LiveObjects
{
    public class InteractableZone : MonoBehaviour
    {
        private enum ZoneType
        {
            Collectable,
            Action,
            HoldAction
        }

        private enum KeyState
        {
            Press,
            PressHold
        }

        [SerializeField]
        private ZoneType _zoneType;
        [SerializeField]
        private int _zoneID;
        [SerializeField]
        private int _requiredID;
        [SerializeField]
        [Tooltip("Press the (---) Key to .....")]
        private string _displayMessage;
        [SerializeField]
        private string _customDisplayMessage;
        [SerializeField]
        private GameObject[] _zoneItems;
        private bool _inZone = false;
        private bool _itemsCollected = false;
        private bool _actionPerformed = false;
        [SerializeField]
        private Sprite _inventoryIcon;
        [SerializeField]
        private InputActionReference _inputActionAsset;
        [SerializeField]
        private InputActionReference _alternateInputActionAsset;
        [SerializeField]
        private KeyCode _zoneKeyInput;
        [SerializeField]
        private KeyState _keyState;
        [SerializeField]
        private GameObject _marker;

        private bool _inHoldState = false;

        private static int _currentZoneID = 0;
        public static int CurrentZoneID
        {
            get
            {
                return _currentZoneID;
            }
            set
            {
                _currentZoneID = value;

            }
        }


        public static event Action<InteractableZone> onZoneInteractionComplete;
        public static event Action<InteractableZone> onZoneAlternateInteractionComplete;
        public static event Action<int> onHoldStarted;
        public static event Action<int> onHoldEnded;

        private void Start()
        {
            AssignInput();
        }

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += SetMarker;
            InteractableZone.onZoneAlternateInteractionComplete += SetMarker;

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && _currentZoneID > _requiredID)
            {
                switch (_zoneType)
                {
                    case ZoneType.Collectable:
                        if (_itemsCollected == false)
                        {
                            _inZone = true;
                            if (_displayMessage != null)
                            {
                                string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the {_zoneKeyInput.ToString()} key to collect");
                        }
                        break;

                    case ZoneType.Action:
                        if (_actionPerformed == false)
                        {
                            _inZone = true;
                            if (_displayMessage != null)
                            {
                                string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the {_zoneKeyInput.ToString()} key to perform action");
                        }
                        break;

                    case ZoneType.HoldAction:
                        _inZone = true;
                        if (_displayMessage != null)
                        {
                            string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                            UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                        }
                        else
                            UIManager.Instance.DisplayInteractableZoneMessage(true, $"Hold the {_zoneKeyInput.ToString()} key to perform action");
                        break;
                }
                if (_customDisplayMessage != string.Empty)
                {
                    UIManager.Instance.DisplayInteractableZoneMessage(true, _customDisplayMessage);
                }
            }
        }

        private void CollectItems(bool alternate = false)
        {
            foreach (var item in _zoneItems)
            {
                item.SetActive(false);
            }

            UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            CompleteTask(_zoneID);
            if (alternate)
            {
                onZoneAlternateInteractionComplete?.Invoke(this);
            }
            else
            {
                onZoneInteractionComplete?.Invoke(this);
            }
        }

        private void PerformAction(bool alternate = false)
        {
            foreach (var item in _zoneItems)
            {
                item.SetActive(true);
            }

            if (_inventoryIcon != null)
                UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            if (alternate)
            {
                onZoneAlternateInteractionComplete?.Invoke(this);
            }
            else
            {
                onZoneInteractionComplete?.Invoke(this);
            }
        }

        private void PerformHoldAction()
        {
            UIManager.Instance.DisplayInteractableZoneMessage(false);
            onHoldStarted?.Invoke(_zoneID);
        }

        public GameObject[] GetItems()
        {
            return _zoneItems;
        }

        public int GetZoneID()
        {
            return _zoneID;
        }

        public void CompleteTask(int zoneID, bool alternate = false)
        {
            if (zoneID == _zoneID)
            {
                _currentZoneID++;
                if (alternate)
                {
                    onZoneAlternateInteractionComplete?.Invoke(this);
                }
                else
                {
                    onZoneInteractionComplete?.Invoke(this);
                }
            }
        }

        public void ResetAction(int zoneID)
        {
            if (zoneID == _zoneID)
                _actionPerformed = false;
        }

        public void SetMarker(InteractableZone zone)
        {
            if (_zoneID == _currentZoneID)
                _marker.SetActive(true);
            else
                _marker.SetActive(false);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _inZone = false;
                UIManager.Instance.DisplayInteractableZoneMessage(false);
            }
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= SetMarker;
            InteractableZone.onZoneAlternateInteractionComplete -= SetMarker;
        }

        private void AssignInput()
        {
            _inputActionAsset.action.performed += ActionPerformed;
            _inputActionAsset.action.canceled += ActionCancelled;
            if (_alternateInputActionAsset != null)
            {
                _alternateInputActionAsset.action.performed += AlternateActionPerformed; ;
            }
        }

        private void AlternateActionPerformed(InputAction.CallbackContext obj)
        {
            ActionPerformed(true);
        }

        private void ActionCancelled(InputAction.CallbackContext context)
        {
            if (_inZone == true)
            {
                if (_inputActionAsset.action.WasReleasedThisFrame() && _keyState == KeyState.PressHold)
                {
                    _inHoldState = false;
                    onHoldEnded?.Invoke(_zoneID);
                }
            }
        }

        private void ActionPerformed(InputAction.CallbackContext context)
        {
            ActionPerformed(false);
        }

        private void ActionPerformed(bool alternate = false)
        {
            if (_inZone == true)
            {
                if (_keyState != KeyState.PressHold)
                {
                    //press
                    switch (_zoneType)
                    {
                        case ZoneType.Collectable:
                            if (_itemsCollected == false)
                            {
                                CollectItems(alternate);
                                _itemsCollected = true;
                                UIManager.Instance.DisplayInteractableZoneMessage(false);
                            }
                            break;

                        case ZoneType.Action:
                            if (_actionPerformed == false)
                            {
                                PerformAction(alternate);
                                _actionPerformed = true;
                                UIManager.Instance.DisplayInteractableZoneMessage(false);
                            }
                            break;
                    }
                }
                else if (_keyState == KeyState.PressHold && _inHoldState == false)
                {
                    _inHoldState = true;
                    switch (_zoneType)
                    {
                        case ZoneType.HoldAction:
                            PerformHoldAction();
                            break;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            _inputActionAsset.action.performed -= ActionPerformed;
            _inputActionAsset.action.canceled -= ActionCancelled;
            if (_alternateInputActionAsset != null)
            {
                _alternateInputActionAsset.action.performed -= AlternateActionPerformed; ;
            }
        }
    }
}


