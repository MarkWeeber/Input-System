using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private int _strongPunchCrateDestructionNumber = 5;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        private bool _isReadyToBreak = false;

        private List<Rigidbody> _brakeOff = new List<Rigidbody>();

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += Interact;
            InteractableZone.onZoneAlternateInteractionComplete += AlternateInteract;
        }

        private void Interact(InteractableZone zone)
        {
            InteractableZone_onZoneInteractionComplete(zone);
        }

        private void AlternateInteract(InteractableZone zone)
        {
            InteractableZone_onZoneInteractionComplete(zone, true);
        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone, bool alternate = false)
        {
           
            if (_isReadyToBreak == false && _brakeOff.Count >0)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }

            if (_isReadyToBreak && zone.GetZoneID() == 6) //Crate zone            
            {
                if (_brakeOff.Count > 0)
                {
                    if (alternate)
                    {
                        StrongBreak();
                    }
                    else
                    {
                        BreakPart();
                    }
                    StartCoroutine(PunchDelay());
                }
                else if(_brakeOff.Count == 0)
                {
                    _isReadyToBreak = false;
                    _crateCollider.enabled = false;
                    _interactableZone.CompleteTask(6);
                    Debug.Log("Completely Busted");
                }
            }
        }

        private void Start()
        {
            _brakeOff.AddRange(_pieces);
            
        }


        private void StrongBreak()
        {
            int partsToBreak = Mathf.Min(_brakeOff.Count, _strongPunchCrateDestructionNumber);
            for (int i = 0; i < partsToBreak; i++)
            {
                BreakPart();
            }
        }

        private void BreakPart()
        {
            int rng = Random.Range(0, _brakeOff.Count);
            _brakeOff[rng].constraints = RigidbodyConstraints.None;
            _brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
            _brakeOff.Remove(_brakeOff[rng]);            
        }

        IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(6);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= Interact;
            InteractableZone.onZoneAlternateInteractionComplete -= AlternateInteract;
        }
    }
}
