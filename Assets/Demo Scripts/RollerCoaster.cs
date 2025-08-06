using UnityEngine;

[RequireComponent(typeof(TrackSpline))]
public class RollerCoaster : MonoBehaviour
{
    [SerializeField] private Track[] tracks;
    private TrackSpline spline;

    private void Start()
    {
        spline = GetComponent<TrackSpline>();
    }

    public void SetUp(int trackIndex)
    {
        spline.track = tracks[trackIndex];
        spline.LoadTrack();
    }

    public void StartCoaster(CoasterTrain coaster)
    {
        coaster.speed = 0.75f;
        //coaster.curveIndex = 0;
        //coaster.curveDistance = 0;

        if (spline.points[0].PowerNext)
        {
            MovePowered(coaster);
        }
        else
        {
            MoveGravity(coaster);
        }
    }

    public bool Move(CoasterTrain coaster)
    {
        if (!SetCurveAndDistance(coaster))
        {
            if (spline.points[coaster.curveIndex * 3].PowerNext)
            {
                if (coaster.iPowered)
                {
                    coaster.ipSpeed = coaster.speed;
                    coaster.ipVelocity = coaster.velocity;
                    coaster.ipCurveIndex = coaster.curveIndex;
                    coaster.iPowered = false;

                    int currCurveIndex = coaster.curveIndex;
                    if (spline.points[(currCurveIndex + 1) * 3].PowerNext) // do not power across loop
                    {
                        float distance = spline.curveLookUpTables[currCurveIndex].CurveLength;
                        currCurveIndex++;
                        while (spline.points[(currCurveIndex + 1) * 3].PowerNext)
                        {
                            distance += spline.curveLookUpTables[currCurveIndex].CurveLength;
                            currCurveIndex++;
                        }
                        distance += spline.curveLookUpTables[currCurveIndex].CurveLength;
                        float targetSpeed = spline.points[(currCurveIndex + 1) * 3].Speed;
                        float acceleration = ((targetSpeed * targetSpeed) - (coaster.ipSpeed * coaster.ipSpeed)) / (2 * distance);

                        while (currCurveIndex > coaster.curveIndex)
                        {
                            distance -= spline.curveLookUpTables[currCurveIndex].CurveLength;
                            spline.points[currCurveIndex * 3].Speed = Mathf.Sqrt((coaster.ipSpeed * coaster.ipSpeed) + (2 * acceleration * distance));
                            currCurveIndex--;
                        }
                    }
                }

                MovePowered(coaster);
            }
            else
            {
                //coaster.iPowered = true;
                MoveGravity(coaster);
            }
            
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Sets the curve that the player should currently be on and the distance they have traveled. 
    /// </summary>
    /// <param name="coaster"></param>
    /// <returns>Returns true if the player has finished the track and false otherwise.</returns>
    private bool SetCurveAndDistance(CoasterTrain coaster)
    {
        float deltaDistance = coaster.speed * Time.deltaTime; // calculate how far it should travel

        coaster.curveDistance += deltaDistance;

        while (coaster.curveDistance > spline.curveLookUpTables[coaster.curveIndex].CurveLength)
        {
            coaster.iPowered = true;
            if (coaster.curveIndex + 1 >= spline.points.Length / 3) // coaster traveled an entire loop
            {
                if (coaster.currLoop++ < spline.loops && spline.continuousLoop)
                {
                    coaster.curveDistance -= spline.curveLookUpTables[coaster.curveIndex].CurveLength; // subtract the distance of the curve and go to the 0th curve
                    coaster.curveIndex = 0;
                }
                else // Otherwise end the traversal
                {
                    SplinePoint point = spline.points[spline.points.Length - 1];
                    Vector3 position = point.Position;
                    coaster.movingObject.position = position;
                    coaster.movingObject.rotation = point.Rotation;
                    coaster.curveIndex = spline.points.Length / 3;
                    coaster.curveDistance = 0.0f;
                    return true;
                }
            }
            else
            {
                coaster.curveDistance -= spline.curveLookUpTables[coaster.curveIndex].CurveLength; // subtract the distance of the curve and go to the next curve
                coaster.curveIndex++;
            }
        }
        return false;
    }

    private void MovePowered(CoasterTrain coaster) // fix powered rails going slow up a slope
    {
        // find the value of t (not the t for position)
        float t = Mathf.Clamp01(coaster.curveDistance / spline.curveLookUpTables[coaster.curveIndex].CurveLength); // get rid of clamp?

        float tDistance = spline.curveLookUpTables[coaster.curveIndex].GetTFromDistance(coaster.curveDistance);

        // find the position
        coaster.movingObject.position = spline.GetPosition(coaster.curveIndex * 3, tDistance); // POSITION CHANGE

        // Get current direction of travel
        Vector3 cDirection = spline.GetDirection(coaster.curveIndex * 3, tDistance);
        coaster.cDirection = cDirection;

        // Set the rotation of the coaster
        Quaternion rotation = spline.GetRotation(coaster.curveIndex * 3, t, cDirection);
        coaster.movingObject.rotation = rotation;

        // Calculate speed interpolated between start speed and final target speed
        coaster.speed = Mathf.Lerp(coaster.ipSpeed, spline.points[(coaster.curveIndex + 1) * 3].Speed, t);

        // Calculate velocity
        coaster.velocity = coaster.speed * cDirection;
    }

    private void MoveGravity(CoasterTrain coaster)
    {
        // find the value of t (not the t for position)
        float t = Mathf.Clamp01(coaster.curveDistance / spline.curveLookUpTables[coaster.curveIndex].CurveLength);

        float tDistance = spline.curveLookUpTables[coaster.curveIndex].GetTFromDistance(coaster.curveDistance);

        // find the position
        coaster.movingObject.position = spline.GetPosition(coaster.curveIndex * 3, tDistance); // POSITION CHANGE

        // Get current direction of travel
        Vector3 cDirection = spline.GetDirection(coaster.curveIndex * 3, tDistance);
        coaster.cDirection = cDirection;

        // Set the rotation of the coaster
        Quaternion rotation = spline.GetRotation(coaster.curveIndex * 3, t, cDirection);
        coaster.movingObject.rotation = rotation;

        // Calculate a normal that has no local x component (upwards facing normal)
        Vector3 normal = (Vector3.up + Vector3.Project(Vector3.down, cDirection)).normalized;
        coaster.normal = normal;

        // Calculate the added speed
        float addedSpeed = (cDirection.y > 0 ? -1 : 1) * spline.gravity * Mathf.Sin(Vector3.Angle(normal, Vector3.up) * Mathf.Deg2Rad) * Time.deltaTime;

        coaster.speed += addedSpeed;

        coaster.velocity = coaster.speed * cDirection;
    }
}
