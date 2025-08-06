using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public float angle;
    [SerializeField] private Vector3 fwd;
    [SerializeField] private Track track;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Vector3 localEuler = transform.localRotation.eulerAngles;
        localEuler.z = angle;
        transform.rotation = Quaternion.Euler(localEuler);
        Gizmos.DrawLine(transform.position, transform.position + (transform.forward * 2));
        
        //Mathf.LerpAngle

        /*
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + (fwd.normalized * 2));

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (transform.up * 2));

        Quaternion rot = transform.rotation;
        transform.rotation = Quaternion.LookRotation(fwd, Vector3.ProjectOnPlane(transform.up, fwd).normalized);

        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, transform.position + (transform.rotation * Vector3.right * 2));
        */
    }
}
