using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        public Animator Animator;

        public float MaximumCharacterSpeed = 5.0f;
        public float CurrentSpeed = 0.0f;

        public Transform Camera;

        // Update is called once per frame
        public void LateUpdate()
        {
            if (Input.GetKey(KeyCode.W))
            {
                bool walk = Input.GetKey(KeyCode.LeftShift);
                CurrentSpeed = walk ? 0.5f : 1.0f;
            }
            else
            {
                CurrentSpeed = 0;
            }

            Vector3 direction = new Vector3(Camera.transform.forward.x, 0, Camera.transform.forward.z).normalized;
            this.transform.LookAt(this.transform.position + direction);
            this.transform.position += direction * CurrentSpeed * MaximumCharacterSpeed * Time.deltaTime;

            Animator.SetFloat("Speed", CurrentSpeed);
        }
    }
}