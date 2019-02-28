using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace Assets.Scripts
{
    public class PlayerCamera : MonoBehaviour
    {
        public Transform Target;
        public float MouseSensitivity = 2;
        public Vector2 PitchMinMax = new Vector2(-80, 85);
        public bool InversePitch;
        public bool InverseYaw;
        public float MaxDistanceFromTarget = 2;
        public float SmoothTime = 0.12f;

        private Vector3 rotationSmoothVelocity = Vector3.zero;
        private Vector3 currentRotation = Vector3.zero;

        private Vector3 desiredRotation = Vector3.zero;

        // Update is called once per frame
        public void LateUpdate()
        {
            if (Input.GetMouseButton((int)MouseButton.MiddleMouse))
            {
                Vector3 rotationInput = new Vector3(Input.GetAxis("Mouse Y") * MouseSensitivity * (InverseYaw ? -1 : 1),
                    Input.GetAxis("Mouse X") * MouseSensitivity * (InversePitch ? -1 : 1), 0);
                desiredRotation += rotationInput;

                desiredRotation.x = Mathf.Clamp(desiredRotation.x, PitchMinMax.x, PitchMinMax.y);
            }

            currentRotation = Vector3.SmoothDamp(currentRotation, desiredRotation, ref rotationSmoothVelocity, SmoothTime);
            transform.eulerAngles = currentRotation;

            float distanceFromTarget = MaxDistanceFromTarget;
            RaycastHit hitInfo;
            if (Physics.Raycast(new Ray(Target.position, -transform.forward), out hitInfo))
            {
                if (hitInfo.distance < MaxDistanceFromTarget)
                {
                    distanceFromTarget = hitInfo.distance - 0.001f;
                }
            }

            transform.position = Target.position - transform.forward * distanceFromTarget;
        }
    }
}