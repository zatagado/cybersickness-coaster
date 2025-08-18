using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

/// <summary>
/// State machine component for managing the experiment.
/// </summary>
public class DemoController : MonoBehaviour // TODO rename this
{
    /// <summary>
    /// Enum representing the current state of the application.
    /// </summary>
    [SerializeField] private DemoState currentDemoState;

    [SerializeField] private RollerCoasterTest rollerCoasterTest; // TODO probably remove this
    [SerializeField] private Survey survey;
    [SerializeField] private BalanceMeasure balanceTest;

    [SerializeField] private int levels;
    private int currLevel;

    [SerializeField] private ExitScene ui;
    [SerializeField] private GameObject environment;

    [TextArea(1, 20)]
    [SerializeField] private string surveyURL;

    [SerializeField] private string startSceneName;
    private bool loadingScene;

    /// <summary>
    /// Getter/setter property for the current state of the application.
    /// </summary>
    public DemoState CurrentDemoState { get => currentDemoState; set => currentDemoState = value; }

    private async void Start()
    {
        currentDemoState = DemoState.start;

        Debug.Log("Loading demo.");
        CybersickData.LoadData();

        currLevel = 0;

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
        loadingScene = false;
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            currentDemoState = DemoState.survey;
            currLevel = levels; //ensure it will quit after sending the data
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
                SendData.SendLevel(this, surveyURL, currLevel, rollerCoasterTest.GetTargetsDestroyedRatio(), rollerCoasterTest.GetRunTime(),
                balanceTest.GetMomentOfInertiaResults(), survey.GetSurveyResults());
                if (currLevel < levels)
                {
                    currLevel++;
                    currentDemoState = DemoState.rollerCoasterTest;
                }
                else
                {
                    currentDemoState = DemoState.end;
                }
                break;
            case DemoState.end:
                if (!loadingScene)
                {
                    loadingScene = true;
                    LoadScene("Exit_Scene");
                }
                break;
        }
    }

    public void SetEnvironmentActive(bool value)
    {
        environment.SetActive(value);
    }

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