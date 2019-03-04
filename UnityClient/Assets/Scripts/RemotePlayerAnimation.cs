using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class RemotePlayerAnimation : MonoBehaviour
    {
        public Animator Animator;

        public float CurrentSpeed = 0.0f;
        public int CurrentAnimation = 0;

        public void LateUpdate()
        {
            Animator.SetFloat(PlayerController.AnimatorSpeedName, CurrentSpeed);
            Animator.SetInteger(PlayerController.AnimatorCurrentAnimationName, CurrentAnimation);
        }
    }
}