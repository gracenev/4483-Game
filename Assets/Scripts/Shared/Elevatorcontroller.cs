using System.Collections;
using UnityEngine;

// Controls the elevator sliding doors.
// Listens to the call and close buttons and smoothly slides the doors open or shut.
public class ElevatorController : MonoBehaviour
{
    [Header("Door Panels")]
    [Tooltip("The left sliding door panel")]
    public Transform leftDoor;
    [Tooltip("The right sliding door panel")]
    public Transform rightDoor;

    [Header("Door Movement")]
    [Tooltip("How far each panel slides when opening")]
    public float slideDistance  = 1.2f;
    [Tooltip("How fast the doors slide in units per second")]
    public float slideSpeed     = 2.0f;
    [Tooltip("How long the doors stay open before closing on their own (0 = stay open)")]
    public float autoCloseDelay = 0f;

    [Header("Buttons")]
    [Tooltip("The call button outside the elevator")]
    public InteractableButton callButton;
    [Tooltip("The close button inside the elevator")]
    public InteractableButton closeButton;

    public enum DoorState { Closed, Opening, Open, Closing }
    public DoorState State { get; private set; } = DoorState.Closed;

    private Vector3   _leftClosed,  _leftOpen;
    private Vector3   _rightClosed, _rightOpen;
    private Coroutine _slideCoroutine;
    private Coroutine _autoCloseCoroutine;

    void Start()
    {
        if (leftDoor == null || rightDoor == null)
        {
            Debug.LogError("ElevatorController is missing a door reference - check the Inspector.", this);
            return;
        }

        // Store the starting positions so we know where closed is
        _leftClosed  = leftDoor.localPosition;
        _rightClosed = rightDoor.localPosition;

        // Calculate where each door needs to slide to when open
        _leftOpen  = _leftClosed  + Vector3.left  * slideDistance;
        _rightOpen = _rightClosed + Vector3.right * slideDistance;

        if (callButton  != null) callButton.OnPressedCallback  += OnCallButtonPressed;
        if (closeButton != null) closeButton.OnPressedCallback += OnCloseButtonPressed;
    }

    void OnDestroy()
    {
        if (callButton  != null) callButton.OnPressedCallback  -= OnCallButtonPressed;
        if (closeButton != null) closeButton.OnPressedCallback -= OnCloseButtonPressed;
    }

    void OnCallButtonPressed()
    {
        if (State == DoorState.Closed || State == DoorState.Closing)
            OpenDoors();
    }

    void OnCloseButtonPressed()
    {
        if (State == DoorState.Open || State == DoorState.Opening)
            CloseDoors();
    }

    public void OpenDoors()
    {
        if (_slideCoroutine      != null) StopCoroutine(_slideCoroutine);
        if (_autoCloseCoroutine  != null) StopCoroutine(_autoCloseCoroutine);
        State           = DoorState.Opening;
        _slideCoroutine = StartCoroutine(SlideDoors(_leftOpen, _rightOpen, DoorState.Open));
    }

    public void CloseDoors()
    {
        if (_slideCoroutine      != null) StopCoroutine(_slideCoroutine);
        if (_autoCloseCoroutine  != null) StopCoroutine(_autoCloseCoroutine);
        State           = DoorState.Closing;
        _slideCoroutine = StartCoroutine(SlideDoors(_leftClosed, _rightClosed, DoorState.Closed));
    }

    // Moves both doors toward their target positions each frame until they arrive
    IEnumerator SlideDoors(Vector3 leftTarget, Vector3 rightTarget, DoorState endState)
    {
        while (true)
        {
            leftDoor.localPosition  = Vector3.MoveTowards(leftDoor.localPosition,  leftTarget,  slideSpeed * Time.deltaTime);
            rightDoor.localPosition = Vector3.MoveTowards(rightDoor.localPosition, rightTarget, slideSpeed * Time.deltaTime);

            bool leftDone  = Vector3.Distance(leftDoor.localPosition,  leftTarget)  < 0.002f;
            bool rightDone = Vector3.Distance(rightDoor.localPosition, rightTarget) < 0.002f;

            if (leftDone && rightDone)
            {
                // Snap to final position and update the state
                leftDoor.localPosition  = leftTarget;
                rightDoor.localPosition = rightTarget;
                State = endState;

                if (endState == DoorState.Open && autoCloseDelay > 0f)
                    _autoCloseCoroutine = StartCoroutine(AutoClose());

                yield break;
            }

            yield return null;
        }
    }

    // Waits a bit then closes the doors automatically if they're still open
    IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        if (State == DoorState.Open)
            CloseDoors();
    }
}