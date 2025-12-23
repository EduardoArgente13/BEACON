using System.Collections;
using UnityEngine;
using BEACON.Core;

namespace BEACON.Managers
{
    /// <summary>
    /// Centralized time control for hitstop, slow motion, and other time effects.
    /// Uses Singleton pattern for global access.
    /// </summary>
    public class TimeManager : Singleton<TimeManager>
    {
        // ============ STATE ============
        
        private Coroutine activeHitstopCoroutine;
        private Coroutine activeSlowMotionCoroutine;
        private float targetTimeScale = 1f;
        private float originalFixedDeltaTime;

        // ============ PROPERTIES ============

        /// <summary>True if currently in hitstop/freeze</summary>
        public bool IsInHitstop { get; private set; }

        /// <summary>True if currently in slow motion</summary>
        public bool IsInSlowMotion { get; private set; }

        /// <summary>Current time scale (0-1)</summary>
        public float CurrentTimeScale => Time.timeScale;

        // ============ INITIALIZATION ============

        protected override void OnSingletonAwake()
        {
            originalFixedDeltaTime = Time.fixedDeltaTime;
        }

        // ============ HITSTOP ============

        /// <summary>
        /// Freezes time briefly for impact feedback.
        /// Used for hits, parries, and important collisions.
        /// </summary>
        /// <param name="duration">Duration in real-time seconds</param>
        public void DoHitstop(float duration)
        {
            // Cancel existing hitstop if any
            if (activeHitstopCoroutine != null)
            {
                StopCoroutine(activeHitstopCoroutine);
            }

            activeHitstopCoroutine = StartCoroutine(HitstopCoroutine(duration));
        }

        private IEnumerator HitstopCoroutine(float duration)
        {
            IsInHitstop = true;
            Time.timeScale = 0f;
            Time.fixedDeltaTime = 0f;

            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale = targetTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime * targetTimeScale;
            IsInHitstop = false;
            activeHitstopCoroutine = null;
        }

        // ============ SLOW MOTION ============

        /// <summary>
        /// Enters slow motion for a duration.
        /// Used for Void State, parry success, dramatic moments.
        /// </summary>
        /// <param name="timeScale">Target time scale (0.1 = 10% speed)</param>
        /// <param name="duration">Duration in real-time seconds</param>
        /// <param name="easeIn">Duration to ease into slow motion</param>
        /// <param name="easeOut">Duration to ease back to normal</param>
        public void DoSlowMotion(float timeScale, float duration, float easeIn = 0f, float easeOut = 0.2f)
        {
            if (activeSlowMotionCoroutine != null)
            {
                StopCoroutine(activeSlowMotionCoroutine);
            }

            activeSlowMotionCoroutine = StartCoroutine(SlowMotionCoroutine(timeScale, duration, easeIn, easeOut));
        }

        private IEnumerator SlowMotionCoroutine(float targetScale, float duration, float easeIn, float easeOut)
        {
            IsInSlowMotion = true;
            float startScale = Time.timeScale;

            // Ease in
            if (easeIn > 0)
            {
                float elapsed = 0f;
                while (elapsed < easeIn)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / easeIn;
                    SetTimeScale(Mathf.Lerp(startScale, targetScale, EaseOutQuad(t)));
                    yield return null;
                }
            }

            SetTimeScale(targetScale);

            // Hold
            yield return new WaitForSecondsRealtime(duration);

            // Ease out
            if (easeOut > 0)
            {
                float elapsed = 0f;
                startScale = Time.timeScale;
                while (elapsed < easeOut)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / easeOut;
                    SetTimeScale(Mathf.Lerp(startScale, 1f, EaseOutQuad(t)));
                    yield return null;
                }
            }

            SetTimeScale(1f);
            IsInSlowMotion = false;
            activeSlowMotionCoroutine = null;
        }

        /// <summary>
        /// Instantly sets time scale.
        /// </summary>
        public void SetTimeScale(float scale)
        {
            targetTimeScale = scale;
            Time.timeScale = scale;
            Time.fixedDeltaTime = originalFixedDeltaTime * scale;
        }

        /// <summary>
        /// Resets time to normal.
        /// </summary>
        public void ResetTime()
        {
            if (activeHitstopCoroutine != null)
            {
                StopCoroutine(activeHitstopCoroutine);
                activeHitstopCoroutine = null;
            }
            
            if (activeSlowMotionCoroutine != null)
            {
                StopCoroutine(activeSlowMotionCoroutine);
                activeSlowMotionCoroutine = null;
            }

            IsInHitstop = false;
            IsInSlowMotion = false;
            SetTimeScale(1f);
        }

        // ============ EASING ============

        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private float EaseInQuad(float t)
        {
            return t * t;
        }

        // ============ CLEANUP ============

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            // Ensure time is reset when quitting
            Time.timeScale = 1f;
            Time.fixedDeltaTime = originalFixedDeltaTime;
        }
    }
}
