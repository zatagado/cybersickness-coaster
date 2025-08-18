using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Represents a spline consisting of points.
/// </summary>
[Serializable]
[ExecuteAlways]
public class TrackSpline : MonoBehaviour
{
    public Track track;

    [HideInInspector] public int selectedIndex = 0;

    // TODO change all of these to properties
    public Vector3 SelectedPosition { get; set; }
    [HideInInspector] public Vector3 selectedPosition;
    /// <summary>
    /// Rotation around a rail track, facing towards the front of the track, in degrees.
    /// </summary>
    public float SelectedRailRotation { get; set; }
    [HideInInspector] public float selectedRailRotation;
    public bool SelectedPowerNext { get; set; }
    [HideInInspector] public bool selectedPowerNext;
    public bool SelectedPowerPrevious { get; set; }
    [HideInInspector] public bool selectedPowerPrevious;
    public bool SelectedSpeed { get; set; }
    [HideInInspector] public float selectedSpeed;

    public float Gravity { get; set; } = 10;
    [HideInInspector] public float gravity = 10;
    [HideInInspector] public bool continuousLoop;
    [HideInInspector] public int loops;
    [HideInInspector] public SplinePoint[] points;
    [HideInInspector] public CumulativeDistanceLUT[] curveLookUpTables;

    /// <summary>
    /// Resets the spline to four points.
    /// </summary>
    public void Reset() // Fix problem when script is reset and custom editor selected index is out of bounds 
    {
        selectedIndex = 0;
        GameObject[] children = new GameObject[transform.childCount];
        int i = 0;
        foreach (Transform child in transform)
        {
            children[i++] = child.gameObject;
        }
        foreach (GameObject child in children)
        {
            DestroyImmediate(child.gameObject);
        }

        Transform transformA = new GameObject("Point 0").transform;
        transformA.SetParent(transform);

        Transform transformB = new GameObject("Point 1").transform;
        transformB.SetParent(transform);

        Transform transformC = new GameObject("Point 2").transform;
        transformC.SetParent(transform);

        Transform transformD = new GameObject("Point 3").transform;
        transformD.SetParent(transform);

        points = new SplinePoint[]
        {
            new SplinePoint(transformA, new Vector3(1, 0, 0), Quaternion.identity, false, -1f),
            new SplinePoint(transformB, new Vector3(2, 0, 0), false),
            new SplinePoint(transformC, new Vector3(3, 0, 0), false),
            new SplinePoint(transformD, new Vector3(4, 0, 0), Quaternion.identity, false, -1f),
        };

        Vector3 direction = (points[1].Position - points[0].Position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        points[3].Rotation = rotation;
        points[0].Rotation = rotation;
        UpdateSelected(0);
    }

    /// <summary>
    /// Adds a curve of three more points.
    /// </summary>
    public void AddCurve()
    {
        Array.Resize(ref points, points.Length + 3);
        Vector3 previousLastPosition = points[^4].LocalPosition; // TODO rename this
        Vector3 previous2ToLastPosition = points[^5].LocalPosition; // TODO rename this
        Vector3 direction = (previousLastPosition - previous2ToLastPosition).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

        Transform transformA = new GameObject("Point " + (points.Length - 3)).transform;
        transformA.SetParent(transform);

        Transform transformB = new GameObject("Point " + (points.Length - 2)).transform;
        transformB.SetParent(transform);

        Transform transformC = new GameObject("Point " + (points.Length - 1)).transform;
        transformC.SetParent(transform);

        bool powerPrevious = points[^4].PowerPrevious;
        SplinePoint endPoint = points[^4];
        endPoint.PowerNext = powerPrevious;
        points[^3] = new SplinePoint(transformA, previousLastPosition + (direction), powerPrevious);
        points[^2] = new SplinePoint(transformB, previousLastPosition + (direction * 2), powerPrevious);
        points[^1] = new SplinePoint(transformC, previousLastPosition + (direction * 3), rotation, powerPrevious, endPoint.Speed);
    }

    /// <summary>
    /// Adds a curve of three points after a certain point in the spline.
    /// </summary>
    /// <param name="startIndex">The index along the track</param>
    public void AddCurve(int startIndex)
    {
        Array.Resize(ref points, points.Length + 3);
        Vector3 midDirection = GetDirection(startIndex, 0.5f);
        Quaternion rotation = GetRotation(startIndex, 0.5f);

        Vector3 midPoint = transform.InverseTransformPoint(GetPosition(startIndex, 0.5f));
        float distanceBetween = Vector3.Distance(points[startIndex + 3].LocalPosition, points[startIndex].LocalPosition);

        for (int i = points.Length - 4; i > startIndex + 1; i--)
        {
            points[i].GameObject.name = "Point " + (i + 3);
            points[i + 3] = points[i];
        }

        Transform transformA = new GameObject("Point " + (startIndex + 2)).transform;
        transformA.SetParent(transform);

        Transform transformB = new GameObject("Point " + (startIndex + 3)).transform;
        transformB.SetParent(transform);

        Transform transformC = new GameObject("Point " + (startIndex + 4)).transform;
        transformC.SetParent(transform);

        bool powerSpline = points[startIndex].PowerNext;
        points[startIndex + 2] = new SplinePoint(transformA, midPoint + (midDirection * distanceBetween * -0.2f), powerSpline);
        points[startIndex + 3] = new SplinePoint(transformC, midPoint, rotation, powerSpline, 
            Mathf.Lerp(points[startIndex].Speed, points[startIndex + 3].Speed, 0.5f));
        points[startIndex + 3].PowerNext = powerSpline;
        points[startIndex + 4] = new SplinePoint(transformB, midPoint + (midDirection * distanceBetween * 0.2f), powerSpline);
    }

    /// <summary>
    /// Removes the three-point curve at the end of the spline.
    /// </summary>
    public void RemoveCurve()
    {
        DestroyImmediate(points[^3].GameObject);
        DestroyImmediate(points[^2].GameObject);
        DestroyImmediate(points[^1].GameObject);
        Array.Resize(ref points, points.Length - 3);
    }

    /// <summary>
    /// Removes a three-point Bézier curve a certain point given a start index value. Used for removing curves in the
    /// middle of a spline.
    /// </summary>
    /// <param name="startIndex">The start index along a spline of the Bézier curve you are trying to remove.</param>
    public void RemoveCurve(int startIndex)
    {
        // TODO check if destroyimmediate is actually needed, but it is probably fine since this is used in the editor
        DestroyImmediate(points[startIndex + 1].GameObject);
        DestroyImmediate(points[startIndex].GameObject);
        DestroyImmediate(points[startIndex - 1].GameObject);

        for (int i = startIndex + 2; i < points.Length; i++)
        {
            points[i].GameObject.name = $"Point {i - 3}";
            points[i - 3] = points[i];
        }
        Array.Resize(ref points, points.Length - 3);
    }

    /// <summary>
    /// Updates the values displayed in the inspector GUI to the actual values of the SplinePoint.
    /// </summary>
    /// <param name="index"></param>
    public void UpdateSelected(int index)
    {
        SplinePoint point = points[index];
        selectedPosition = point.LocalPosition;

        selectedRailRotation = point.LocalRotation.eulerAngles.z;

        selectedPowerNext = point.PowerNext;

        if (index == 0 && continuousLoop)
        {
            selectedPowerPrevious = points[^1].PowerPrevious;
        }
        else
        {
            selectedPowerPrevious = point.PowerPrevious;
        }

        selectedSpeed = point.Speed;
    }

    /// <summary>
    /// Updates the actual values of the SplinePoint to the values displayed in the inspector GUI.
    /// </summary>
    /// <param name="index"></param>
    public void EditSelected(int index)
    {
        SplinePoint point = points[index];
        point.LocalPosition = selectedPosition;

        if (index % 3 == 0)
        {
            Vector3 localEuler = point.LocalRotation.eulerAngles;
            localEuler.z = selectedRailRotation;
            point.LocalRotation = Quaternion.Euler(localEuler);

            if (point.PowerNext != selectedPowerNext)
            {
                point.PowerNext = selectedPowerNext;
                points[index + 1].PowerNext = selectedPowerNext;
                points[index + 1].PowerPrevious = selectedPowerNext;
                points[index + 2].PowerNext = selectedPowerNext;
                points[index + 2].PowerPrevious = selectedPowerNext;
                points[index + 3].PowerPrevious = selectedPowerNext;
            }

            int lastIndex = points.Length - 1;
            if (index == 0)
            {
                if (points[lastIndex].PowerPrevious != selectedPowerPrevious)
                {
                    points[lastIndex].PowerPrevious = selectedPowerPrevious;
                    points[lastIndex - 1].PowerNext = selectedPowerPrevious;
                    points[lastIndex - 1].PowerPrevious = selectedPowerPrevious;
                    points[lastIndex - 2].PowerNext = selectedPowerPrevious;
                    points[lastIndex - 2].PowerPrevious = selectedPowerPrevious;
                    points[lastIndex - 3].PowerNext = selectedPowerPrevious;
                }
            }
            else if (point.PowerPrevious != selectedPowerPrevious)
            {
                point.PowerPrevious = selectedPowerPrevious;
                points[index - 1].PowerNext = selectedPowerPrevious;
                points[index - 1].PowerPrevious = selectedPowerPrevious;
                points[index - 2].PowerNext = selectedPowerPrevious;
                points[index - 2].PowerPrevious = selectedPowerPrevious;
                points[index - 3].PowerNext = selectedPowerPrevious;
            }
            point.Speed = selectedSpeed;
        }
        else // Otherwise not one of the end points
        {
            if (point.PowerNext != selectedPowerNext)
            {
                point.PowerNext = selectedPowerNext;
                point.PowerPrevious = selectedPowerNext;

                if (index % 3 == 1) // The first control point
                {
                    points[index - 1].PowerNext = selectedPowerNext;
                    points[index + 1].PowerNext = selectedPowerNext;
                    points[index + 1].PowerPrevious = selectedPowerNext;
                    points[index + 2].PowerPrevious = selectedPowerNext;
                }
                else // The second control point
                {
                    points[index - 2].PowerNext = selectedPowerNext;
                    points[index - 1].PowerNext = selectedPowerNext;
                    points[index - 1].PowerPrevious = selectedPowerNext;
                    points[index + 1].PowerPrevious = selectedPowerNext;
                }
            }
        }
    }

    /// <summary>
    /// Changes the position and rotation of the transform of a point on the spline.
    /// </summary>
    /// <param name="index"></param>
    public void EditTransform(int index)
    {
        SplinePoint point = points[index];
        point.LocalPosition = selectedPosition;

        if (index % 3 == 0)
        {
            Vector3 localEuler = point.LocalRotation.eulerAngles;
            localEuler.z = selectedRailRotation;
            point.LocalRotation = Quaternion.Euler(localEuler);

            if (index == 0 && continuousLoop)
            {
                points[points.Length - 1].LocalRotation = point.LocalRotation;
            }
        }
    }

    /// <summary>
    /// Changes the spline power values for an end point on the spline.
    /// </summary>
    /// <param name="index"></param>
    public void EditPowerEndPoint(int index)
    {
        SplinePoint point = points[index];
        int lastIndex = points.Length - 1;

        if (point.PowerNext != selectedPowerNext) // Power Next
        {
            point.PowerNext = selectedPowerNext;
            points[index + 1].PowerNext = selectedPowerNext;
            points[index + 1].PowerPrevious = selectedPowerNext;
            points[index + 2].PowerNext = selectedPowerNext;
            points[index + 2].PowerPrevious = selectedPowerNext;
            points[index + 3].PowerPrevious = selectedPowerNext;

            if (continuousLoop)
            {
                if (index == 0)
                {
                    points[lastIndex].PowerNext = selectedPowerNext;
                }
                else if (index == lastIndex - 3)
                {
                    points[0].PowerPrevious = selectedPowerNext;
                }
            }
        }
        else if (point.PowerPrevious != selectedPowerPrevious)
        {
            if (index == 0) // Power previous is only visible on 0 if continuous loop
            {
                points[0].PowerPrevious = selectedPowerPrevious;
                points[lastIndex].PowerPrevious = selectedPowerPrevious;
                points[lastIndex - 1].PowerNext = selectedPowerPrevious;
                points[lastIndex - 1].PowerPrevious = selectedPowerPrevious;
                points[lastIndex - 2].PowerNext = selectedPowerPrevious;
                points[lastIndex - 2].PowerPrevious = selectedPowerPrevious;
                points[lastIndex - 3].PowerNext = selectedPowerPrevious;
            }
            else
            {
                point.PowerPrevious = selectedPowerPrevious; // point ~3
                points[index - 1].PowerNext = selectedPowerPrevious; // point ~2
                points[index - 1].PowerPrevious = selectedPowerPrevious; // point ~2
                points[index - 2].PowerNext = selectedPowerPrevious; // point ~1
                points[index - 2].PowerPrevious = selectedPowerPrevious; // point ~1
                points[index - 3].PowerNext = selectedPowerPrevious; // point ~0

                if (index == 3 && continuousLoop)
                {
                    points[lastIndex].PowerNext = selectedPowerPrevious;
                }
            }

        }
    }

    /// <summary>
    /// Changes the power values for a control point on the spline.
    /// </summary>
    /// <param name="index"></param>
    public void EditPowerControlPoint(int index)
    {
        SplinePoint point = points[index];

        if (point.PowerNext != selectedPowerNext)
        {
            point.PowerNext = selectedPowerNext;
            point.PowerPrevious = selectedPowerNext;

            if (index % 3 == 1) // The first control point
            {
                points[index - 1].PowerNext = selectedPowerNext;
                points[index + 1].PowerNext = selectedPowerNext;
                points[index + 1].PowerPrevious = selectedPowerNext;
                points[index + 2].PowerPrevious = selectedPowerNext;
            }
            else // The second control point
            {
                points[index - 2].PowerNext = selectedPowerNext;
                points[index - 1].PowerNext = selectedPowerNext;
                points[index - 1].PowerPrevious = selectedPowerNext;
                points[index + 1].PowerPrevious = selectedPowerNext;
            }
        }
    }

    /// <summary>
    /// Changes the speed values for a point on the spline.
    /// </summary>
    /// <param name="index"></param>
    public void EditSpeed(int index)
    {
        points[index].Speed = selectedSpeed;

        if (index == 0 && continuousLoop)
        {
            points[points.Length - 1].Speed = selectedSpeed;
        }
    }

    /// <summary>
    /// Gets the position of a point on the bezier curve given a 0 - 1 decimal value t.
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="t"></param>
    /// <returns>A point on the bezier curve.</returns>
    public Vector3 GetPosition(int startIndex, float t)
    {
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * oneMinusT * points[startIndex].Position + 3f * oneMinusT * oneMinusT * t * points[startIndex + 1].Position +
            3f * oneMinusT * t * t * points[startIndex + 2].Position + t * t * t * points[startIndex + 3].Position;
    }

    /// <summary>
    /// Gets the direction of the curve at a certain point given a 0 - 1 decimal value t.
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="t"></param>
    /// <returns>A direction on the bezier curve.</returns>
    public Vector3 GetDirection(int startIndex, float t)
    {
        float oneMinusT = 1f - t;
        return (3f * oneMinusT * oneMinusT * (points[startIndex + 1].Position - points[startIndex].Position) +
            6f * oneMinusT * t * (points[startIndex + 2].Position - points[startIndex + 1].Position) +
            3f * t * t * (points[startIndex + 3].Position - points[startIndex + 2].Position)).normalized;
    }

    /// <summary>
    /// Gets on the rotation of the curve at a certain point given a 0 - 1 decimal position t.
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="t"></param>
    /// <returns>A rotation on the bezier curve.</returns>
    public Quaternion GetRotation(int startIndex, float t)
    {
        return Quaternion.Slerp(points[startIndex].Rotation, points[startIndex + 3].Rotation, t);
    }

    /// <summary>
    /// Gets on the rotation of the curve at a certain point given a 0 - 1 decimal position t and a direction to rotate around.
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="t"></param>
    /// <param name="cDirection"></param>
    /// <returns>A rotation on the bezier curve.</returns>
    public Quaternion GetRotation(int startIndex, float t, Vector3 cDirection)
    {
        Vector3 localEuler = Quaternion.LookRotation(cDirection).eulerAngles;
        localEuler.z = Mathf.LerpAngle(points[startIndex].Rotation.eulerAngles.z, points[startIndex + 3].Rotation.eulerAngles.z, 
            Mathf.SmoothStep(0, 1, t));
        // Debug.Log(Mathf.SmoothStep(0, 1, t));
        // Mathf.Sin(t * (Mathf.PI / 2)));
        // Debug.Log(Mathf.Sin(t * (Mathf.PI / 2)));
        return Quaternion.Euler(localEuler);
    }

    /// <summary>
    /// Saves the path and settings of a track to the Track scriptable object.
    /// </summary>
    public void SaveTrack()
    {
        track.Gravity = gravity;
        track.ContinuousLoop = continuousLoop;
        track.Loops = loops;

        StoredSplinePoint[] storedPoints = new StoredSplinePoint[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            SplinePoint point = points[i];
            StoredSplinePoint storedPoint = new StoredSplinePoint(point.LocalPosition, point.Rotation,
                point.PowerNext, point.PowerPrevious, point.Speed);
            storedPoints[i] = storedPoint;
        }
        track.Points = storedPoints;

        CumulativeDistanceLUT[] curveLUTs = new CumulativeDistanceLUT[points.Length / 3];
        for (int i = 0; i < points.Length / 3; i++)
        {
            curveLUTs[i] = new CumulativeDistanceLUT(this, i * 3);
        }
        track.CurveLookUpTables = curveLUTs;
        curveLookUpTables = curveLUTs;
        // track.Lengths = lengths;
#if UNITY_EDITOR
        EditorUtility.SetDirty(track);
#endif
    }

    /// <summary>
    /// Loads track path and settings from the current scriptable object Track.
    /// </summary>
    public void LoadTrack()
    {
        foreach (SplinePoint point in points)
        {
            DestroyImmediate(point.GameObject);
        }

        gravity = track.Gravity;
        continuousLoop = track.ContinuousLoop;
        loops = track.Loops;

        points = new SplinePoint[track.Points.Length];
        for (int i = 0; i < track.Points.Length; i++)
        {
            Transform splinePointTransform = new GameObject("Point " + i).transform;
            splinePointTransform.SetParent(transform);
            StoredSplinePoint point = track.Points[i];
            points[i] = new SplinePoint(splinePointTransform, point.position, point.rotation, point.speed);
            points[i].PowerNext = point.powerNext;
            points[i].PowerPrevious = point.powerPrevious;
        }

        curveLookUpTables = track.CurveLookUpTables;

        selectedIndex = 0;
        UpdateSelected(0);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < (points.Length / 3); i++)
        {
            DisplayCurve(i);
        }
        for (int i = 0; i < points.Length; i += 3)
        {
            Gizmos.color = Color.black;
            SplinePoint endPoint = points[i];
            Gizmos.DrawLine(endPoint.Position, endPoint.Position + (endPoint.Transform.up * 0.5f));
        }
    }

    /// <summary>
    /// Displays a curve.
    /// </summary>
    /// <param name="curveNum"></param>
    private void DisplayCurve(int curveNum)
    {
        int startIndex = curveNum * 3;
        Vector3[] transformedPositions = new Vector3[]
        {
            transform.TransformPoint(points[startIndex].LocalPosition),
            transform.TransformPoint(points[startIndex + 1].LocalPosition),
            transform.TransformPoint(points[startIndex + 2].LocalPosition),
            transform.TransformPoint(points[startIndex + 3].LocalPosition)
        };

#if UNITY_EDITOR
        if (points[startIndex].PowerNext) // The current spline is powered
        {
            Handles.DrawBezier(transformedPositions[0], transformedPositions[3], transformedPositions[1], transformedPositions[2], Color.red, null, 2);
        }
        else
        {
            Handles.DrawBezier(transformedPositions[0], transformedPositions[3], transformedPositions[1], transformedPositions[2], Color.white, null, 2);
        }
#endif
    }
}
