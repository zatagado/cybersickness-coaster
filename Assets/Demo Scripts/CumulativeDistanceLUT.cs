using UnityEngine;

[System.Serializable]
public class CumulativeDistanceLUT
{
    private const float IntervalT = 0.01f;
    private const float IntervalTargetDist = 0.01f;
    [SerializeField] private float intervalDistance = 0.0f;

    public float[] LUT;

    public float CurveLength => (LUT.Length - 1) * intervalDistance;

    public CumulativeDistanceLUT(TrackSpline spline, int startIndex)
    {
        int arrayLength = Mathf.CeilToInt(1.0f / IntervalT);

        LUT = new float[arrayLength + 1];

        Vector3 prevPosition = spline.GetPosition(startIndex, 0.0f);

        LUT[0] = 0.0f;

        for (int i = 1; i < arrayLength; i++)
        {
            float t = IntervalT * i;
            Vector3 currPosition = spline.GetPosition(startIndex, t);
            float distance = Vector3.Distance(prevPosition, currPosition);
            LUT[i] = LUT[i - 1] + distance;
            prevPosition = currPosition;
        }

        LUT[arrayLength] = LUT[arrayLength - 1] + Vector3.Distance(prevPosition, spline.GetPosition(startIndex, 1.0f)); // last point must be t = 1.0f

        LUT = UseDistanceIntervals(LUT);
    }

    // credit freya holmer for most of this method
    private float[] UseDistanceIntervals(float[] LUT)
    {
        float arcLength = LUT[LUT.Length - 1];

        int divisions = Mathf.CeilToInt(arcLength / IntervalTargetDist);
        intervalDistance = arcLength / divisions;

        float[] tByDistanceLUT = new float[divisions + 1];

        tByDistanceLUT[0] = 0.0f;

        int j = 1;
        for (int i = 1; i < divisions; i++)
        {
            float distance = intervalDistance * i; // distance trying to find a t for

            for (; distance > LUT[j]; j++) ; // keep incrementing j until it distance is less than or equal to the value in the Look up table

            float lerpBetweenTs = Mathf.InverseLerp(LUT[j - 1], LUT[j], distance);
            tByDistanceLUT[i] = Mathf.Lerp((j - 1) * IntervalT, j * IntervalT, lerpBetweenTs);
        }

        tByDistanceLUT[tByDistanceLUT.Length - 1] = 1.0f;

        return tByDistanceLUT;
    }

    public float GetTFromDistance(float distance)
    {
        float arcLength = (LUT.Length - 1) * intervalDistance;

        if (distance >= arcLength)
        {
            return 1.0f;
        }
        else
        {
            // find the lower and upper index that the distance fits into the LUT
            int lowerIndex = (int)(distance / intervalDistance);
            int upperIndex = lowerIndex + 1;

            float lerpBetweenDists = Mathf.InverseLerp(intervalDistance * lowerIndex, intervalDistance * upperIndex, distance); // t value between two distances

            return Mathf.Lerp(LUT[lowerIndex], LUT[upperIndex], lerpBetweenDists);
        }
    }
}
