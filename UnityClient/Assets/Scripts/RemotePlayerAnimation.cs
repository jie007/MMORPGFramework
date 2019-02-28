using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class RemotePlayerAnimation : MonoBehaviour
    {
        public Animator Animator;

        public float CurrentSpeed = 0.0f;

        public void LateUpdate()
        {
            Animator.SetFloat("Speed", CurrentSpeed);
        }
    }
}