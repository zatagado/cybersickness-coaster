using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// State machine component for managing the experiment.
/// </summary>
public class DemoController : MonoBehaviour // TODO rename this
{
    /// <summary>
    /// Enum representing the current state of the application.
    /// </summary>
    private DemoState currentDemoState = DemoState.start;

    /// <summary>
    /// Controller object for running the roller coaster test.
    /// </summary>
    [SerializeField] private RollerCoasterTest rollerCoasterTest;

    /// <summary>
    /// Controller object for gathering survey data.
    /// </summary>
    [SerializeField] private Survey survey;

    /// <summary>
    /// Controller object for measuring balance data.
    /// </summary>
    [SerializeField] private MeasureBalance balanceTest;

    /// <summary>
    /// The number of times to repeat the experiment, per session.
    /// </summary>
    [SerializeField] private int levels;
    private int currentLevel;

    [SerializeField] private GameObject environment;

    [SerializeField] private string startSceneName;
    private bool isLoadingScene;

    /// <summary>
    /// Qualtrics survey URL to send data to.
    /// </summary>
    [TextArea(1, 20)][SerializeField] private string surveyURL;

    /// <summary>
    /// Getter/setter property for the current state of the application.
    /// </summary>
    public DemoState CurrentDemoState { get => currentDemoState; set => currentDemoState = value; }

    /// <summary>
    /// Set the environment active or inactive.
    /// </summary>
    /// <param name="value">True to set the environment active, false to set it inactive.</param>
    public void SetEnvironmentActive(bool value)
    {
        environment.SetActive(value);
    }

    /// <summary>
    /// Experiment initialization.
    /// </summary>
    private async void Start()
    {
        currentDemoState = DemoState.start;

        Debug.Log("Loading demo.");
        CybersickData.LoadData();

        currentLevel = 0;

        Transform centeredPlayer = GameObject.Find("Player (Centered)")?.transform;

        if (!centeredPlayer)
        {
            Debug.LogError("VR is not active!");
            LoadScene(startSceneName);
            return;
        }

        Transform vrCamera = GameObject.Find("Camera").transform;
        rollerCoasterTest.SetPlayerObjects(centeredPlayer, vrCamera);
        balanceTest.SetPlayerObjects(centeredPlayer, vrCamera);
        survey.SetPlayerObjects(centeredPlayer);

        survey.Teleport();
        SetEnvironmentActive(false);

        rollerCoasterTest.SetUpAction += SetEnvironmentActive;
        balanceTest.SetUpAction += SetEnvironmentActive;
        survey.SetUpAction += SetEnvironmentActive;

        await Task.Delay(1);

        currentDemoState = DemoState.survey;
        Debug.Log("Finished loading.");
        isLoadingScene = false;
    }

    /// <summary>
    /// Main update loop with state machine.
    /// </summary>
    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            currentDemoState = DemoState.survey;
            currentLevel = levels; //ensure it will quit after sending the data
            Debug.Log("Running time was " + rollerCoasterTest.GetRunTime());
        }

        // Quit the application when the escape key is pressed.
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Application.Quit();
        }
        switch (currentDemoState)
        {
            case DemoState.start:
                break;
            case DemoState.rollerCoasterTest:
                currentDemoState = rollerCoasterTest.Tick();         
                break;
            case DemoState.balanceTest:
                currentDemoState = balanceTest.Tick();
                break;
            case DemoState.survey:
                currentDemoState = survey.Tick();
                break;
            case DemoState.send:
                StartCoroutine(SendLevelToQualtrics(currentLevel, rollerCoasterTest.GetTargetsDestroyedRatio(), rollerCoasterTest.GetRunTime(),
                    balanceTest.GetMomentOfInertiaResults(), survey.Scales));

                if (currentLevel < levels)
                {
                    currentLevel++;
                    currentDemoState = DemoState.rollerCoasterTest;
                }
                else
                {
                    currentDemoState = DemoState.end;
                }
                break;
            case DemoState.end:
                if (!isLoadingScene)
                {
                    isLoadingScene = true;
                    LoadScene("Exit_Scene");
                }
                break;
        }
    }

    /// <summary>
    /// Send start data to the Qualtrics survey API coroutine.
    /// </summary>
    private IEnumerator SendStartToQualtrics()
    {
        // Form containing data
        WWWForm form = new WWWForm();
        form.AddField("pid", PlayerPrefs.GetString("pid"));
        form.AddField("session", PlayerPrefs.GetInt("session"));

        // Input the data into the site
        using (UnityWebRequest request = UnityWebRequest.Post(surveyURL, form))
        {
            yield return request.SendWebRequest();
        }
        Debug.Log("Sent");
    }

    /// <summary>
    /// Send level data to the Qualtrics survey API coroutine.
    /// </summary>
    /// <param name="level">The level number.</param>
    /// <param name="balloonsPoppedRatio">The ratio of balloons popped to total balloons.</param>
    /// <param name="runTime">The time the user spent on the level.</param>
    /// <param name="moi">The moment of inertia of the user.</param>
    private IEnumerator SendLevelToQualtrics(int level, float balloonsPoppedRatio, float runTime, double moi, Survey.MatrixScale[] scales)
    {
        // Form containing data
        WWWForm form = new WWWForm();

        // form.AddField("balloons popped ratio", balloonsPoppedRatio.ToString());
        // form.AddField("moment of inertia", moi.ToString());
        // form.AddField("general discomfort", surveyData[0]);
        // form.AddField("fatigue", surveyData[1]);
        // form.AddField("eyestrain", surveyData[2]);
        // form.AddField("difficulty focusing", surveyData[3]);
        // form.AddField("headache", surveyData[4]);
        // form.AddField("fullness of head", surveyData[5]);
        // form.AddField("blurred vision", surveyData[6]);
        // form.AddField("dizzy", surveyData[7]);
        // form.AddField("vertigo", surveyData[8]);

        form.AddField("pid", CybersickData.Pid);
        form.AddField("session", CybersickData.Session);
        form.AddField("level", level);

        foreach (Survey.MatrixScale scale in scales)
        {
            form.AddField(scale.Name, Survey.SurveyResultToString(scale));
        }

        form.AddField("running time", runTime.ToString());

        // Input the data into the site
        using (UnityWebRequest request = UnityWebRequest.Post(surveyURL, form))
        {
            yield return request.SendWebRequest();
        }
    }

    /// <summary>
    /// Load a scene asynchronously.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    private async void LoadScene(string sceneName)
    {
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        do
        {
            await Task.Delay(100);
        } while (scene.progress < 0.9f);

        await Task.Delay(1000);

        scene.allowSceneActivation = true;
    }
}