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
    public class LightningStrikeNew : MonoBehaviour
    {
        public float explosionForce = 7, explosionRange = 10, airDuration = 2;
        Item item;
        SpellCastCharge lightningSpell;
        bool canPound, imbued, held;
        AudioSource audioSource;
        Coroutine airCheck;

        public void Setup(float explosionForce, float explosionRange, float airDuration)
        {
            item = GetComponent<Item>();
            lightningSpell = Catalog.GetData<SpellCastCharge>("Lightning");
            audioSource = item.GetCustomReference("audioSource").gameObject.GetComponent<AudioSource>();


            item.OnGrabEvent += OnGrab;
            item.OnUngrabEvent += OnUnGrab;
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
            this.explosionForce = explosionForce;
            this.explosionRange = explosionRange;
            this.airDuration = airDuration;
        }

        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if (imbued && canPound && PlayerControl.GetHand(Side.Left).alternateUsePressed || imbued && canPound && PlayerControl.GetHand(Side.Right).alternateUsePressed)
            {
                imbued = false;
                canPound = false;

                Catalog.GetData<EffectData>("SpellLightningStaffSlam", true).Spawn(collisionInstance.contactPoint, Quaternion.LookRotation(collisionInstance.contactNormal)).Play();


                foreach (Imbue imbue in item.imbues)
                {
                    imbue.energy = 0;
                }
                foreach (Collider collider in Physics.OverlapSphere(item.transform.position, explosionRange))
                {
                    if (collider.GetComponentInParent<Item>() is Item item && !item.mainHandler)
                    {
                        Vector3 direction = (collider.transform.position - item.gameObject.transform.position).normalized;
                        collider.attachedRigidbody?.AddForce(direction * explosionForce, ForceMode.Impulse);
                    }
                    if (collider.GetComponentInParent<Creature>() is Creature creature && !creature.isPlayer)
                    {
                        Vector3 direction = (collider.transform.position - this.item.gameObject.transform.position).normalized;
                        collider.attachedRigidbody?.AddForce(direction * explosionForce, ForceMode.Impulse);
                        if (!creature.isKilled) creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                        creature.TryElectrocute(50f, 5, true, true, Catalog.GetData<EffectData>("ImbueLightningRagdoll", true));
                    }
                }
            }
        }

        public void Update()
        {
            if (held && item.transform.position.y > Player.local.creature.ragdoll.headPart.transform.position.y + 0.1f && !imbued && item.mainHandler?.creature == Player.currentCreature && airCheck == null) airCheck = GameManager.local.StartCoroutine(AirCheck());
        }

        public IEnumerator AirCheck()
        {
            float time = airDuration;
            while (time > 0)
            {
                time -= Time.deltaTime;
                if (item.transform.position.y < Player.local.creature.ragdoll.headPart.transform.position.y + 0.1f || !item.mainHandler)
                {
                    airCheck = null;
                    yield break;
                }
                yield return null;
            }
            imbued = true;
            canPound = true;
            foreach (Imbue imbue in item.imbues)
            {
                imbue.Transfer(lightningSpell, imbue.maxEnergy);
            }

            var BoltEffect = Catalog.GetData<EffectData>("SpellLightningBolt").Spawn(item.transform);


            GameObject strike = new GameObject();

            strike.transform.position =
                Player.local.creature.ragdoll.headPart.transform.position + new Vector3(0, 50, 0);

            int bolts = 0;
            while (bolts < 35)
            {
                BoltEffect.SetSource(strike.transform);
                BoltEffect.SetTarget(audioSource.transform);
                BoltEffect.Play();
                bolts++;
            }


            audioSource.Play();
            airCheck = null;
        }

        void OnUnGrab(Handle handle, RagdollHand ragdollHand, bool throwing) => held = false;

        void OnGrab(Handle handle, RagdollHand ragdollHand) => held = true;
    }




    public class LightningStrikeModule : ItemModule
    {
        public float explosionForce, explosionRange, airDuration;
        public override void OnItemLoaded(Item item)
        {
            item.gameObject.AddComponent<LightningStrikeNew>().Setup(explosionForce, explosionRange, airDuration);
            base.OnItemLoaded(item);
        }
    }
}