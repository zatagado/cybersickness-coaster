using System;
using UnityEngine;
using UnityEngine.UI;

public class Survey : MonoBehaviour
{
    public enum Scale
    {
        notAtAll,
        slightly,
        moderately,
        very
    }

    [System.Serializable]
    private class MatrixScale
    {
        [SerializeField] private string name;
        [SerializeField] private bool toggled;
        [SerializeField] private Toggle notAtAll;
        [SerializeField] private Toggle slightly;
        [SerializeField] private Toggle moderately;
        [SerializeField] private Toggle very;

        public string Name => name;
        public bool Toggled { get => toggled; set => toggled = value; }
        public Toggle NotAtAll => notAtAll;
        public Toggle Slightly => slightly;
        public Toggle Moderately => moderately;
        public Toggle Very => very;
    }

    private enum SurveyState
    {
        Setup,
        Waiting,
        End,
        Quit
    }
    [SerializeField] private SurveyState currSurveyState;

    [SerializeField] private GameObject text;
    [SerializeField] private MatrixScale[] scales;
    private bool changedToggleThisTick;

    [SerializeField] private Button finishedSurveyButton;
    private bool finishedSurvey;

    [SerializeField] private float textDistanceFromHead = 2.0f;
    [SerializeField] private float textHeight = 1.5f;

    private Transform centeredPlayer;

    [SerializeField] private Transform surveyPosition;

    [SerializeField] private VRPointers vrPointers;


    public Action<bool> SetUpAction;

    [SerializeField] private bool isCurrentlyRunning;

    private void Start()
    {
        finishedSurvey = false;
        text.SetActive(false);

        currSurveyState = SurveyState.Setup;

        foreach (MatrixScale scale in scales)
        {
            scale.NotAtAll.onValueChanged.AddListener((value) =>
            {
                if (!changedToggleThisTick)
                {
                    changedToggleThisTick = true;
                    scale.Toggled = value;
                    scale.Slightly.isOn = false;
                    scale.Moderately.isOn = false;
                    scale.Very.isOn = false;
                    if (value && CheckSurveyFilled())
                    {
                        finishedSurveyButton.interactable = true;
                    }
                    else
                    {
                        finishedSurveyButton.interactable = false;
                    }
                }
            });
            scale.Slightly.onValueChanged.AddListener((value) =>
            {
                if (!changedToggleThisTick)
                {
                    changedToggleThisTick = true;
                    scale.Toggled = value;
                    scale.NotAtAll.isOn = false;
                    scale.Moderately.isOn = false;
                    scale.Very.isOn = false;
                    if (value && CheckSurveyFilled())
                    {
                        finishedSurveyButton.interactable = true;
                    }
                    else
                    {
                        finishedSurveyButton.interactable = false;
                    }
                }
            });
            scale.Moderately.onValueChanged.AddListener((value) =>
            {
                if (!changedToggleThisTick)
                {
                    changedToggleThisTick = true;
                    scale.Toggled = value;
                    scale.NotAtAll.isOn = false;
                    scale.Slightly.isOn = false;
                    scale.Very.isOn = false;
                    if (value && CheckSurveyFilled())
                    {
                        finishedSurveyButton.interactable = true;
                    }
                    else
                    {
                        finishedSurveyButton.interactable = false;
                    }
                }
            });
            scale.Very.onValueChanged.AddListener((value) =>
            {
                if (!changedToggleThisTick)
                {
                    changedToggleThisTick = true;
                    scale.Toggled = value;
                    scale.NotAtAll.isOn = false;
                    scale.Slightly.isOn = false;
                    scale.Moderately.isOn = false;
                    if (value && CheckSurveyFilled())
                    {
                        finishedSurveyButton.interactable = true;
                    }
                    else
                    {
                        finishedSurveyButton.interactable = false;
                    }
                }
            });
        }
    }

    public void SetPlayerObjects(Transform centeredPlayer)
    {
        this.centeredPlayer = centeredPlayer;
        vrPointers = centeredPlayer.GetComponentInChildren<VRPointers>();
    }

    public DemoState Tick()
    {
        switch (currSurveyState)
        {
            case SurveyState.Setup:
                currSurveyState = SetUp();
                break;
            case SurveyState.Waiting:
                currSurveyState = Wait();
                changedToggleThisTick = false;
                break;
            case SurveyState.End:
                End();
                return DemoState.send; // order of events?
            case SurveyState.Quit:
                return DemoState.end;
        }
        return DemoState.survey; // CHECK THIS STATEMENT IF ISSUES WITH SURVEY NOT EXECUTING
    }

    public void Teleport()
    {
        centeredPlayer.position = surveyPosition.position;
        centeredPlayer.rotation = surveyPosition.rotation;
    }

    private SurveyState SetUp()
    {
        isCurrentlyRunning = true;
        SetUpAction?.Invoke(false);
        text.SetActive(true);
        finishedSurvey = false;

        ClearSurvey();
        vrPointers.enabled = true;

        Teleport();
        Vector3 position = centeredPlayer.position + centeredPlayer.forward * textDistanceFromHead;
        position.y += textHeight;
        text.transform.position = position;
        return SurveyState.Waiting;
    }

    private SurveyState Wait()
    {
        if (finishedSurvey)
        {
            return SurveyState.End;
        }
        else
        {
            return SurveyState.Waiting;
        }
    }

    private void End()
    {
        isCurrentlyRunning = false;
        text.SetActive(false);

        vrPointers.enabled = false;

        currSurveyState = SurveyState.Setup;
    }

    private void ClearSurvey()
    {
        finishedSurveyButton.interactable = false;
        foreach (MatrixScale scale in scales)
        {
            scale.Toggled = false;
            scale.NotAtAll.isOn = false;
            scale.Slightly.isOn = false;
            scale.Moderately.isOn = false;
            scale.Very.isOn = false;
        }
    }

    private bool CheckSurveyFilled()
    {
        foreach (MatrixScale scale in scales)
        {
            if (!scale.Toggled)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Called by the finish survey button.
    /// </summary>
    public void FinishSurvey()
    {
        finishedSurvey = true;

        int totalScores = 0;
        int[] results = GetSurveyResults();
        foreach (int result in results)
        {
            totalScores += result;
        }

        if (totalScores / scales.Length >= 3.5f) // change threshold as see fit
        {
            currSurveyState = SurveyState.Quit;
        }
    }

    /// <summary>
    /// Reports results for the survey.
    /// </summary>
    /// <returns>Results int array.</returns>
    public int[] GetSurveyResults()
    {
        int[] results = new int[scales.Length];
        for (int i = 0; i < scales.Length; i++)
        {
            if (scales[i].NotAtAll.isOn)
            {
                results[i] = 1;
            }
            else if (scales[i].Slightly.isOn)
            {
                results[i] = 2;
            }
            else if (scales[i].Moderately.isOn)
            {
                results[i] = 3;
            }
            else if (scales[i].Very.isOn)
            {
                results[i] = 4;
            }
        }
        return results;
    }
}
