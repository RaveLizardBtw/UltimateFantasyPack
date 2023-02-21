using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace UltimateFantasyPack
{
    public class Voidborne : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<VoidMono>();
        }
    }
    public class VoidMono : MonoBehaviour
    {
        public static Item item;
        bool Cooldown;
        public void Start()
        {
            Debug.Log("Meteor Item Spawned");
            item = GetComponent<Item>();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            Cooldown = false;
        }
        private IEnumerator voidSmash()
        {
            Cooldown = true;
            yield return new WaitForSeconds(15f);
            Cooldown = false;
        }
        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart && !Cooldown)
            {
                Debug.Log("Alt Use Pressed");
                Catalog.GetData<ItemData>("VoidMeteor").SpawnAsync(Meteor =>
                {
                    Meteor.transform.position = item.transform.position;
                    Meteor.transform.rotation = Player.currentCreature.transform.rotation;
                    Meteor.rb.useGravity = false;
                    Meteor.GetCustomReference("Meteor").GetComponent<ParticleSystem>().gameObject.AddComponent<MeteorCollision>();
                    Meteor.Despawn(6);
                });
                Debug.Log("Meteor spawned in the sky");
                StartCoroutine(voidSmash());
            }
        }
    }

    public class MeteorCollision : MonoBehaviour
    {
        public ParticleSystem part;
        public List<ParticleCollisionEvent> collisionEvents;
        EffectData effect;
        EffectData effect2;
        DamageStruct damageStruct;
        public float blastForce = 20f;

        public void Start()
        {
            part = GetComponent<ParticleSystem>();
            collisionEvents = new List<ParticleCollisionEvent>();
            damageStruct = new DamageStruct(DamageType.Energy, 50f);
            effect = Catalog.GetData<EffectData>("SpellGravityShockwave");
            effect2 = Catalog.GetData<EffectData>("ImbueGravityRagdoll");
        }



        void OnParticleCollision(GameObject other)
        {
            Debug.Log("Collision Added");
            int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);
            effect.Spawn(part.transform).Play();
            int i = 0;
            while (i < numCollisionEvents)
            {
                foreach (Collider collider in Physics.OverlapSphere(part.transform.position, 10))
                {
                    if (collider.attachedRigidbody != Player.local.locomotion.rb && collider.attachedRigidbody != VoidMono.item.rb) collider.attachedRigidbody?.AddForce((collider.transform.position - VoidMono.item.transform.position).normalized * blastForce, ForceMode.Impulse);
                    if (collider.gameObject.GetComponentInParent<Creature>() is Creature creature && !creature.isPlayer)
                    {
                        if (!creature.isKilled) creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                        creature.Kill();
                    }
                }

                i++;
            }
        }
    }
}