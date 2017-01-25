using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Raycast : MonoBehaviour {

    public float raycastLength = 12f;
    [HideInInspector]
    public float dis_l, dis_flO, dis_flT, dis_f, dis_frO, dis_frT, dis_r, dis_b;
    [HideInInspector]
    public RaycastHit hit_l, hit_flO, hit_flT, hit_f, hit_frO, hit_frT, hit_r, hit_b;
    [HideInInspector]
    public bool m_crash = false;

    public LayerMask layerMask;

    public int m_pastWaypointID = -1;
    private int m_waypointThreshold = 16;

    private Vector3 origin, left, frontLeftOne, frontLeftTwo, front, frontRightOne, frontRightTwo, right, back;
    private float heading;

    private UnityStandardAssets.Vehicles.Car.CarUserControl m_carControl;
    private Brains m_brains;

    private Dictionary<string, float> m_colliderInfo = new Dictionary<string, float> ();

    // Use this for initialization
    void Start () {
        m_carControl = GetComponent<UnityStandardAssets.Vehicles.Car.CarUserControl> ();
        m_brains = FindObjectOfType<Brains> ();
        origin = transform.position + Vector3.up * 0.2f;
        heading = transform.rotation.eulerAngles.y;

        float angle = heading / 180 * Mathf.PI;
        left = new Vector3 (origin.x - raycastLength * Mathf.Cos (angle), origin.y, origin.z + raycastLength * Mathf.Sin (angle));
        frontLeftOne = new Vector3 (origin.x - raycastLength * Mathf.Sin (angle - Mathf.PI / 4), origin.y, origin.z - raycastLength * Mathf.Cos (angle - Mathf.PI / 4));
        frontLeftTwo = new Vector3 (origin.x - raycastLength * Mathf.Sin (angle - Mathf.PI / 8), origin.y, origin.z - raycastLength * Mathf.Cos (angle - Mathf.PI / 8));
        front = new Vector3 (origin.x - raycastLength * Mathf.Sin (angle), origin.y, origin.z - raycastLength * Mathf.Cos (angle));
        frontRightOne = new Vector3 (origin.x - raycastLength * Mathf.Sin (angle + Mathf.PI / 4), origin.y, origin.z - raycastLength * Mathf.Cos (angle + Mathf.PI / 4));
        frontRightTwo = new Vector3 (origin.x - raycastLength * Mathf.Sin (angle + Mathf.PI / 8), origin.y, origin.z - raycastLength * Mathf.Cos (angle + Mathf.PI / 8));
        right = origin + origin - left;
        back = origin + origin - front;

        dis_l = 0.0f;
        dis_flO = 0.0f;
        dis_flT = 0.0f;
        dis_f = 0.0f;
        dis_frO = 0.0f;
        dis_frT = 0.0f;
        dis_r = 0.0f;
        dis_b = 0.0f;

        m_colliderInfo.Add ("Untagged", 0f);
        m_colliderInfo.Add ("Wall", 1f);
        m_colliderInfo.Add ("Waypoint", 2f);
    }
	
	// Update is called once per frame
	void Update () {
        origin = transform.position + Vector3.up * 0.5f;
        heading = transform.rotation.eulerAngles.y - 180f;

        float angle = heading / 180 * Mathf.PI;
        right = new Vector3 (origin.x - raycastLength * Mathf.Cos (angle), origin.y, origin.z + raycastLength * Mathf.Sin (angle));
        frontRightOne = new Vector3 (origin.x - raycastLength * Mathf.Sin (angle - Mathf.PI / 4), origin.y, origin.z - raycastLength * Mathf.Cos (angle - Mathf.PI / 4));
        frontRightTwo = new Vector3 (origin.x - raycastLength * Mathf.Sin (angle - Mathf.PI / 8), origin.y, origin.z - raycastLength * Mathf.Cos (angle - Mathf.PI / 8));
        front = new Vector3 (origin.x - raycastLength * Mathf.Sin (angle), origin.y, origin.z - raycastLength * Mathf.Cos (angle));
        frontLeftOne = new Vector3 (origin.x - raycastLength * Mathf.Sin (angle + Mathf.PI / 4), origin.y, origin.z - raycastLength * Mathf.Cos (angle + Mathf.PI / 4));
        frontLeftTwo = new Vector3 (origin.x - raycastLength * Mathf.Sin (angle + Mathf.PI / 8), origin.y, origin.z - raycastLength * Mathf.Cos (angle + Mathf.PI / 8));
        left = origin + origin - right;
        back = origin + origin - front;


        Physics.Linecast (origin, left, out hit_l, layerMask);
        Debug.DrawLine (origin, left, Color.red);
        
        Physics.Linecast (origin, frontLeftOne, out hit_flO, layerMask);
        Debug.DrawLine (origin, frontLeftOne, Color.cyan);

        Physics.Linecast (origin, frontLeftTwo, out hit_flT, layerMask);
        Debug.DrawLine (origin, frontLeftTwo, Color.cyan);

        Physics.Linecast (origin, front, out hit_f, layerMask);
        Debug.DrawLine (origin, front, Color.green);
        
        Physics.Linecast (origin, frontRightOne, out hit_frO, layerMask);
        Debug.DrawLine (origin, frontRightOne, Color.magenta);

        Physics.Linecast (origin, frontRightTwo, out hit_frT, layerMask);
        Debug.DrawLine (origin, frontRightTwo, Color.magenta);

        Physics.Linecast (origin, right, out hit_r, layerMask);
        Debug.DrawLine (origin, right, Color.blue);

        Physics.Linecast (origin, back, out hit_b, layerMask);
        Debug.DrawLine (origin, back, Color.white);

        dis_l = (hit_l.distance / raycastLength);
        dis_flO = (hit_flO.distance / raycastLength);
        dis_flT = (hit_flT.distance / raycastLength);
        dis_f = (hit_f.distance / raycastLength);
        dis_frO = (hit_frO.distance / raycastLength);
        dis_frT = (hit_frT.distance / raycastLength);
        dis_r = (hit_r.distance / raycastLength);
        dis_b = (hit_b.distance / raycastLength);
    }

    private float GetHitType (RaycastHit hit)
    {
        if (hit.collider == null) {
            return 0f;
        }

        return m_colliderInfo[hit.collider.tag];
    }

    private void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.tag != "Waypoint") {
            m_crash = true;
            m_carControl.crash = true;
        } else if (other.gameObject.tag == "Waypoint") {
            if (m_brains.m_waypoints.Count == 0) {
                return;
            }
            Waypoint waypoint = m_brains.m_waypoints.First (x => x.ID == other.GetComponent<Waypoint> ().ID);
            if (waypoint.ID > m_pastWaypointID && waypoint.ID - m_pastWaypointID < m_waypointThreshold) {
                m_pastWaypointID++;
                m_brains.DeactivateWaypoint (waypoint);
            }
        }
    }

    public void ResetCrash ()
    {
        m_crash = false;
        m_pastWaypointID = 0;
    }
}
