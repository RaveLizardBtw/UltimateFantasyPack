using System;
using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace UFP
{
    public class Atlantean : ItemModule
    {
        public float cooldown = 5;
        
        
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<AtlanteanMono>().settings = this;
        }
    }




    public class AtlanteanMono : MonoBehaviour
    {
        private Item item;
        private Item bubbleItem;
        private bool onCooldown;
        public Atlantean settings;
        private bool bubbleSpawned;
        private Creature creature;


        public void Start()
        {
            item = GetComponent<Item>();
            item.OnHeldActionEvent += ItemOnOnHeldActionEvent;
        }

        private void ItemOnOnHeldActionEvent(RagdollHand ragdollhand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart && !onCooldown)
            {
                foreach(Collider collider in Physics.OverlapSphere(item.transform.position, 25f))
                {
                    if (collider.GetComponentInParent<Creature>())
                    {
                        creature = collider.GetComponentInParent<Creature>();
                        
                        if (!creature.isPlayer && !creature.isKilled)
                        {
                            Catalog.GetData<ItemData>("WaterBubble").SpawnAsync(bubble =>
                            {
                                bubble.transform.position = creature.ragdoll.headPart.transform.position;
                                bubble.transform.SetParent(creature.ragdoll.headPart.transform);

                                bubbleItem = bubble;


                                StartCoroutine(Cooldown());

                                StartCoroutine(FloatEnemy(creature, bubble));
                                bubbleSpawned = true;

                            });
                        }
                    }
                }
            }
        }


        public IEnumerator Cooldown()
        {
            onCooldown = true;
            yield return new WaitForSeconds(settings.cooldown);
            onCooldown = false;
        }

        public IEnumerator FloatEnemy(Creature creatureFloat, Item itemBubble)
        {
            creatureFloat.ragdoll.SetPhysicModifier(this, 0);
            creatureFloat.locomotion.SetPhysicModifier(this, 0);
            creatureFloat.brain.AddNoStandUpModifier(this);
            creatureFloat.ragdoll.SetState(Ragdoll.State.Destabilized);
            yield return new WaitForSeconds(2);

            foreach(RagdollPart part in creatureFloat.ragdoll.parts) { part.rb.isKinematic = true; }

            yield return new WaitForSeconds(4f);

            foreach (RagdollPart part in creatureFloat.ragdoll.parts) { part.rb.isKinematic = false; }
            creatureFloat.ragdoll.RemovePhysicModifier(this);
            creatureFloat.locomotion.RemovePhysicModifier(this);
            creatureFloat.brain.RemoveNoStandUpModifier(this);

            itemBubble.Despawn();
            bubbleSpawned = false;

        }


        public void Update()
        {
            if (bubbleSpawned)
            {
                bubbleItem.transform.position = creature.ragdoll.headPart.transform.position;
            }
            
        }
    }
}