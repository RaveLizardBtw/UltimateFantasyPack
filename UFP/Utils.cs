using ThunderRoad;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

namespace UFP
{
    public static class Utils
    {
        public static RagdollPart GetRandomSlicePart(this Creature creature)
        {
            List<RagdollPart> parts = new List<RagdollPart>();
            for (var i = 0; i < creature.ragdoll.parts.Count; i++)
            {
                var part = creature.ragdoll.parts[i];
                if (part.sliceAllowed && ((int)part.type) > 0) parts.Add(part);
            }

            return parts.ElementAtOrDefault(UnityEngine.Random.Range(0, parts.Count));
        }
    }
    public class Timer : MonoBehaviour
    {
        public float TimerDuration;
        public Action OnTimerEnd;
        public bool DestroyOnFinish = true;
        public bool AlwaysActive = true;
        private float _timeRemaining;

        public float SecondsRemaining => this._timeRemaining;

        private void Awake() => this.ResetTimer();

        private void Update()
        {
            if (!this.AlwaysActive)
                return;
            this.TickTimer();
        }

        public void TickTimer() => this._timeRemaining -= Time.deltaTime;

        public void HandleTimerEnd()
        {
            if ((double)this._timeRemaining > 0.0)
                return;
            Action onTimerEnd = this.OnTimerEnd;
            if (onTimerEnd != null)
                onTimerEnd();
            if (this.DestroyOnFinish)
                UnityEngine.Object.Destroy((UnityEngine.Object)this);
            this.ResetTimer();
        }

        public void ResetTimer() => this._timeRemaining = this.TimerDuration;
    }

    public class Cooldown : MonoBehaviour
    {
        private bool isOnCooldown = false;
        public float cooldownLength;
        private float remainingSeconds;
        public Action onCooldownEnd;

        public bool IsOnCooldown => this.isOnCooldown;

        public float RemainingSeconds => this.remainingSeconds;

        private void Awake() => this.remainingSeconds = this.cooldownLength;

        private void Update()
        {
            if (!this.isOnCooldown)
                return;
            this.TickCooldown();
        }

        private void TickCooldown()
        {
            this.remainingSeconds -= Time.deltaTime;
            if ((double)this.remainingSeconds > 0.0)
                return;
            Action onCooldownEnd = this.onCooldownEnd;
            if (onCooldownEnd != null)
                onCooldownEnd();
            this.isOnCooldown = false;
        }

        public void StartCooldown()
        {
            this.isOnCooldown = true;
            this.remainingSeconds = this.cooldownLength;
        }
    }
}
