using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    public enum StartState
    {
        start,
        demoInstructions,
        playspaceCalibration,
        loadInstructions,
        loading
    }
    private StartState startState;

    [SerializeField] private float textDistanceFromHead = 2.0f;
    [SerializeField] private float textDistanceDownward = 0.2f;

    [SerializeField] private Transform demoInstructionsText;
    [SerializeField] private Transform playspaceCalibrationText;
    [SerializeField] private Transform loadingInstructionsText;
    [SerializeField] private Transform loadingText;

    [SerializeField] private AudioAndSubtitles demoInstructions;
    [SerializeField] private AudioAndSubtitles playspaceCalibration;
    [SerializeField] private AudioAndSubtitles loadingInstructions;

    private Transform currentText;

    [SerializeField] private Transform player;
    [SerializeField] private Transform playerHead;
    [SerializeField] private VRControllers controllers;

    private bool loadingScene;

    private void Start()
    {
        startState = StartState.start;

        currentText = demoInstructionsText;
        currentText.rotation = Quaternion.LookRotation(playerHead.forward);
        currentText.position = playerHead.position + (currentText.forward * textDistanceFromHead) + new Vector3(0.0f, -textDistanceDownward, 0.0f);

        demoInstructions.OnContinue += () =>
        {
            startState = StartState.playspaceCalibration;
            demoInstructionsText.gameObject.SetActive(false);
            playspaceCalibrationText.gameObject.SetActive(true);
            playspaceCalibration.Play();
            currentText = playspaceCalibrationText;
            currentText.rotation = Quaternion.LookRotation(playerHead.forward);
            currentText.position = playerHead.position + (currentText.forward * textDistanceFromHead) + new Vector3(0.0f, -textDistanceDownward, 0.0f);
        };

        playspaceCalibration.OnContinue += () =>
        {
            startState = StartState.loadInstructions;
            CalibratePlayspace();
            playspaceCalibrationText.gameObject.SetActive(false);
            loadingInstructionsText.gameObject.SetActive(true);
            loadingInstructions.Play();
            currentText = loadingInstructionsText;
            currentText.rotation = Quaternion.LookRotation(playerHead.forward);
            currentText.position = playerHead.position + (currentText.forward * textDistanceFromHead) + new Vector3(0.0f, -textDistanceDownward, 0.0f);
        };

        loadingInstructions.OnContinue += () =>
        {
            startState = StartState.loading;
            loadingInstructionsText.gameObject.SetActive(false);
            loadingText.gameObject.SetActive(true);
            currentText = loadingText;
            currentText.rotation = Quaternion.LookRotation(playerHead.forward);
            currentText.position = playerHead.position + (currentText.forward * textDistanceFromHead) + new Vector3(0.0f, -textDistanceDownward, 0.0f);
        };

        loadingScene = false;
    }

    private void Update()
    {
        InstructionsFollowHead();
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            LoadScene("Exit_Scene");
        }
        switch (startState)
        {
            case StartState.start:
                if (controllers.InteractPressR || Keyboard.current.eKey.wasPressedThisFrame)
                {
                    demoInstructions.Play();
                    startState = StartState.demoInstructions;
                }
                break;
            case StartState.demoInstructions:
                if (controllers.InteractPressL)
                {
                    demoInstructions.PromptReplay();
                }
                else if (controllers.InteractPressR || Keyboard.current.eKey.wasPressedThisFrame)
                {
                    demoInstructions.PromptContinue();
                }
                break;
            case StartState.playspaceCalibration:
                if (controllers.InteractPressR || Keyboard.current.eKey.wasPressedThisFrame)
                {
                    playspaceCalibration.PromptContinue();
                }
                break;
            case StartState.loadInstructions:
                if (controllers.InteractPressR || Keyboard.current.eKey.wasPressedThisFrame)
                {
                    loadingInstructions.PromptContinue();
                }
                break;
            case StartState.loading:
                if (!loadingScene)
                {
                    loadingScene = true;
                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        Debug.LogError("Could not establish a connection to the internet.");
                        LoadScene("No_Internet_Scene");
                    }
                    else
                    {
                        LoadScene("Demo_Scene");
                    }
                }
                break;
        }
    }

    private void CalibratePlayspace()
    {
        Transform centeredPlayer = new GameObject("Player (Centered)").transform;
        centeredPlayer.position = new Vector3(playerHead.transform.position.x, player.transform.position.y, playerHead.transform.position.z);
        centeredPlayer.rotation = Quaternion.Euler(0, playerHead.transform.rotation.eulerAngles.y, 0);
        player.parent = centeredPlayer;
        DontDestroyOnLoad(centeredPlayer);

        Debug.Log("Finished calibrating playspace.");
    }

    private void InstructionsFollowHead()
    {
        currentText.rotation = Quaternion.LookRotation(Vector3.Lerp(currentText.forward, playerHead.forward, 1f * Time.deltaTime));
        currentText.position = playerHead.position + (currentText.forward * textDistanceFromHead) + new Vector3(0.0f, -textDistanceDownward, 0.0f);
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
