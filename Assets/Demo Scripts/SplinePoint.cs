using UnityEngine;

[System.Serializable]
public class SplinePoint
{
    [SerializeField] private Transform transform;
    [SerializeField] private bool powerNext;
    [SerializeField] private bool powerPrevious;
    [SerializeField] private float speed;

    public Vector3 LocalPosition { get => transform.localPosition; set => transform.localPosition = value; }
    public Vector3 Position { get => transform.position; set => transform.position = value; }
    public Quaternion LocalRotation { get => transform.localRotation; set => transform.localRotation = value; }
    public Quaternion Rotation { get => transform.rotation; set => transform.rotation = value; }
    public bool PowerNext { get => powerNext; set => powerNext = value; }
    public bool PowerPrevious { get => powerPrevious; set => powerPrevious = value; }
    public float Speed { get => speed; set => speed = value; }
    public Transform Transform => transform;
    public GameObject GameObject => transform.gameObject;

    public SplinePoint(Transform transform, Vector3 position, bool powerSpline)
    {
        this.transform = transform;
        this.transform.localPosition = position;
        powerNext = powerSpline;
        powerPrevious = powerSpline;
        speed = -1f;
    }

    public SplinePoint(Transform transform, Vector3 position, Quaternion rotation, float speed)
    {
        this.transform = transform;
        this.transform.localPosition = position;
        this.transform.rotation = rotation;
        this.powerNext = false;
        this.powerPrevious = false;
        this.speed = speed;
    }

    public SplinePoint(Transform transform, Vector3 position, Quaternion rotation, bool powerSpline, float speed)
    {
        this.transform = transform;
        this.transform.localPosition = position;
        this.transform.rotation = rotation;
        this.powerNext = false;
        this.powerPrevious = powerSpline;
        this.speed = speed;
    }
}