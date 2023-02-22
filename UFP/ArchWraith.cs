using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace UFP
{
    public class ArchWraith : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ArcWraithMono>();
        }
    }
    public class ArcWraithMono : MonoBehaviour
    {
        Item item;
        Item ArchInstance;
        EffectData effect;
        bool isShocking;
        List<Creature> affectedCreatures = new List<Creature>();
        ItemData ArchWraithItem;
        FixedJoint fixedJoint;
        public void Start()
        {
            item = GetComponent<Item>();
            ArchWraithItem = Catalog.GetData<ItemData>("ArchWraithPower");
            effect = Catalog.GetData<EffectData>("ImbueLightningRagdoll");
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if(action == Interactable.Action.AlternateUseStart)
            {
                Player.currentCreature.mana.currentMana--;
                Catalog.GetData<ItemData>("ArchWraithPower").SpawnAsync(Arch =>
                {
                    Arch.transform.position = item.flyDirRef.position;
                    Arch.transform.rotation = Quaternion.identity;
                    Arch.rb.useGravity = false;


                    fixedJoint = Arch.gameObject.AddComponent<FixedJoint>();
                    fixedJoint.connectedBody = item.rb;


                    ArchInstance = Arch;
                });
                foreach(Creature creature in Creature.allActive)
                {
                    if(Vector3.Distance(creature.transform.position, item.flyDirRef.position) < 5f && !creature.isPlayer)
                    {
                        creature.TryElectrocute(10, 10, true, true, effect);
                        creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                        affectedCreatures.Add(creature);
                    }
                }
                isShocking = true;
            }
            else if(action == Interactable.Action.AlternateUseStop)
            {
                if(ArchInstance != null)
                {
                    ArchInstance.Despawn();

                }
                foreach(Creature creature in affectedCreatures)
                {
                    creature.StopShock();
                    affectedCreatures.Remove(creature);
                }
                isShocking = false;
            }
        }
        void Update()
        {
            if (isShocking)
            {
                foreach(Creature creature in affectedCreatures)
                {
                    creature.TryElectrocute(10, 10, true, true, effect);
                }
            }
        }
    }
}
