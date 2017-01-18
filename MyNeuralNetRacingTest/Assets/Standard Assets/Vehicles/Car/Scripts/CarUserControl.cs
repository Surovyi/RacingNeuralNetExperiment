using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent (typeof (CarController))]
    public class CarUserControl : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        private float h;
        private float v;

        public float H { get { return h; } set { h = value; } }
        public float V { get { return v; } set { v = value; } }

        [HideInInspector]
        public bool crash = false;

        private void Awake ()
        {
            // get the car controller
            m_Car = GetComponent<CarController> ();
        }


        private void FixedUpdate ()
        {
            //Here neural net will control car;
            //Neural net has to return 'h' and 'v' {-1; 1}


            // pass the input to the car!
            //float h = CrossPlatformInputManager.GetAxis ("Horizontal");
            //float v = CrossPlatformInputManager.GetAxis ("Vertical");
#if !MOBILE_INPUT
            //float handbrake = CrossPlatformInputManager.GetAxis ("Jump");
            m_Car.Move (h, v, v, 0f);
#else
            m_Car.Move(h, v, v, 0f);
#endif
        }

        public void ResetAxis ()
        {
            h = 0f;
            v = 0f;
            
            foreach (WheelCollider wheel in m_Car.m_WheelColliders) {
                wheel.brakeTorque = Mathf.Infinity;
            }

            m_Car.m_Rigidbody.isKinematic = true;
            StartCoroutine (ActivateRigidbody ());
        }

        private IEnumerator ActivateRigidbody ()
        {
            yield return new WaitForFixedUpdate ();
            foreach (WheelCollider wheel in m_Car.m_WheelColliders) {
                wheel.brakeTorque = 0f;
            }
            m_Car.m_Rigidbody.isKinematic = false;
        }

        public float GetCurrentNormalizedSpeed ()
        {
            return m_Car.CurrentSpeed / m_Car.MaxSpeed;
        }

        public float GetCurrentSpeed ()
        {
            return m_Car.CurrentSpeed;
        }
    }
}
