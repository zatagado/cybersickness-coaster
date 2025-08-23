using UnityEngine;

/// <summary>
/// Serializable struct for storing a spline point in the Track scriptable object.
/// </summary>
[System.Serializable]
public struct StoredSplinePoint
{
    public Vector3 position;
    public Quaternion rotation;
    public bool powerNext;
    public bool powerPrevious;
    public float speed;

    /// <summary>
    /// Constructor for the StoredSplinePoint struct.
    /// </summary>
    /// <param name="position">The position of the spline point.</param>
    /// <param name="rotation">The rotation of the spline point.</param>
    /// <param name="powerNext">Whether the track between this point and the next point is powered (overrides acceleration).</param>
    /// <param name="powerPrevious">Whether the track between this point and the previous point is powered (overrides acceleration).</param>
    /// <param name="speed">The speed of the spline point, if it is a powered point.</param>
    public StoredSplinePoint(Vector3 position, Quaternion rotation, 
        bool powerNext, bool powerPrevious, float speed)
    {
        this.position = position;
        this.rotation = rotation;
        this.powerNext = powerNext;
        this.powerPrevious = powerPrevious;
        this.speed = speed;
    }
}
