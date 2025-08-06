using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRControllers : MonoBehaviour
{
    public enum Controller
    {
        left,
        right
    }    

    [SerializeField] private XRBaseController leftHand;
    [SerializeField] private XRBaseController rightHand;
    private XRIDefaultInputActions inputActions;

    public bool InteractPressL => inputActions.XRILeftHandInteraction.UIPress.WasPressedThisFrame();
    public bool InteractPressR => inputActions.XRIRightHandInteraction.UIPress.WasPressedThisFrame();

    private void OnEnable()
    {
        inputActions = new XRIDefaultInputActions(); // maybe in awake
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    /// <summary>
    /// Vibrate a controller.
    /// </summary>
    /// <param name="source">The controller you would like to vibrate</param>
    /// <param name="amplitude">Intensity 0-1</param>
    /// <param name="duration">Time the vibration lasts</param>

    public void HapticImpulse(Controller source, float amplitude, float duration)
    {
        switch (source)
        {
            case Controller.left:
                leftHand.SendHapticImpulse(amplitude, duration);
                break;
            case Controller.right:
                rightHand.SendHapticImpulse(amplitude, duration);
                break;
        }
    }
}
