using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Eyes : MonoBehaviour {

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

    private Vector3 originPosition, left, frontLeftOne, frontLeftTwo, front, frontRightOne, frontRightTwo, right, back;
    private float originRotation;

    private UnityStandardAssets.Vehicles.Car.CarUserControl m_carControl;
    private Brains m_brains;

    // Use this for initialization
    void Start () {
        m_carControl = GetComponent<UnityStandardAssets.Vehicles.Car.CarUserControl> ();
        m_brains = FindObjectOfType<Brains> ();
        originPosition = transform.position + Vector3.up * 0.2f;
        originRotation = transform.rotation.eulerAngles.y;

        dis_l = 0.0f;
        dis_flO = 0.0f;
        dis_flT = 0.0f;
        dis_f = 0.0f;
        dis_frO = 0.0f;
        dis_frT = 0.0f;
        dis_r = 0.0f;
        dis_b = 0.0f;
    }
	
	// Update is called once per frame
	void Update () {
        originPosition = transform.position + Vector3.up * 0.5f;
        originRotation = transform.rotation.eulerAngles.y - 180f;

        float angle = originRotation / 180 * Mathf.PI;
        right = new Vector3 (originPosition.x - raycastLength * Mathf.Cos (angle), originPosition.y, originPosition.z + raycastLength * Mathf.Sin (angle));
        frontRightOne = new Vector3 (originPosition.x - raycastLength * Mathf.Sin (angle - Mathf.PI / 4), originPosition.y, originPosition.z - raycastLength * Mathf.Cos (angle - Mathf.PI / 4));
        frontRightTwo = new Vector3 (originPosition.x - raycastLength * Mathf.Sin (angle - Mathf.PI / 8), originPosition.y, originPosition.z - raycastLength * Mathf.Cos (angle - Mathf.PI / 8));
        front = new Vector3 (originPosition.x - raycastLength * Mathf.Sin (angle), originPosition.y, originPosition.z - raycastLength * Mathf.Cos (angle));
        frontLeftOne = new Vector3 (originPosition.x - raycastLength * Mathf.Sin (angle + Mathf.PI / 4), originPosition.y, originPosition.z - raycastLength * Mathf.Cos (angle + Mathf.PI / 4));
        frontLeftTwo = new Vector3 (originPosition.x - raycastLength * Mathf.Sin (angle + Mathf.PI / 8), originPosition.y, originPosition.z - raycastLength * Mathf.Cos (angle + Mathf.PI / 8));
        left = originPosition + originPosition - right;
        back = originPosition + originPosition - front;

        Physics.Linecast (originPosition, left, out hit_l, layerMask);
        Vector3 l_lineEnd = (hit_l.collider == null) ? left : hit_l.point;
        Debug.DrawLine (originPosition, l_lineEnd, Color.red);
        
        Physics.Linecast (originPosition, frontLeftOne, out hit_flO, layerMask);
        Vector3 flO_lineEnd = (hit_flO.collider == null) ? frontLeftOne : hit_flO.point;
        Debug.DrawLine (originPosition, flO_lineEnd, Color.cyan);

        Physics.Linecast (originPosition, frontLeftTwo, out hit_flT, layerMask);
        Vector3 flT_lineEnd = (hit_flT.collider == null) ? frontLeftTwo : hit_flT.point;
        Debug.DrawLine (originPosition, flT_lineEnd, Color.cyan);

        Physics.Linecast (originPosition, front, out hit_f, layerMask);
        Vector3 f_lineEnd = (hit_f.collider == null) ? front : hit_f.point;
        Debug.DrawLine (originPosition, f_lineEnd, Color.green);
        
        Physics.Linecast (originPosition, frontRightOne, out hit_frO, layerMask);
        Vector3 frO_lineEnd = (hit_frO.collider == null) ? frontRightOne : hit_frO.point;
        Debug.DrawLine (originPosition, frO_lineEnd, Color.magenta);

        Physics.Linecast (originPosition, frontRightTwo, out hit_frT, layerMask);
        Vector3 frT_lineEnd = (hit_frT.collider == null) ? frontRightTwo : hit_frT.point;
        Debug.DrawLine (originPosition, frT_lineEnd, Color.magenta);

        Physics.Linecast (originPosition, right, out hit_r, layerMask);
        Vector3 r_lineEnd = (hit_r.collider == null) ? right : hit_r.point;
        Debug.DrawLine (originPosition, r_lineEnd, Color.blue);

        Physics.Linecast (originPosition, back, out hit_b, layerMask);
        Vector3 b_lineEnd = (hit_b.collider == null) ? back : hit_b.point;
        Debug.DrawLine (originPosition, b_lineEnd, Color.white);

        dis_l = (hit_l.distance / raycastLength);
        dis_flO = (hit_flO.distance / raycastLength);
        dis_flT = (hit_flT.distance / raycastLength);
        dis_f = (hit_f.distance / raycastLength);
        dis_frO = (hit_frO.distance / raycastLength);
        dis_frT = (hit_frT.distance / raycastLength);
        dis_r = (hit_r.distance / raycastLength);
        dis_b = (hit_b.distance / raycastLength);
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
        m_pastWaypointID = -1;
    }
}
