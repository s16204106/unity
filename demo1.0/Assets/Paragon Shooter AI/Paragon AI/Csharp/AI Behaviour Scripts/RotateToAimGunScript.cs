﻿using UnityEngine;
using System.Collections;

/*
 * Rotates the spine of the agent such that the gun points in the direction of the target.
 * */

namespace ParagonAI
{
    public class RotateToAimGunScript : MonoBehaviour
    {
        public Transform spineBone;
        public Transform bulletSpawnTransform;
        [HideInInspector]
        public Transform targetTransform;

        public bool shouldDebug = false;

        public Vector3 maximumRotationAngles = new Vector3(90, 90, 90);

        public float rotationSpeed = 5;

        Quaternion spineRotationLastFrame;
        Vector3 tempSpineLocalEulerAngles;

        public bool isEnabled = false;

        public float minDistToAim = 3f;

        Quaternion targetRot;


        void Awake()
        {
            if (spineBone)
                spineRotationLastFrame = spineBone.rotation;
            else
            {
                this.enabled = false;
            }
        }

        void LateUpdate()
        {
            if (isEnabled && targetTransform && minDistToAim < Vector3.Distance(spineBone.position, targetTransform.position))
            {
                //Rotate the spine bone so the gun (roughly) aims at the target
                spineBone.rotation = Quaternion.FromToRotation(bulletSpawnTransform.forward, targetTransform.position - bulletSpawnTransform.position) * spineBone.rotation;

                tempSpineLocalEulerAngles = spineBone.localEulerAngles;

                //Stop our agent from breaking their back by rotating too far
                tempSpineLocalEulerAngles = new Vector3(ClampEulerAngles(tempSpineLocalEulerAngles.x, maximumRotationAngles.x),
                                                        ClampEulerAngles(tempSpineLocalEulerAngles.y, maximumRotationAngles.y),
                                                        ClampEulerAngles(tempSpineLocalEulerAngles.z, maximumRotationAngles.z));

                spineBone.localEulerAngles = tempSpineLocalEulerAngles;
                targetRot = spineBone.rotation;

                //Smoothly rotate to the new position.  
                spineBone.rotation = Quaternion.Slerp(spineRotationLastFrame, targetRot, Time.deltaTime * rotationSpeed);
                spineRotationLastFrame = spineBone.rotation;

                if (shouldDebug)
                {
                    Debug.DrawRay(bulletSpawnTransform.position, bulletSpawnTransform.forward * 1000, Color.red);
                    Debug.DrawLine(bulletSpawnTransform.position, targetTransform.position, Color.blue);
                }
            }
            else
            {
                //Smoothly return to the default position if we're not engaged to a target.  More or less mirrors the agent's animations.
                targetRot = spineBone.rotation;
                spineBone.rotation = Quaternion.Slerp(spineRotationLastFrame, targetRot, Time.deltaTime * rotationSpeed);
                spineRotationLastFrame = spineBone.rotation;
            }
        }

        public void Activate()
        {
            spineRotationLastFrame = spineBone.rotation;
            isEnabled = true;
        }

        public void Deactivate()
        {
            isEnabled = false;
        }

        public void SetTargetTransform(Transform x)
        {
            targetTransform = x;
        }

        //Can't really decide which of the following two methods to use.
        float ClampEulerAngles(float r, float lim)
        {
            if (r > 180)
                r -= 360;

            r = Mathf.Clamp(r, -lim, lim);

            return r;
        }


        float ResetIfTooHigh(float r, float lim)
        {
            if (r > 180)
                r -= 360;

            if (r < -lim || r > lim)
            {
                return 0;
            }
            else
                return r;
        }
    }
}
