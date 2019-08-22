﻿using System;

#if EDITOR_BUILD
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

namespace BulwarkStudios.GameSystems.Platform {

    public sealed class Unity : BasePlatform<IUnityConfig> {

        /// <summary>
        /// Initialize
        /// </summary>
        protected override void Initialize() {

        }

        /// <summary>
        /// Get the user name
        /// </summary>
        /// <returns></returns>
        protected override string GetUserName() {
            return string.Empty;
        }

        /// <summary>
        /// Update
        /// </summary>
        protected override void Update() {

        }

        /// <summary>
        /// Destroy
        /// </summary>
        protected override void OnDestroy() {

        }

        /// <summary>
        /// Get the current language
        /// </summary>
        /// <returns></returns>
        protected override SystemLanguage GetLanguage() {
            return Application.systemLanguage;
        }

        /// <summary>
        /// Has a dlc ?
        /// </summary>
        protected override bool HasDlc(PlatformDlc dlc) {
            return dlc.hasInEditor;
        }

        /// <summary>
        /// Buy a Dlc
        /// </summary>
        /// <param name="dlc"></param>
        /// <param name="result"></param>
        protected override void BuyDlc(PlatformDlc dlc, System.Action<PlatformDlcResult> result) {

            if (result != null) {
                result(new PlatformDlcResult(PlatformDlcResult.STATE.FAIL, "Not implemented"));
            }

        }

        /// <summary>
        /// Reset all achievements
        /// </summary>
        protected override void ResetAllAchievements() {
            Debug.Log("ResetAllAchievements");
        }

        /// <summary>
        /// Reset an achievement
        /// </summary>
        /// <param name="achievement"></param>
        protected override void ResetAchievement(PlatformAchievement achievement) {
            Debug.Log("ResetAchievement " + achievement.steamId);
        }

        /// <summary>
        /// Unlock an achievment
        /// </summary>
        /// <param name="achievement"></param>
        protected override void UnlockAchievement(PlatformAchievement achievement) {

            // Constraint?
            if (achievementUnlockConstraint != null && achievementUnlockConstraint()) {
                Debug.Log("UnlockAchievement constrained: " + achievement.steamId);
                return;
            }

            Debug.Log("UnlockAchievement " + achievement.steamId);
        }

        /// <summary>
        /// Refresh an achievement
        /// </summary>
        /// <param name="achievement"></param>
        protected override void RefreshAchievement(PlatformAchievement achievement) {
            Debug.Log("RefreshAchievement " + achievement.steamId);
        }

        /// <summary>
        /// Analytics enabled state
        /// </summary>
        /// <param name="value"></param>
        protected override void AnalyticsEnabled(bool value) {
            Analytics.enabled = value;
            PerformanceReporting.enabled = value;
        }

        /// <summary>
        /// Analytics game start
        /// </summary>
        protected override void AnalyticsGameStart() {
            AnalyticsEvent.GameStart();
        }

        /// <summary>
        /// Analytics game over
        /// </summary>
        /// <param name="name"></param>
        protected override void AnalyticsGameOver(string name) {
            AnalyticsEvent.GameOver(name);
        }

        /// <summary>
        /// Analytics level start
        /// </summary>
        /// <param name="name"></param>
        /// <param name="eventData"></param>
        protected override void AnalyticsLevelStart(string name, IDictionary<string, object> eventData = null) {
            AnalyticsEvent.LevelStart(name, eventData);
        }

        /// <summary>
        /// Analytics level up
        /// </summary>
        /// <param name="name"></param>
        /// <param name="eventData"></param>
        protected override void AnalyticsLevelUp(string name, IDictionary<string, object> eventData = null) {
            AnalyticsEvent.LevelUp(name, eventData);
        }

        /// <summary>
        /// Analytics level complete
        /// </summary>
        /// <param name="name"></param>
        /// <param name="eventData"></param>
        protected override void AnalyticsLevelComplete(string name, IDictionary<string, object> eventData = null) {
            AnalyticsEvent.LevelComplete(name, eventData);
        }

        /// <summary>
        /// Analytics level fail
        /// </summary>
        /// <param name="name"></param>
        /// <param name="eventData"></param>
        protected override void AnalyticsLevelFail(string name, IDictionary<string, object> eventData = null) {
            AnalyticsEvent.LevelFail(name, eventData);
        }

        /// <summary>
        /// Analytics cutscene skip
        /// </summary>
        /// <param name="name"></param>
        protected override void AnalyticsCutsceneSkip(string name) {
            AnalyticsEvent.CutsceneSkip(name);
        }

        /// <summary>
        /// Analytics tutorial skip
        /// </summary>
        /// <param name="name"></param>
        protected override void AnalyticsTutorialSkip(string name) {
            AnalyticsEvent.TutorialSkip(name);
        }

    }

}

#endif