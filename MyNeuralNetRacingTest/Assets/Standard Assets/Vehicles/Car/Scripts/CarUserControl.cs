using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent (typeof (CarController))]
    public class CarUserControl : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        public float h, v;
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
            float handbrake = CrossPlatformInputManager.GetAxis ("Jump");

            //Debug.Log ("Hor: " + h + " || Vert: " + v);
            m_Car.Move (h, v, v, handbrake);
#else
            m_Car.Move(h, v, v, 0f);
#endif
        }

        private void OnTriggerEnter (Collider other)
        {
            if (other.gameObject.tag != "Waypoint") {
                crash = true;
            }
        }
    }
}
