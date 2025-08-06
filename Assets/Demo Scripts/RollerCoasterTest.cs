using System;
using UnityEngine;

public class RollerCoasterTest : MonoBehaviour
{
    private enum RollerCoasterTestState
    {
        setup,
        waiting,
        startMoving,
        moving,
        end
    }
    [SerializeField] private RollerCoasterTestState currRollerCoasterTestState;
    
    [SerializeField] private float time;
    [SerializeField] private float waitPeriod;

    private int currLevel;
    [SerializeField] private RollerCoaster rollerCoaster;
    [SerializeField] private LookAtTarget lookAtTarget;

    [SerializeField] private CoasterTrain playerCoaster;

    public Action<bool> SetUpAction;

    private float targetsDestroyed;
    private float totalTargets;

    [SerializeField] private bool isCurrentlyRunning;

    private void Start()
    {
        currLevel = 0;
        isCurrentlyRunning = false;
        currRollerCoasterTestState = RollerCoasterTestState.setup;
        lookAtTarget.OnDestroyTarget += DestroyedTarget;
        lookAtTarget.OnCreateTarget += CreateTarget;
    }

    public void SetPlayerObjects(Transform centeredPlayer, Transform vrCamera)
    {
        Debug.Log($"center coaster named {centeredPlayer}");
        playerCoaster = new CoasterTrain(centeredPlayer);
        lookAtTarget.VRCamera = vrCamera;
    }

    public DemoState Tick()
    {
        switch (currRollerCoasterTestState)
        {
            case RollerCoasterTestState.setup:
                currRollerCoasterTestState = SetUp();
                break;
            case RollerCoasterTestState.waiting:
                currRollerCoasterTestState = Wait();
                break;
            case RollerCoasterTestState.startMoving:
                currRollerCoasterTestState = StartMoving();
                break;
            case RollerCoasterTestState.moving:
                currRollerCoasterTestState = Move();
                // if no movement for a certain period of time then send error and end session
                break;
            case RollerCoasterTestState.end:
                End();
                return DemoState.balanceTest;
        }
        time += Time.deltaTime;
        return DemoState.rollerCoasterTest;
    }

    private RollerCoasterTestState SetUp()
    {
        isCurrentlyRunning = true;
        SetUpAction?.Invoke(true);

        time = 0.0f;
        targetsDestroyed = 0;
        totalTargets = 0;

        rollerCoaster.SetUp(currLevel);
        rollerCoaster.StartCoaster(playerCoaster);
        return RollerCoasterTestState.waiting;
    }

    private RollerCoasterTestState Wait()
    {
        if (time > waitPeriod)
        {
            time = 0.0f;
            return RollerCoasterTestState.startMoving;
        }
        else
        {
            return RollerCoasterTestState.waiting;
        }
    }

    private RollerCoasterTestState StartMoving()
    {
        lookAtTarget.SetUp();
        return RollerCoasterTestState.moving;
    }

    private RollerCoasterTestState Move()
    {
        if (rollerCoaster.Move(playerCoaster))
        {
            return RollerCoasterTestState.end;
        }
        else
        {
            lookAtTarget.Tick(playerCoaster); // run the look at object program
            return RollerCoasterTestState.moving;
        }
    }

    public void CreateTarget()
    {
        totalTargets++;
    }

    public void DestroyedTarget()
    {
        targetsDestroyed++;
    }

    public float GetTargetsDestroyedRatio()
    {
        return targetsDestroyed / totalTargets;
    }

    public float GetRunTime()
    {
        Debug.Log("Time "+ time);
        return time;
    }

    private void End()
    {
        isCurrentlyRunning = false;
        currLevel++;
        playerCoaster.Reset();
        currRollerCoasterTestState = RollerCoasterTestState.setup;
    }
}
