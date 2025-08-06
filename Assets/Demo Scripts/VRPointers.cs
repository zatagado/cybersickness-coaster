using UnityEngine;

public class VRPointers : MonoBehaviour
{
    /*
    [SerializeField] private float defaultLength = 3f;
    private GameObject dot;

    [SerializeField] private LayerMask uiLayer;

    [SerializeField] private LineRenderer lLineRenderer;
    [SerializeField] private LineRenderer rLineRenderer;

    [SerializeField] private VRControllers controllers;
    private VRMouseInputModule inputModule;

    private SteamVR_Input_Sources activeHand;
    private SteamVR_Input_Sources inactiveHand;

    private void OnEnable()
    {
        activeHand = SteamVR_Input_Sources.RightHand;
        inactiveHand = SteamVR_Input_Sources.LeftHand;

        inputModule = UnityEngine.EventSystems.EventSystem.current.gameObject.GetComponent<VRMouseInputModule>();

        dot = GameObject.CreatePrimitive(PrimitiveType.Sphere); // replace with prefab
        dot.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
    }

    private void Update()
    {
        inputModule.InteractDown = controllers.InteractPress.GetStateDown(activeHand);
        if (inputModule.InteractDown)
        {
            controllers.HapticPulse(0.05f, 200f, 0.1f, activeHand);
        }
        inputModule.InteractUp = controllers.InteractPress.GetStateUp(activeHand);

        switch (activeHand)
        {
            case SteamVR_Input_Sources.LeftHand:
                UpdateLine(lLineRenderer);
                lLineRenderer.gameObject.SetActive(true);
                rLineRenderer.gameObject.SetActive(false);
                break;
            case SteamVR_Input_Sources.RightHand:
                UpdateLine(rLineRenderer);
                lLineRenderer.gameObject.SetActive(false);
                rLineRenderer.gameObject.SetActive(true);
                break;
            default:
                break;
        }

        if (controllers.InteractPress.GetStateUp(inactiveHand))
        {
            SteamVR_Input_Sources temp = activeHand;
            activeHand = inactiveHand;
            inactiveHand = temp;
        }
    }

    private void UpdateLine(LineRenderer lineRenderer)
    {
        float targetLength = defaultLength;
        Vector3 endPosition = lineRenderer.transform.position + (lineRenderer.transform.forward * targetLength);

        if (Physics.Raycast(lineRenderer.transform.position, lineRenderer.transform.forward, out RaycastHit hit, defaultLength, uiLayer)) // check if object is interactable
        {
            if (inputModule.CurrentObject != hit.collider.gameObject)
            {
                inputModule.CurrentObject = hit.collider.gameObject;
                // controllers.HapticPulse(0.05f, 200f, 0.05f, activeHand); // If you want haptics when hovering over a new button
            }
            endPosition = hit.point;
        }
        else
        {
            inputModule.CurrentObject = null;
        }

        dot.transform.position = endPosition;

        lineRenderer.SetPosition(0, lineRenderer.transform.position);
        lineRenderer.SetPosition(1, endPosition);
    }

    private void OnDisable()
    {
        Destroy(dot);
        lLineRenderer.gameObject.SetActive(false);
        rLineRenderer.gameObject.SetActive(false);
    }
    */

    private VRControllers vrControllers;
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;

    private void Start()
    {
        vrControllers = GetComponent<VRControllers>();
    }

    private void OnEnable()
    {
        // Left hand is already inactive
        rightHand.SetActive(true);
    }

    private void OnDisable()
    {
        leftHand.SetActive(false);
        rightHand.SetActive(false);
    }

    private void Update()
    {
        if (leftHand.activeSelf && vrControllers.InteractPressR) // switch to right hand
        {
            leftHand.gameObject.SetActive(false);
            rightHand.gameObject.SetActive(true);
        }
        else if (rightHand.activeSelf && vrControllers.InteractPressL) // switch to left hand
        {
            leftHand.gameObject.SetActive(true);
            rightHand.gameObject.SetActive(false);
        }
    }
}
