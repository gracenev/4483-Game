using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the elevator doors — slides them open and closed.
///
/// SETUP:
///   1. Attach this to the ElevatorShaft or any persistent GameObject.
///   2. Assign leftDoor and rightDoor (the two sliding panels) in the Inspector.
///   3. The call button outside and the close button inside should have
///      InteractableButton attached. Assign them below.
///   4. Your FPS raycast interaction script should call:
///        interactableButton.Press()  →  which calls  NotifyPressed(button)
///      via the OnPressed UnityEvent, OR just call OpenDoors() / CloseDoors()
///      directly from your interaction system.
/// </summary>
public class ElevatorController : MonoBehaviour
{
    [Header("Door Panels")]
    [Tooltip("The left sliding door panel Transform")]
    public Transform leftDoor;
    [Tooltip("The right sliding door panel Transform")]
    public Transform rightDoor;

    [Header("Door Movement")]
    [Tooltip("How far each door slides open along the X axis")]
    public float slideDistance = 1.2f;
    [Tooltip("Units per second the door slides")]
    public float slideSpeed    = 2.0f;
    [Tooltip("Seconds the door stays open before auto-closing (0 = never auto-close)")]
    public float autoCloseDelay = 0f;

    [Header("Buttons")]
    [Tooltip("The call button on the wall outside the elevator")]
    public InteractableButton callButton;
    [Tooltip("The close button mounted inside the elevator")]
    public InteractableButton closeButton;

    // ── State ──────────────────────────────────────────────────────────────
    public enum DoorState { Closed, Opening, Open, Closing }
    public DoorState State { get; private set; } = DoorState.Closed;

    private Vector3 _leftClosed, _leftOpen;
    private Vector3 _rightClosed, _rightOpen;
    private Coroutine _slideCoroutine;
    private Coroutine _autoCloseCoroutine;

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        if (leftDoor == null || rightDoor == null)
        {
            Debug.LogError("[ElevatorController] Left or Right door not assigned!", this);
            return;
        }

        // Record closed positions
        _leftClosed  = leftDoor.localPosition;
        _rightClosed = rightDoor.localPosition;

        // Open positions: doors slide outward along X
        _leftOpen  = _leftClosed  + Vector3.left  * slideDistance;
        _rightOpen = _rightClosed + Vector3.right * slideDistance;

        // Hook into button press callbacks
        if (callButton  != null) callButton.OnPressedCallback  += OnCallButtonPressed;
        if (closeButton != null) closeButton.OnPressedCallback += OnCloseButtonPressed;
    }

    void OnDestroy()
    {
        if (callButton  != null) callButton.OnPressedCallback  -= OnCallButtonPressed;
        if (closeButton != null) closeButton.OnPressedCallback -= OnCloseButtonPressed;
    }

    // ── Button callbacks ───────────────────────────────────────────────────
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

    // ── Public API (call from your FPS raycast system if needed) ───────────
    public void OpenDoors()
    {
        if (_slideCoroutine != null) StopCoroutine(_slideCoroutine);
        if (_autoCloseCoroutine != null) StopCoroutine(_autoCloseCoroutine);
        State = DoorState.Opening;
        _slideCoroutine = StartCoroutine(SlideDoors(_leftOpen, _rightOpen, DoorState.Open));
    }

    public void CloseDoors()
    {
        if (_slideCoroutine != null) StopCoroutine(_slideCoroutine);
        if (_autoCloseCoroutine != null) StopCoroutine(_autoCloseCoroutine);
        State = DoorState.Closing;
        _slideCoroutine = StartCoroutine(SlideDoors(_leftClosed, _rightClosed, DoorState.Closed));
    }

    // ── Sliding coroutine ──────────────────────────────────────────────────
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

    IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        if (State == DoorState.Open)
            CloseDoors();
    }
}