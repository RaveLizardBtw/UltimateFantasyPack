using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace UFP
{
    public class Vampiric : MonoBehaviour
    {
        private bool imbued = false;
        private bool onCooldown = false;
        private Item item;
        private AudioSource activationAudio;
        private AudioSource readyAudio;
        private SpellCastCharge vampSpell;

        public GameObject bloodVFX;
        public void Start()
        {
            item = GetComponent<Item>();
            vampSpell = Catalog.GetData<SpellCastCharge>("Vamp");
            item.OnDespawnEvent += Item_OnDespawnEvent;
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            activationAudio = item.GetCustomReference("activationAudio").gameObject.GetComponent<AudioSource>();
            bloodVFX = item.GetCustomReference("BloodVFX").gameObject;
            readyAudio = item.GetCustomReference("readyAudio").gameObject.GetComponent<AudioSource>();
            
            bloodVFX.SetActive(false);
            
            EventManager.onCreatureHit += EventManager_onCreatureHit;
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                GameManager.local.StartCoroutine(Imbue());
            }
        }

        private void Item_OnDespawnEvent(EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart)
            {
                item.OnHeldActionEvent -= Item_OnHeldActionEvent;
                item.OnDespawnEvent -= Item_OnDespawnEvent;
                EventManager.onCreatureHit -= EventManager_onCreatureHit;
            }
        }

        private void EventManager_onCreatureHit(Creature creature, CollisionInstance collisionInstance)
        {
            if (!imbued)
                return;
            Player.currentCreature.Heal(collisionInstance.damageStruct.damage * 0.3f, Player.currentCreature);
        }

        public void Update()
        {
            if (!imbued)
                return;
            foreach (Imbue imbue in item.imbues)
            {
                imbue.Transfer(vampSpell, imbue.maxEnergy);
            }
        }
        public IEnumerator Imbue()
        {
            if (!onCooldown)
            {
                onCooldown = true;
                activationAudio.Play();
                bloodVFX.SetActive(true);
                imbued = true;
                yield return new WaitForSeconds(10);
                GameManager.local.StartCoroutine(Cooldown());
                bloodVFX.SetActive(false);
                imbued = false;
            }
        }
        public IEnumerator Cooldown()
        {
            foreach (Imbue imbue in item.imbues)
            {
                imbue.energy = 0f;
            }
            yield return new WaitForSeconds(20);
            onCooldown = false;
            readyAudio.Play();
        }
    }

    public class VampItem : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            item.gameObject.AddComponent<Vampiric>();
            base.OnItemLoaded(item);
        }
    }
    public class VampireLifesteal : MonoBehaviour
    {
        private bool _imbued;
        private Item _item;
        private Cooldown _cooldown;
        private Timer _duration;
        private AudioSource _activationAudio;
        private AudioSource _readyAudio;

        private void Start()
        {
            _item = GetComponent<Item>();
            _activationAudio = this._item.GetCustomReference("activationAudio").gameObject.GetComponent<AudioSource>();
            _readyAudio = this._item.GetCustomReference("readyAudio").gameObject.GetComponent<AudioSource>();
            _cooldown = this.gameObject.AddComponent<Cooldown>();
            _duration = this.gameObject.AddComponent<Timer>();
            _cooldown.cooldownLength = 20f;
            _duration.TimerDuration = 10f;
            _cooldown.onCooldownEnd = () => _readyAudio.Play(0UL);
            _duration.AlwaysActive = false;
            _duration.DestroyOnFinish = false;
            _duration.OnTimerEnd = UnImbue;
            _imbued = false;
        }

        private void Update()
        {
            if (!this._imbued)
                return;
            foreach (ThunderRoad.Imbue imbue in this._item.imbues)
                imbue.Transfer(Catalog.GetData<SpellCastCharge>("Vampiric").Clone(), imbue.maxEnergy);
        }

        private void Imbue()
        {
            this._imbued = true;
            this._duration.AlwaysActive = true;
            EventManager.onCreatureSpawn += new EventManager.CreatureSpawnedEvent(this.OnCreatureSpawn);
            Creature.allActive.Where<Creature>((Func<Creature, bool>)(creature => (UnityEngine.Object)creature.GetComponent<Player>() == (UnityEngine.Object)null)).ToList<Creature>().ForEach((System.Action<Creature>)(creature => creature.OnDamageEvent += new Creature.DamageEvent(this.OnDamage)));
        }

        private void OnCreatureSpawn(Creature creature) => creature.OnDamageEvent += new Creature.DamageEvent(this.OnDamage);

        private void UnImbue()
        {
            this._cooldown.StartCooldown();
            this._imbued = false;
            this._duration.AlwaysActive = false;
            this._duration.ResetTimer();
            foreach (ThunderRoad.Imbue imbue in this._item.imbues)
                imbue.energy = 0.0f;
            EventManager.onCreatureSpawn += new EventManager.CreatureSpawnedEvent(this.OnCreatureSpawn);
            Creature.allActive.Where<Creature>((Func<Creature, bool>)(creature => (UnityEngine.Object)creature.GetComponent<Player>() == (UnityEngine.Object)null)).ToList<Creature>().ForEach((System.Action<Creature>)(creature => creature.OnDamageEvent -= new Creature.DamageEvent(this.OnDamage)));
        }

        private void OnDamage(CollisionInstance collisioninstance)
        {
            if (!(bool)(UnityEngine.Object)collisioninstance.targetCollider.GetComponent<Creature>() || (bool)(UnityEngine.Object)collisioninstance.targetCollider.GetComponent<Player>() || !(bool)(UnityEngine.Object)collisioninstance.sourceCollider.GetComponent<VampireLifesteal>() || !this._imbued)
                return;
            Debug.Log((object)("Healing player for " + (collisioninstance.damageStruct.damage * 0.3f).ToString()));
            Player.currentCreature.Heal(collisioninstance.damageStruct.damage * 0.3f, (Creature)null);
        }

        private void OnHeld(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action != Interactable.Action.AlternateUseStart || this._cooldown.IsOnCooldown || this._imbued)
                return;
            this.Imbue();
        }
    }
}
