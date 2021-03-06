﻿using System.Collections.Generic;
using BulwarkStudios.GameSystems.Logs;
using BulwarkStudios.GameSystems.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BulwarkStudios.GameSystems.Events {

    public abstract class GameEvent<T, P1> : ScriptableObjectSingleton<T>, IGameEvent
        where T : GameEvent<T, P1>, new() {

        /// <summary>
        /// Get the instance
        /// </summary>
        public new static T Instance {
            get {
                GameEventSystem.Load();
                return instance;
            }
        }

        /// <summary>
        /// Deactive the logs?
        /// Get the instance
        /// </summary>
        [SerializeField]
        private bool deactiveLogs = false;

#if UNITY_EDITOR
        /// <summary>
        /// Editor initialization
        /// </summary>
        public static GameEvent<T, P1> EditorInitialize() {
            return CreateSingleton(GameEventConstants.RESOURCE_GAMESYSTEM_EVENT_LIST_FOLDER);
        }
#endif

        /// <summary>
        /// List of events
        /// </summary>
        [ShowInInspector, ReadOnly]
        private List<GameEventData1<P1>> events = new List<GameEventData1<P1>>();

        /// <summary>
        /// Listen to the event
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="callback"></param>
        public static GameEventData Listen(object listener, System.Action<P1> callback) {

            GameEventData1<P1> data = new GameEventData1<P1>(listener, callback);
            Instance.events.Add(data);
            return data;

        }

        /// <summary>
        /// Unlisten the event
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="callback"></param>
        public static void Unlisten(object listener, System.Action<P1> callback) {

            for (int i = Instance.events.Count - 1; i >= 0; i--) {

                if (Instance.events[i].listener == listener && Instance.events[i].callback == callback) {
                    Instance.events.RemoveAt(i);
                    break;
                }

            }

        }

        /// <summary>
        /// Trigger the event
        /// </summary>
        [Button]
        public static void Trigger(P1 p1) {

            if (!Instance.deactiveLogs) {
                GameLogSystem.Info("Event triggered: " + Instance + " p1: " + p1, GameEventConstants.LOG_TAG);
            }

            for (int i = Instance.events.Count - 1; i >= 0; i--) {

                if (i < 0 || i >= Instance.events.Count) {
                    continue;
                }

                if (!Instance.events[i].CanTriggerEvent()) {
                    if (!Instance.deactiveLogs) {
                        GameLogSystem.Info(
                            "Event can't be triggered: " + Instance.events[i].listener + " " +
                            Instance.events[i].callback, GameEventConstants.LOG_TAG);
                    }

                    continue;
                }

                Instance.events[i].callback?.Invoke(p1);
            }

        }

        #region Implementation of IGameEvent

        /// <summary>
        /// Add a listener
        /// </summary>
        /// <param name="listener"></param>
        void IGameEvent.Listen(object listener) {
            Listen(listener, null);
        }

        /// <summary>
        /// Remove a listener
        /// </summary>
        void IGameEvent.Unlisten(object listener) {
            Unlisten(listener, null);
        }

        /// <summary>
        /// Trigger
        /// </summary>
        void IGameEvent.Trigger() {
            Trigger(default(P1));
        }

        #endregion

    }

}