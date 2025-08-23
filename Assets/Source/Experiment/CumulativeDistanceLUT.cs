using UnityEngine;

/// <summary>
/// Look up table data structure for finding the "t" value along a Bézier curve given a target distance. There is no formula to get the length of a
/// Bézier curve given a value "t".
/// 
/// Credit: Freya Holmer 
/// The Beauty of Bezier Curves - https://youtu.be/aVwxzDHniEw?si=hJ-wBKQBCPfJn5Rq
/// </summary>
[System.Serializable]
public class CumulativeDistanceLUT
{
    private const float IntervalT = 0.01f;
    private const float IntervalTargetDistance = 0.01f;
    private float intervalActualDistance;

    public float[] lut;

    /// <summary>
    /// Getter for the length of the curve.
    /// </summary>
    public float CurveLength => (lut.Length - 1) * intervalActualDistance;

    /// <summary>
    /// Constructor for the cumulative distance look up table.
    /// </summary>
    /// <param name="spline">Roller coaster track spline.</param>
    /// <param name="startIndex">Index of the first point of the curve along the track spline.</param>
    public CumulativeDistanceLUT(TrackSpline spline, int startIndex)
    {
        // Each float in this array is the cumulative distance from t = 0 to t = IntervalT * the current index.
        // The length of the array is (1.0f / IntervalT) + 1.
        // UseDistanceIntervals() is used to convert this array to look up by t value to get the cumulative distance.
        int arrayLength = Mathf.CeilToInt(1.0f / IntervalT);

        lut = new float[arrayLength + 1];

        Vector3 previousPosition = spline.GetPosition(startIndex, 0.0f);

        lut[0] = 0.0f;

        for (int i = 1; i < arrayLength; i++)
        {
            float t = IntervalT * i;
            Vector3 currentPosition = spline.GetPosition(startIndex, t);
            float distance = Vector3.Distance(previousPosition, currentPosition);
            lut[i] = lut[i - 1] + distance;
            previousPosition = currentPosition;
        }

        lut[arrayLength] = lut[arrayLength - 1] + Vector3.Distance(previousPosition, spline.GetPosition(startIndex, 1.0f)); // last point must be t = 1.0f

        lut = UseDistanceIntervals(lut);
    }

    /// <summary>
    /// Converts a look up table from look up t value to get cumulative distance to look up distance to get t value.
    /// </summary>
    /// <param name="lut">Look up table from t value to get cumulative distance.</param>
    /// <returns>Look up table from distance to get t value.</returns>
    private float[] UseDistanceIntervals(float[] lut)
    {
        float arcLength = lut[^1];

        int divisions = Mathf.CeilToInt(arcLength / IntervalTargetDistance);
        intervalActualDistance = arcLength / divisions;

        float[] tByDistanceLut = new float[divisions + 1];

        tByDistanceLut[0] = 0.0f;

        int j = 1;
        for (int i = 1; i < divisions; i++)
        {
            float distance = intervalActualDistance * i; // distance trying to find a t for

            for (; distance > lut[j]; j++); // keep incrementing j until it distance is less than or equal to the value in the Look up table

            float lerpBetweenTs = Mathf.InverseLerp(lut[j - 1], lut[j], distance);
            tByDistanceLut[i] = Mathf.Lerp((j - 1) * IntervalT, j * IntervalT, lerpBetweenTs);
        }

        tByDistanceLut[^1] = 1.0f;

        return tByDistanceLut;
    }

    /// <summary>
    /// Get the "t" value along the curve given a target distance.
    /// </summary>
    /// <param name="distance">Target distance along the curve.</param>
    /// <returns>The "t" value along the curve.</returns>
    public float GetTFromDistance(float distance)
    {
        float arcLength = (lut.Length - 1) * intervalActualDistance;

        if (distance >= arcLength)
        {
            return 1.0f;
        }
        else
        {
            // find the lower and upper index that the distance fits into the lut
            int lowerIndex = (int)(distance / intervalActualDistance);
            int upperIndex = lowerIndex + 1;

            float lerpBetweenDistances = Mathf.InverseLerp(intervalActualDistance * lowerIndex, intervalActualDistance * upperIndex, distance); // t value between two distances

            return Mathf.Lerp(lut[lowerIndex], lut[upperIndex], lerpBetweenDistances);
        }
    }
}
