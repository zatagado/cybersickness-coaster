using System;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    private class Target
    {
        private Transform transform;
        private int targetCurveIndex;
        private float targetDistance;
        private int targetCurrLoop;
        private float lookTime; // time the target has been looked at

        public Transform Transform => transform;
        public int TargetCurveIndex => targetCurveIndex;
        public float TargetDistance => targetDistance;
        public int TargetCurrLoop => targetCurrLoop;
        public float LookTime => lookTime;

        public Target(Transform transform, int targetCurveIndex, float targetDistance, int targetCurrLoop)
        {
            this.transform = transform;
            this.targetCurveIndex = targetCurveIndex;
            this.targetDistance = targetDistance;
            this.targetCurrLoop = targetCurrLoop;
            lookTime = 0.0f;
        }

        public void IncreaseLookTime()
        {
            lookTime += Time.deltaTime;
        }
    }

    private TrackSpline spline;
    private AudioSource audioSource;
    [SerializeField] private GameObject targetPrefab;
    private float timeSincePreviousTarget; // target becomes more likely to spawn as time goes on
    [SerializeField] private float minTimeTargetSpawn;
    [SerializeField] private float maxTimeTargetSpawn;

    [SerializeField] private float minTargetDistance; // how far the target can spawn from the player

    [SerializeField] private float targetDistanceMultiplier;

    [SerializeField] private float spawnCircleRadius; // max distance from track that the target can spawn

    [SerializeField] private float lookPeriod;
    [SerializeField] private float maxLookAngle;

    private Target target;

    public Action OnDestroyTarget;
    public Action OnCreateTarget;

    public Transform VRCamera { private get; set; }

    private void Start()
    {
        spline = GetComponent<TrackSpline>();
        audioSource = GetComponent<AudioSource>();
    }

    public void SetUp()
    {
        if (target != null)
        {
            Destroy(target.Transform.gameObject);
        }
        target = null;
        timeSincePreviousTarget = 0.0f;
    }

    public void Tick(CoasterTrain player)
    {
        if (target != null)
        {
            float angle = Vector3.Angle(VRCamera.forward, target.Transform.position - VRCamera.position);
            if (angle <= maxLookAngle) // player looking at target
            {
                target.IncreaseLookTime();
                if (target.LookTime >= lookPeriod) // player looked at target long enough to destroy it
                {
                    OnDestroyTarget?.Invoke();
                    Destroy(target.Transform.gameObject);
                    audioSource.Play();
                    target = null;
                    timeSincePreviousTarget = 0.0f;
                    return;
                }
            }

            if (target.TargetCurrLoop <= player.currLoop && target.TargetCurveIndex <= player.curveIndex && 
                target.TargetDistance <= player.curveDistance) // passed the target
            {
                Destroy(target.Transform.gameObject);
                target = null;
                timeSincePreviousTarget = 0.0f;
            }
        }
        else
        {
            timeSincePreviousTarget += Time.deltaTime;
            if (timeSincePreviousTarget > minTimeTargetSpawn)
            {
                float random = UnityEngine.Random.Range(Mathf.Clamp(timeSincePreviousTarget, 0, maxTimeTargetSpawn), maxTimeTargetSpawn); // becomes more likely to spawn
                if (random > maxTimeTargetSpawn - 0.01f)
                {
                    CreateTargetOnRails(player);
                    OnCreateTarget?.Invoke();
                }
            }
        }
    }

    /// <summary>
    /// Creates a target near the rails in front of the player.
    /// </summary>
    /// <param name="player">The player on the rails.</param>
    private void CreateTargetOnRails(CoasterTrain player)
    {
        // choose curve in front of the player
        int targetCurveIndex = player.curveIndex;
        // float targetDistance = player.curveDistance + minTargetDistance;
        float targetDistance = Mathf.Max(player.curveDistance + minTargetDistance, player.curveDistance + (player.speed * targetDistanceMultiplier));
        if (targetDistance == player.curveDistance + (player.speed * targetDistanceMultiplier));
        int targetCurrLoop = player.currLoop;

        while (targetDistance > spline.curveLookUpTables[targetCurveIndex].CurveLength)
        {
            if (targetCurveIndex + 1 >= spline.points.Length / 3)
            {
                if (player.currLoop + 1 < spline.loops && spline.continuousLoop)
                {
                    targetDistance -= spline.curveLookUpTables[targetCurveIndex].CurveLength; // subtract the distance of the curve and go to the 0th curve
                    targetCurveIndex = 0;
                    targetCurrLoop++;
                }
                else // Too close to the end of the track to make a new balloon.
                {
                    return;
                }
            }
            else
            {
                targetDistance -= spline.curveLookUpTables[targetCurveIndex++].CurveLength; // subtract the distance of the curve and go to the next curve
            }
        }

        float tDistance = spline.curveLookUpTables[targetCurveIndex].GetTFromDistance(targetDistance);
        Vector3 center = spline.GetPosition(targetCurveIndex * 3, tDistance);
        Vector3 direction = spline.GetDirection(targetCurveIndex * 3, tDistance);
        Quaternion rotation = spline.GetRotation(targetCurveIndex * 3, targetDistance / 
            spline.curveLookUpTables[targetCurveIndex].CurveLength, direction);

        Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * spawnCircleRadius; // raycast in direction to not make go through objects.
        if (Physics.SphereCast(center, 0.35f, rotation * randomPoint, out RaycastHit hit, randomPoint.magnitude, 1 << 9))
        {
            target = new Target(Instantiate(targetPrefab, center + rotation * (randomPoint.normalized * 
                Mathf.Clamp(hit.distance - 1, 0, Mathf.Infinity)), Quaternion.identity, spline.transform).transform,
                targetCurveIndex, targetDistance, targetCurrLoop);
        }
        else
        {
            target = new Target(Instantiate(targetPrefab, center + rotation * randomPoint, Quaternion.identity, 
                spline.transform).transform, targetCurveIndex, targetDistance, targetCurrLoop);
        }

        target.Transform.name = "Target Balloon";
    }
}
