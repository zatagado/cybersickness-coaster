using UnityEngine;
using UnityEngine.EventSystems;

public class VRMouseInputModule : StandaloneInputModule
{
    public PointerEventData Data { private set; get; } // could just make a private variable
    public GameObject CurrentObject { get; set; }
    public bool InteractDown { get; set; }
    public bool InteractUp { get; set; }

    protected override void Awake()
    {
        base.Awake();

        Data = new PointerEventData(eventSystem);
        CurrentObject = null;
        InteractUp = false;
        InteractUp = false;
    }

    public override void Process()
    {
        if (Input.GetMouseButton(0))
        {
            base.Process();
        }
        else
        {
            Data.Reset();

            if (CurrentObject)
            {
                eventSystem.RaycastAll(Data, m_RaycastResultCache); // is this needed
                Data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);

                m_RaycastResultCache.Clear();

                HandlePointerExitAndEnter(Data, CurrentObject);

                if (InteractDown)
                {
                    ProcessPress(Data);
                }

                if (InteractUp)
                {
                    ProcessRelease(Data);
                }
            }
        }
    }

    private void ProcessPress(PointerEventData data)
    {
        data.pointerPressRaycast = data.pointerCurrentRaycast;

        GameObject newPointerPress = ExecuteEvents.ExecuteHierarchy(CurrentObject, data, ExecuteEvents.pointerDownHandler);

        if (!newPointerPress)
        {
            newPointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(CurrentObject);
        }

        data.pressPosition = data.position;
        data.pointerPress = newPointerPress;
        data.rawPointerPress = CurrentObject;
    }

    private void ProcessRelease(PointerEventData data)
    {
        ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);

        GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(CurrentObject);

        if (data.pointerPress == pointerUpHandler)
        {
            ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerClickHandler);
        }

        eventSystem.SetSelectedGameObject(null);

        data.pressPosition = Vector2.zero;
        data.pointerPress = null;
        data.rawPointerPress = null;
    }
}
