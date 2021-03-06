﻿using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;

#endif

namespace BulwarkStudios.GameSystems.Ui {

#if UNITY_EDITOR
    public class MyAssetModificationProcessor : UnityEditor.AssetModificationProcessor {

        public static string[] OnWillSaveAssets(string[] paths) {

            // Is there a xcene in the path ?
            bool hasScene = false;

            foreach (string path in paths) {
                if (path.Contains(".unity")) {
                    hasScene = true;
                    break;
                }
            }

            // Save the scene
            if (hasScene) {
                foreach (GameObject rootGameObject in EditorSceneManager.GetActiveScene().GetRootGameObjects()) {
                    foreach (UiButton child in rootGameObject.GetComponentsInChildren<UiButton>(true)) {
                        child.UpdateState(UiButton.STATE.NORMAL);
                    }
                }
            }

            return paths;
        }

        /// <summary>
        /// Reset the initialisation
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnProjectLoadedInEditor() {
            foreach (GameObject rootGameObject in EditorSceneManager.GetActiveScene().GetRootGameObjects()) {
                foreach (UiButton child in rootGameObject.GetComponentsInChildren<UiButton>(true)) {
                    child.UpdateState(UiButton.STATE.NORMAL);
                }
            }
        }

    }
#endif

    [RequireComponent(typeof(Selectable))]
    public class UiButton : MonoBehaviour, IDeselectHandler, IMoveHandler, IPointerClickHandler, IPointerDownHandler,
        IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, ISelectHandler, ISubmitHandler,
        IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
        IScrollHandler {

        /// <summary>
        /// Button state
        /// </summary>
        public enum STATE {

            NORMAL,

            HIGHLIGHTED,

            PRESSED,

            DISABLED,

            CUSTOM1,

            CUSTOM2,

        }

        /// <summary>
        /// Events available
        /// </summary>
        public enum EVENT {

            ON_DESELECT,

            ON_MOVE,

            ON_POINTER_CLICK,

            ON_POINTER_DOWN,

            ON_POINTER_ENTER,

            ON_POINTER_EXIT,

            ON_POINTER_UP,

            ON_SELECT,

            ON_SUBMIT,

            ON_INITIALIZE_POTENTIAL_DRAG,

            ON_BEGIN_DRAG,

            ON_DRAG,

            ON_END_DRAG,

            ON_DROP,

            ON_SCROLL

        }

        /// <summary>
        /// Ref to the selectable
        /// </summary>
        private Selectable refSelectable;

        /// <summary>
        /// Get the selectable
        /// </summary>
        private Selectable Selectable {
            get {
                if (refSelectable == null) {
                    refSelectable = GetComponent<Selectable>();
                }

                return refSelectable;
            }
        }

        /// <summary>
        /// Current state
        /// </summary>
        [ShowInInspector, ReadOnly]
        public STATE state;

        /// <summary>
        /// Overrided state?
        /// </summary>
        [ShowInInspector, ReadOnly]
        private bool overridedState;

        /// <summary>
        /// Overrided state
        /// </summary>
        [ShowInInspector, ReadOnly, ShowIf(nameof(overridedState))]
        private STATE overrideState;

        /// <summary>
        /// Overrided interractable state
        /// </summary>
        [ShowInInspector, ReadOnly]
        private bool overridedInteractable = false;

        /// <summary>
        /// Disable the mouse?
        /// </summary>
        [SerializeField]
        private bool disableMouse = false;

        /// <summary>
        /// Cursor over?
        /// </summary>
        private bool cursorIsOver;

        /// <summary>
        /// Cursor down?
        /// </summary>
        private bool cursorIsDown;

        /// <summary>
        /// Initialized?
        /// </summary>
        [ShowInInspector, ReadOnly]
        private bool isInitialized;

        /// <summary>
        /// On state updated event
        /// </summary>
        public event System.Action<STATE> OnStateUpdated;

        /// <summary>
        /// On trigger event
        /// </summary>
        public event System.Action OnTriggered;

        /// <summary>
        /// An event has been triggered
        /// </summary>
        public event System.Action<UiButton, EVENT, BaseEventData> OnEventTriggered;

        /// <summary>
        /// Current state
        /// </summary>
        [ShowInInspector, System.NonSerialized, HideLabel, OnValueChanged(nameof(PreviewStateChange)),
         EnumToggleButtons, Title("Preview State")]
        public STATE previewState = STATE.NORMAL;

        /// <summary>
        /// Current state
        /// </summary>
        [ShowInInspector, System.NonSerialized, HideLabel, EnumToggleButtons, Title("Preview Event")]
        public EVENT previewEvent = EVENT.ON_DESELECT;

        /// <summary>
        /// Enable
        /// </summary>
        private void OnEnable() {
            RefreshButtonState();
            UpdateState(state);
        }

        /// <summary>
        /// Disable
        /// </summary>
        private void OnDisable() {
            cursorIsOver = false;
            cursorIsDown = false;
            SetState(STATE.NORMAL);
        }

        /// <summary>
        /// Refresh the button state
        /// </summary>
        public void RefreshButtonState() {

            // Valid?
            bool isValid = ConstraintsValid();

            // Override interactable?
            if (overridedInteractable && !Selectable.interactable) {
                isValid = false;
            }

            // No change
            if (Selectable.interactable == isValid) {
                return;
            }

            Selectable.interactable = isValid;

            // Change the state
            if (isValid) {

                // Selected?
                if (state != STATE.HIGHLIGHTED) {
                    SetState(STATE.NORMAL);
                }

            }
            else {
                SetState(STATE.DISABLED);
            }

        }

        /// <summary>
        /// Check the constraints
        /// </summary>
        public bool ConstraintsValid() {

            foreach (UiButtonConstraint constraint in GetComponents<UiButtonConstraint>()) {
                if (!constraint.enabled) {
                    continue;
                }

                if (!constraint.IsValid()) {
                    return false;
                }
            }

            return true;

        }

        /// <summary>
        /// Set overrided state
        /// </summary>
        /// <param name="newState"></param>
        public void SetOverridedState(STATE? newState) {

            // Cancel?
            if (newState == null) {
                overridedState = false;
                RefreshButtonState();
                return;
            }

            // Override
            overridedState = true;
            overrideState = newState.Value;

            // Update the state
            UpdateState(newState.Value);

        }

        /// <summary>
        /// Set a new state
        /// </summary>
        /// <param name="newState"></param>
        private void SetState(STATE newState) {

            // Overrided?
            if (overridedState) {
                return;
            }

            // No change
            if (isInitialized && state == newState) {
                return;
            }

            // No interactable
            if (!IsInteractable()) {
                newState = STATE.DISABLED;
            }

            // Update the state
            UpdateState(newState);

            // Initialize
            isInitialized = true;

        }

        /// <summary>
        /// Update the state
        /// </summary>
        /// <param name="newState"></param>
        public void UpdateState(STATE newState) {

            // Update states
            foreach (IUiButtonEffect buttonState in GetComponents<IUiButtonEffect>()) {
                buttonState.UpdateState(state, newState);
            }

            // Change state
            state = newState;

            // Event
            OnStateUpdated?.Invoke(state);

            //GameLogSystem.Info("Update state " + state);

        }

        /// <summary>
        /// Preview state
        /// </summary>
        [OnInspectorGUI]
        private void PreviewStateChange() {

            if (Application.isPlaying) {
                return;
            }

            // Update states
            foreach (IUiButtonEffect buttonState in GetComponents<IUiButtonEffect>()) {
                buttonState.PreviewState(previewState);
            }

            gameObject.SetActive(!gameObject.activeSelf);
            gameObject.SetActive(!gameObject.activeSelf);
        }

        /// <summary>
        /// Interractable?
        /// </summary>
        /// <returns></returns>
        public bool IsInteractable() {
            return Selectable.interactable;
        }

        /// <summary>
        /// Set Interractable
        /// </summary>
        /// <returns></returns>
        public void SetOverrideInteractable(bool? value) {

            // Reset
            if (value == null) {
                overridedInteractable = false;
                RefreshButtonState();
                return;
            }

            // Override
            overridedInteractable = true;
            Selectable.interactable = value.Value;
            RefreshButtonState();

        }

        /// <summary>
        /// Submit the button
        /// </summary>
        public void Submit() {
            ExecuteEvents.Execute(gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData) {

            OnEventTriggered?.Invoke(this, EVENT.ON_DESELECT, eventData);

            if (!IsInteractable()) {
                return;
            }

            SetState(STATE.NORMAL);

            //GameLogSystem.Info("OnDeselect");
        }

        void IMoveHandler.OnMove(AxisEventData eventData) {

            OnEventTriggered?.Invoke(this, EVENT.ON_MOVE, eventData);

            if (!IsInteractable()) {
                return;
            }

            //GameLogSystem.Info("OnMove");
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {

            if (disableMouse) {
                return;
            }

            OnEventTriggered?.Invoke(this, EVENT.ON_POINTER_CLICK, eventData);

            if (!IsInteractable()) {
                return;
            }

            OnTriggered?.Invoke();

            //GameLogSystem.Info("OnPointerClick");
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {

            if (disableMouse) {
                return;
            }

            OnEventTriggered?.Invoke(this, EVENT.ON_POINTER_DOWN, eventData);

            cursorIsDown = true;

            if (!IsInteractable()) {
                return;
            }

            SetState(STATE.PRESSED);

            //GameLogSystem.Info("OnPointerDown");
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {

            if (disableMouse) {
                return;
            }

            OnEventTriggered?.Invoke(this, EVENT.ON_POINTER_ENTER, eventData);

            cursorIsOver = true;

            if (!IsInteractable()) {
                return;
            }

            if (cursorIsDown) {
                SetState(STATE.PRESSED);
            }
            else {
                SetState(STATE.HIGHLIGHTED);
            }

            //GameLogSystem.Info("OnPointerEnter");
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {

            if (disableMouse) {
                return;
            }

            OnEventTriggered?.Invoke(this, EVENT.ON_POINTER_EXIT, eventData);

            cursorIsOver = false;

            if (!IsInteractable()) {
                return;
            }

            SetState(STATE.NORMAL);

            //GameLogSystem.Info("OnPointerExit");
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {

            if (disableMouse) {
                return;
            }

            OnEventTriggered?.Invoke(this, EVENT.ON_POINTER_UP, eventData);

            cursorIsDown = false;

            if (!IsInteractable()) {
                return;
            }

            if (cursorIsOver) {
                SetState(STATE.HIGHLIGHTED);
            }
            else {
                SetState(STATE.NORMAL);
            }

            //GameLogSystem.Info("OnPointerUp");
        }

        void ISelectHandler.OnSelect(BaseEventData eventData) {

            if (disableMouse && eventData is PointerEventData) {
                return;
            }

            OnEventTriggered?.Invoke(this, EVENT.ON_SELECT, eventData);

            if (!IsInteractable()) {
                return;
            }

            SetState(STATE.HIGHLIGHTED);

            //GameLogSystem.Info("OnSelect");
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData) {

            OnEventTriggered?.Invoke(this, EVENT.ON_SUBMIT, eventData);

            if (!IsInteractable()) {
                return;
            }

            OnTriggered?.Invoke();

            //GameLogSystem.Info("OnSubmit");
        }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData) {

            OnEventTriggered?.Invoke(this, EVENT.ON_INITIALIZE_POTENTIAL_DRAG, eventData);

            if (!IsInteractable()) {
                return;
            }

            //GameLogSystem.Info("OnInitializePotentialDrag");

        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {

            OnEventTriggered?.Invoke(this, EVENT.ON_BEGIN_DRAG, eventData);

            if (!IsInteractable()) {
                return;
            }

            //GameLogSystem.Info("OnBeginDrag");

        }

        void IDragHandler.OnDrag(PointerEventData eventData) {

            OnEventTriggered?.Invoke(this, EVENT.ON_DRAG, eventData);

            if (!IsInteractable()) {
                return;
            }

            //GameLogSystem.Info("OnDrag");

        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {

            OnEventTriggered?.Invoke(this, EVENT.ON_END_DRAG, eventData);

            if (!IsInteractable()) {
                return;
            }

            //GameLogSystem.Info("OnEndDrag");

        }

        void IDropHandler.OnDrop(PointerEventData eventData) {

            OnEventTriggered?.Invoke(this, EVENT.ON_DROP, eventData);

            if (!IsInteractable()) {
                return;
            }

            //GameLogSystem.Info("OnDrop");

        }

        void IScrollHandler.OnScroll(PointerEventData eventData) {

            OnEventTriggered?.Invoke(this, EVENT.ON_SCROLL, eventData);

            if (!IsInteractable()) {
                return;
            }

            //GameLogSystem.Info("OnScroll");

        }

    }

}