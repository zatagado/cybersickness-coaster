using UnityEngine;
using UnityEngine.InputSystem;

public class ExitScene : MonoBehaviour
{
    [SerializeField] private float textDistanceFromHead = 2.0f;
    [SerializeField] private float textHeight = 1.5f;

    [SerializeField] private Transform exitText;

    private void Start()
    {
        Transform centeredPlayer = GameObject.Find("Player (Centered)")?.transform;
        centeredPlayer.position = Vector3.zero;
        centeredPlayer.rotation = Quaternion.identity;

        Vector3 position = centeredPlayer.position + centeredPlayer.forward * textDistanceFromHead;
        position.y += textHeight;
        exitText.transform.position = position;
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Application.Quit();
        }
    }
}
