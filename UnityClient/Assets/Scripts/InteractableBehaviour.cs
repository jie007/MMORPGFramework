using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Interactable;
using Common.Interactables;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class InteractableBehaviour : MonoBehaviour
    {
        public Common.Interactables.Interactable Konfiguration = new Common.Interactables.Interactable();

        private NavMeshAgent navMeshAgent;

        public GameObject ActiveGo;
        public GameObject InactiveGo;

        private Coroutine interactionRoutine = null;

        public void SetInteractableState(bool isInteractable)
        {
            if(!isInteractable)
                OnAbortInteraction();

            ActiveGo.SetActive(isInteractable);
            InactiveGo.SetActive(!isInteractable);
        }

        public void Start()
        {
            navMeshAgent = GameObject.FindObjectOfType<NavMeshAgent>();
        }

        public void OnInteract()
        {
            if (ActiveGo.activeSelf)
            {
                Debug.Log("Send Start Message");
                GameObject.FindObjectOfType<InteractableClient>().SendStartMessage(this.Konfiguration.Id);
                interactionRoutine = this.StartCoroutine(Interacting());
            }
        }

        private IEnumerator Interacting()
        {
            yield return new WaitForSeconds((float) Konfiguration.TimeToInteract / 1000.0f);
            Debug.Log("Send Start Finish");
            GameObject.FindObjectOfType<InteractableClient>().SendFinishMessage(this.Konfiguration.Id);
            interactionRoutine = null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(this.transform.position, Konfiguration.MaximumDistance);
        }

        public void OnAbortInteraction()
        {
            if (interactionRoutine != null)
            {
                this.StopCoroutine(interactionRoutine);
                interactionRoutine = null;
            }
        }
    }
}
