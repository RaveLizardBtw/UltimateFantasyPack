using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using ThunderRoad.AI;
using ThunderRoad.AI.Action;
using ThunderRoad.AI.Control;
using UnityEngine;

namespace UFPScrpts
{
    public class OrcModule : ItemModule
    {
        public string effectID;
        
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<OrcMono>();
        }
    }

    public class OrcMono : MonoBehaviour
    {
        private Item item;
        private OrcModule module;
        private readonly float activeTime = 10f;
        private readonly float cooldown = 30f;
        private bool isActive;
        private bool onCooldown;
        public void Start()
        {
            item = GetComponent<Item>();
            module = item.data.GetModule<OrcModule>();
            isActive = false;
            onCooldown = false;
            item.OnHeldActionEvent += ItemOnOnHeldActionEvent;
        }

        public IEnumerator OrcTimings()
        {
            isActive = true;
            onCooldown = true;
            yield return Yielders.ForSeconds(activeTime);
            EventManager.onCreatureSpawn -= EventManagerOnonCreatureSpawn;
            isActive = false;
            yield return Yielders.ForSeconds(cooldown);
            onCooldown = false;

        }
        
        private void ItemOnOnHeldActionEvent(RagdollHand ragdollhand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart && !isActive && !onCooldown)
            {
                if (module.effectID != null)Catalog.GetData<EffectData>(module.effectID).Spawn(item.transform).Play();
                StartCoroutine(OrcTimings());
                for (var i = 0; i < Creature.allActive.Count; i++)
                {
                    var creature = Creature.allActive[i];
                    if (creature.isPlayer || creature.isKilled) continue;
                    var brain = creature.brain;
                    brain.currentTarget = Player.currentCreature;
                    brain.instance.tree.blackboard.UpdateVariable<bool>("ForceFlee", true);
                    Debug.Log("Creature has been forced to flee!");
                }

                EventManager.onCreatureSpawn += EventManagerOnonCreatureSpawn;
            }
        }

        private void EventManagerOnonCreatureSpawn(Creature creature)
        {
            if (creature.isKilled || creature.isPlayer) return;
            creature.brain.currentTarget = Player.currentCreature;
            creature.brain.instance.tree.blackboard.UpdateVariable<bool>("ForceFlee", true);
        }
    }
}