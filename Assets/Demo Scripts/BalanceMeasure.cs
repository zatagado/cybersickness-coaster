using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// State machine component for measuring user balance after a roller coaster test. 
/// </summary>
public class BalanceMeasure : MonoBehaviour // TODO rename to MeasureBalance
{
    /// <summary>
    /// Enum representing internal state.
    /// </summary>
    private enum BalanceMeasureState
    {
        setup,
        waiting,
        initialIgnoring,
        testing,
        finalIgnoring,
        end
    }
    
    /// <summary>
    /// The current internal state.
    /// </summary>
    [SerializeField] private BalanceMeasureState currentState;

    private float waitingTime;
    /// <summary>
    /// How long to inform the user that their balance is going to be recorded.
    /// </summary>
    [SerializeField] private float waitPeriod;
    
    private float testingTime;
    /// <summary>
    /// How long to ignore balance data before starting to record.
    /// </summary>
    [SerializeField] private float initialIgnoreDataPeriod;
    
    /// <summary>
    /// How long to record balance data.
    /// </summary>
    [SerializeField] private float testPeriod;
    
    /// <summary>
    /// How long to ignore balance data after finishing recording.
    /// </summary>
    [SerializeField] private float finalIgnoreDataPeriod;

    /// <summary>
    /// The frequency of which to record data. TODO convert this to fixed interval
    /// </summary>
    [SerializeField] private int captureFrame;
    private int currentFrame;
    
    private List<Vector3> positions;

    private Transform centeredPlayer;
    private Transform vrCamera;
    /// <summary>
    /// Text output gameobject. Requires a
    /// </summary>
    [SerializeField] private GameObject text;
    /// <summary>
    /// The timer 
    /// </summary>
    [SerializeField] private TMPro.TextMeshProUGUI timer;

    /// <summary>
    /// The distance from the user's head you would like the measuring balance text to be displayed.
    /// </summary>
    [SerializeField] private float textDistanceFromHead = 2.0f;
    /// <summary>
    /// The height of the measuring balance text off of the ground.
    /// </summary>
    [SerializeField] private float textHeight = 1.5f;

    /// <summary>
    /// The position you would like to move the user to to begin measuring.
    /// </summary>
    [SerializeField] private Transform measurementPosition;

    public Action<bool> SetUpAction;

    /// <summary>
    /// Initial setup of the component.
    /// </summary>
    private void Start()
    {
        positions = new List<Vector3>();

        currentFrame = 0;

        text.SetActive(false);
        currentState = BalanceMeasureState.setup;
    }

    public void SetPlayerObjects(Transform centeredPlayer, Transform vrCamera)
    {
        this.centeredPlayer = centeredPlayer;
        this.vrCamera = vrCamera;
    }

    public DemoState Tick()
    {
        switch (currentState)
        {
            case BalanceMeasureState.setup:
                currentState = SetUp();
                break;
            case BalanceMeasureState.waiting:
                currentState = Waiting();
                break;
            case BalanceMeasureState.initialIgnoring:
                currentState = InitialIgnore();
                break;
            case BalanceMeasureState.testing:
                currentState = Test();
                break;
            case BalanceMeasureState.finalIgnoring:
                currentState = FinalIgnore();
                break;
            case BalanceMeasureState.end:
                End();
                return DemoState.survey;
        }
        return DemoState.balanceTest;
    }

    public void Teleport()
    {
        centeredPlayer.position = measurementPosition.position;
        centeredPlayer.rotation = measurementPosition.rotation;
    }

    private BalanceMeasureState SetUp()
    {
        SetUpAction?.Invoke(false);
        text.SetActive(true);
        positions.Clear();
        Teleport();
        Vector3 position = centeredPlayer.position + centeredPlayer.forward * textDistanceFromHead;
        position.y += textHeight;
        text.transform.position = position;

        waitingTime = waitPeriod;
        testingTime = initialIgnoreDataPeriod + testPeriod + finalIgnoreDataPeriod;

        timer.text = string.Format("Stand up! Measuring balance in: {0:00}", waitingTime);

        currentFrame = 0;
        return BalanceMeasureState.waiting;
    }

    private BalanceMeasureState Waiting()
    {
        if (waitingTime <= 0.0f)
        {
            Debug.Log("Starting initial ignore of balance data.");
            return BalanceMeasureState.initialIgnoring;
        }
        else
        {
            waitingTime -= Time.deltaTime;
            timer.text = string.Format("Stand up! Measuring balance in: {0:00}", waitingTime);
            return BalanceMeasureState.waiting;
        }
    }

    private BalanceMeasureState InitialIgnore()
    {
        if (testingTime <= testPeriod + finalIgnoreDataPeriod)
        {
            Debug.Log("Measuring balance values.");
            return BalanceMeasureState.testing;
        }
        else
        {
            testingTime -= Time.deltaTime;
            timer.text = string.Format("Stand still! Measuring balance: {0:00}", testingTime);
            return BalanceMeasureState.initialIgnoring;
        }
    }

    private BalanceMeasureState Test()
    {
        if (testingTime <= finalIgnoreDataPeriod)
        {
            currentFrame = 0;
            Debug.Log("Finished measuring balance values.");
            return BalanceMeasureState.finalIgnoring;
        }
        else
        {
            testingTime -= Time.deltaTime;
            timer.text = string.Format("Stand still! Measuring balance: {0:00}", testingTime);

            if (currentFrame % captureFrame == 0)
            {
                positions.Add(vrCamera.position);
            }
            currentFrame++;
            return BalanceMeasureState.testing;
        }
    }

    private BalanceMeasureState FinalIgnore()
    {
        if (testingTime <= 0.0f)
        {
            testingTime = 0.0f;
            timer.text = string.Format("Stand still! Measuring balance: {0:00}", 0);
            return BalanceMeasureState.end;
        }
        else
        {
            testingTime -= Time.deltaTime;
            timer.text = string.Format("Stand still! Measuring balance: {0:00}", testingTime);
            return BalanceMeasureState.finalIgnoring;
        }
    }

    private void End()
    {
        text.SetActive(false);
        currentState = BalanceMeasureState.setup;
    }

    //average distance traveled
    public double GetMomentOfInertiaResults()
    {
        double sum = 0;
        Vector3 averagePos = CalculateAverage();
        foreach (Vector3 point in positions)
        {
            sum += Mathf.Pow(Vector3.Distance(point, averagePos), 2);
        }
        return sum;
    }

    private Vector3 CalculateAverage() //center of mass (kinda)
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 point in positions)
        {
            sum += point;
        }
        return sum / positions.Count;
    }
}
