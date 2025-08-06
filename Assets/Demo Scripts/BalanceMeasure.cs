using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BalanceMeasure : MonoBehaviour
{
    private enum BalanceMeasureState
    {
        setup,
        waiting,
        initialIgnoring,
        testing,
        finalIgnoring,
        end
    }
    [SerializeField] private BalanceMeasureState currState;

    private float waitingTime;
    [SerializeField] private float waitPeriod;
    private float testingTime;
    [SerializeField] private float iIgnoreDataPeriod;
    [SerializeField] private float testPeriod;
    [SerializeField] private float fIgnoreDataPeriod;

    [SerializeField] private int captureFrame;
    private int currFrame;

    private List<Vector3> positions;

    private Transform centeredPlayer;
    private Transform vrCamera;
    [SerializeField] private GameObject text;
    [SerializeField] private TMPro.TextMeshProUGUI timer;

    [SerializeField] private float textDistanceFromHead = 2.0f;
    [SerializeField] private float textHeight = 1.5f;

    [SerializeField] private Transform measurementPosition;

    public Action<bool> SetUpAction;

    [SerializeField] private bool isCurrentlyRunning;

    private void Start()
    {
        positions = new List<Vector3>();

        currFrame = 0;

        text.SetActive(false);
        isCurrentlyRunning = false;
        currState = BalanceMeasureState.setup;
    }

    public void SetPlayerObjects(Transform centeredPlayer, Transform vrCamera)
    {
        this.centeredPlayer = centeredPlayer;
        this.vrCamera = vrCamera;
    }

    public DemoState Tick()
    {
        switch (currState)
        {
            case BalanceMeasureState.setup:
                currState = SetUp();
                break;
            case BalanceMeasureState.waiting:
                currState = Waiting();
                break;
            case BalanceMeasureState.initialIgnoring:
                currState = InitialIgnore();
                break;
            case BalanceMeasureState.testing:
                currState = Test();
                break;
            case BalanceMeasureState.finalIgnoring:
                currState = FinalIgnore();
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
        isCurrentlyRunning = true;
        SetUpAction?.Invoke(false);
        text.SetActive(true);
        positions.Clear();
        Teleport();
        Vector3 position = centeredPlayer.position + centeredPlayer.forward * textDistanceFromHead;
        position.y += textHeight;
        text.transform.position = position;

        waitingTime = waitPeriod;
        testingTime = iIgnoreDataPeriod + testPeriod + fIgnoreDataPeriod;

        timer.text = string.Format("Stand up! Measuring balance in: {0:00}", waitingTime);

        currFrame = 0;
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
        if (testingTime <= testPeriod + fIgnoreDataPeriod)
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
        if (testingTime <= fIgnoreDataPeriod)
        {
            currFrame = 0;
            Debug.Log("Finished measuring balance values.");
            return BalanceMeasureState.finalIgnoring;
        }
        else
        {
            testingTime -= Time.deltaTime;
            timer.text = string.Format("Stand still! Measuring balance: {0:00}", testingTime);

            if (currFrame % captureFrame == 0)
            {
                positions.Add(vrCamera.position);
            }
            currFrame++;
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
        isCurrentlyRunning = false;
        text.SetActive(false);
        currState = BalanceMeasureState.setup;
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
