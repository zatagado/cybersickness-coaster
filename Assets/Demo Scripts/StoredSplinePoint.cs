using UnityEngine;

[System.Serializable]
public struct StoredSplinePoint
{
    public Vector3 position;
    public Quaternion rotation;
    public bool powerNext;
    public bool powerPrevious;
    public float speed;

    public StoredSplinePoint(Vector3 position, Quaternion rotation, 
        bool powerNext, bool powerPrevious, float speed)
    {
        this.position = position;
        this.rotation = rotation;
        this.powerNext = powerNext;
        this.powerPrevious = powerPrevious;
        this.speed = speed;
    }

    /*
    public static implicit operator bool(StoredSplinePoint storedSplinePoint)
    {
        return storedSplinePoint is not null;
    }

    public static StoredSplinePoint operator +(StoredSplinePoint a, StoredSplinePoint b) => throw new System.Exception();
    */
}
