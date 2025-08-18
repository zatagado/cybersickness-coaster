using UnityEngine;

/// <summary>
/// Look up table data structure for finding the "t" value along a Bézier curve given a target distance. There is no
/// formula to get the length of a Bézier curve given a value "t".
/// </summary>
[System.Serializable]
public class CumulativeDistanceLUT
{
    private const float IntervalT = 0.01f;
    private const float IntervalTargetDistance = 0.01f;
    [SerializeField] private float intervalDistance = 0.0f;

    public float[] lut;

    public float CurveLength => (lut.Length - 1) * intervalDistance;

    /// <summary>
    /// Constructor for the 
    /// </summary>
    /// <param name="spline"></param>
    /// <param name="startIndex"></param>
    public CumulativeDistanceLUT(TrackSpline spline, int startIndex)
    {
        int arrayLength = Mathf.CeilToInt(1.0f / IntervalT);

        lut = new float[arrayLength + 1];

        Vector3 previousPosition = spline.GetPosition(startIndex, 0.0f);

        lut[0] = 0.0f;

        for (int i = 1; i < arrayLength; i++)
        {
            float t = IntervalT * i;
            Vector3 currPosition = spline.GetPosition(startIndex, t);
            float distance = Vector3.Distance(previousPosition, currPosition);
            lut[i] = lut[i - 1] + distance;
            previousPosition = currPosition;
        }

        lut[arrayLength] = lut[arrayLength - 1] + Vector3.Distance(previousPosition, spline.GetPosition(startIndex, 1.0f)); // last point must be t = 1.0f

        lut = UseDistanceIntervals(lut);
    }

    // credit freya holmer for most of this method
    private float[] UseDistanceIntervals(float[] lut)
    {
        float arcLength = lut[^1];

        int divisions = Mathf.CeilToInt(arcLength / IntervalTargetDistance);
        intervalDistance = arcLength / divisions;

        float[] tByDistanceLut = new float[divisions + 1];

        tByDistanceLut[0] = 0.0f;

        int j = 1;
        for (int i = 1; i < divisions; i++)
        {
            float distance = intervalDistance * i; // distance trying to find a t for

            for (; distance > lut[j]; j++); // keep incrementing j until it distance is less than or equal to the value in the Look up table

            float lerpBetweenTs = Mathf.InverseLerp(lut[j - 1], lut[j], distance);
            tByDistanceLut[i] = Mathf.Lerp((j - 1) * IntervalT, j * IntervalT, lerpBetweenTs);
        }

        tByDistanceLut[^1] = 1.0f;

        return tByDistanceLut;
    }

    public float GetTFromDistance(float distance)
    {
        float arcLength = (lut.Length - 1) * intervalDistance;

        if (distance >= arcLength)
        {
            return 1.0f;
        }
        else
        {
            // find the lower and upper index that the distance fits into the lut
            int lowerIndex = (int)(distance / intervalDistance);
            int upperIndex = lowerIndex + 1;

            float lerpBetweenDistances = Mathf.InverseLerp(intervalDistance * lowerIndex, intervalDistance * upperIndex, distance); // t value between two distances

            return Mathf.Lerp(lut[lowerIndex], lut[upperIndex], lerpBetweenDistances);
        }
    }
}
