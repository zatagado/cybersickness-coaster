using UnityEngine;

[System.Serializable]
public class CoasterTrain
{
    public Transform movingObject;

    public float speed;
    [HideInInspector] public Vector3 velocity;
    public float ipSpeed;
    [HideInInspector] public Vector3 ipVelocity; // Initial velocity upon reaching a powered section

    public int currLoop; // Set initially to 1

    public int curveIndex;
    public float curveDistance;
    [HideInInspector] public bool iPowered; // Boolean that helps get te initial velocity
    [HideInInspector] public int ipCurveIndex; // Initial powered curve Index

    // Test variables
    [HideInInspector] public Vector3 normal;
    [HideInInspector] public Vector3 cDirection;

    public CoasterTrain(Transform movingObject)
    {
        this.movingObject = movingObject;
        speed = 0f;
        velocity = Vector3.zero;
        ipSpeed = 0f;
        ipVelocity = Vector3.zero;
        currLoop = 1;
        curveIndex = 0;
        curveDistance = 0.0f;
        iPowered = false;
        ipCurveIndex = 0;
        normal = Vector3.zero;
        cDirection = Vector3.zero;
    }

    public void Reset()
    {
        speed = 0f;
        velocity = Vector3.zero;
        ipSpeed = 0f;
        ipVelocity = Vector3.zero;
        currLoop = 1;
        curveIndex = 0;
        curveDistance = 0.0f;
        iPowered = false;
        ipCurveIndex = 0;
        normal = Vector3.zero;
        cDirection = Vector3.zero;
    }
}