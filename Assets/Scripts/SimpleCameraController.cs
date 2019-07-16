﻿using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityTemplateProjects
{
    public class SimpleCameraController : MonoBehaviour
    {
        class CameraState
        {
            public float yaw;
            public float pitch;
            public float roll;
            public float x;
            public float y;
            public float z;

            public void SetFromTransform(Transform t)
            {
                pitch = t.eulerAngles.x;
                yaw = t.eulerAngles.y;
                roll = t.eulerAngles.z;
                x = t.position.x;
                y = t.position.y;
                z = t.position.z;
            }

            public void Translate(Vector3 translation)
            {
                Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

                x += rotatedTranslation.x;
                y += rotatedTranslation.y;
                z += rotatedTranslation.z;
            }

            public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
            {
                yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
                pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
                roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);

                x = Mathf.Lerp(x, target.x, positionLerpPct);
                y = Mathf.Lerp(y, target.y, positionLerpPct);
                z = Mathf.Lerp(z, target.z, positionLerpPct);
            }

            public void UpdateTransform(Transform t)
            {
                t.eulerAngles = new Vector3(pitch, yaw, roll);
                t.position = new Vector3(x, y, z);
            }
        }

        CameraState m_TargetCameraState = new CameraState();
        CameraState m_InterpolatingCameraState = new CameraState();

        [Header("Movement Settings")]
        [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
        public float boost = 3.5f;

        [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
        public float positionLerpTime = 0.2f;

        [Header("Rotation Settings")]
        [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
        public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

        [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
        public float rotationLerpTime = 0.01f;

        [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
        public bool invertY = false;

        void OnEnable()
        {
            m_TargetCameraState.SetFromTransform(transform);
            m_InterpolatingCameraState.SetFromTransform(transform);
        }

        Vector3 GetInputTranslationDirection()
        {
            Vector3 direction = new Vector3();
            if (Keyboard.current.wKey.isPressed)
            {
                direction += Vector3.forward;
            }
            if (Keyboard.current.sKey.isPressed)
            {
                direction += Vector3.back;
            }
            if (Keyboard.current.aKey.isPressed)
            {
                direction += Vector3.left;
            }
            if (Keyboard.current.dKey.isPressed)
            {
                direction += Vector3.right;
            }
            if (Keyboard.current.qKey.isPressed)
            {
                direction += Vector3.down;
            }
            if (Keyboard.current.eKey.isPressed)
            {
                direction += Vector3.up;
            }

            if (Touchscreen.current.activeTouches.Count == 2)
            {
                var touchMovement = Touchscreen.current.activeTouches[0].delta.ReadValue();
                if (!invertY)
                {
                    touchMovement.y *= -1;
                }
                var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(touchMovement.magnitude) * 0.1f;

                direction += Vector3.right * touchMovement.x * mouseSensitivityFactor;
                direction += Vector3.back * touchMovement.y * mouseSensitivityFactor;
            }

            return direction;
        }

        void FixedUpdate()
        {

            // Rotation
            if (Mouse.current.rightButton.isPressed)
            {
                var mouseMovement = Mouse.current.delta.ReadValue();
                if (!invertY)
                {
                    mouseMovement.y *= -1;
                }
                var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude) * 0.1f;

                m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
                m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
            }

            // Rotation (Touch)
            if (Touchscreen.current.activeTouches.Count == 1)
            {
                var touchMovement = Touchscreen.current.activeTouches[0].delta.ReadValue();
                if (!invertY)
                {
                    touchMovement.y *= -1;
                }
                var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(touchMovement.magnitude) * 0.1f;

                m_TargetCameraState.yaw += touchMovement.x * mouseSensitivityFactor;
                m_TargetCameraState.pitch += touchMovement.y * mouseSensitivityFactor;
            }

            // Translation
            var translation = GetInputTranslationDirection() * Time.deltaTime;

            // Speed up movement when shift key held
            if (Keyboard.current.leftShiftKey.isPressed)
            {
                translation *= 10.0f;
            }

            translation *= Mathf.Pow(2.0f, boost);

            m_TargetCameraState.Translate(translation);

            // Framerate-independent interpolation
            // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
            var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
            var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
            m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

            m_InterpolatingCameraState.UpdateTransform(transform);
        }
    }

}
