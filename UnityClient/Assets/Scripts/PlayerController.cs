using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        public Animator Animator;
        public NavMeshAgent Agent;

        public float AnimationsSpeed = 0.0f;

        public Camera Camera;

        public InteractableBehaviour RunToInteractable;
        public InteractableBehaviour CurrentInteractable;

        public void Update()
        {
            MouseMovement();

            if (RunToInteractable != null)
            {
                float currentDistance = (RunToInteractable.transform.position - this.transform.position).magnitude;
                Debug.Log("Dst: " + currentDistance);
                if (currentDistance <= RunToInteractable.Konfiguration.MaximumDistance)
                {
                    StopPathFollowing();
                    RunToInteractable.OnInteract();
                    CurrentInteractable = RunToInteractable;

                    RunToInteractable = null;
                }
            }
        }

        private void StopPathFollowing()
        {
            Agent.isStopped = true;
        }

        private void MouseMovement()
        {
            var ray = Camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var interactable = hit.transform.GetComponent<InteractableBehaviour>();

                if (interactable != null)
                {
                    var go = hit.transform.gameObject;

                    Debug.Log("Highlight: " + go.name);
                    if (Input.GetMouseButtonDown(0))
                    {
                        AbortInteraction();
                        RunToInteractable = interactable;
                        MoveToPoint(go.transform.position);
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        AbortInteraction();
                        RunToInteractable = null;
                        MoveToPoint(hit.point);
                    }
                }
            }
        }

        private void AbortInteraction()
        {
            if (CurrentInteractable != null)
            {
                CurrentInteractable.OnAbortInteraction();
            }
            CurrentInteractable = null;
        }

        private void MoveToPoint(Vector3 point)
        {
            Agent.isStopped = false;
            Agent.SetDestination(point);
        }

        public void LateUpdate()
        {
            if (Input.GetKey(KeyCode.W))
            {
                bool walk = Input.GetKey(KeyCode.LeftShift);
                AnimationsSpeed = walk ? 0.5f : 1.0f;

                Vector3 direction = new Vector3(Camera.transform.forward.x, 0, Camera.transform.forward.z).normalized;
                this.transform.LookAt(this.transform.position + direction);
                StopPathFollowing();

                AbortInteraction();
                RunToInteractable = null;

                Agent.Move(direction * Agent.speed * Time.deltaTime * AnimationsSpeed);
            }
            else if (Agent.velocity.sqrMagnitude > 0)
            {
                AnimationsSpeed = 1;
            }
            else
            {
                AnimationsSpeed = 0;
            }

            Animator.SetFloat("Speed", AnimationsSpeed);
        }
    }
}