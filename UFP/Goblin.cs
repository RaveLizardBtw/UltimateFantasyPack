using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using ThunderRoad;
using ThunderRoad.AI;
using UnityEngine.VFX;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace UFP
{
    public class Goblin : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.OnSnapEvent += Snap;
            item.OnUnSnapEvent += Unsnap;
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            item.OnGrabEvent += Item_OnGrabEvent;
            item.OnUngrabEvent += Item_OnUngrabEvent;
            if (item.mainHandler != null && item.mainHandler.creature.isPlayer)
            {
                RagdollHandClimb.climbFree = true;
            }
            else RagdollHandClimb.climbFree = false;
        }

        private void Item_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            RagdollHandClimb.climbFree = false;
        }

        private void Item_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            RagdollHandClimb.climbFree = true;
        }

        void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.Grab) RagdollHandClimb.climbFree = true;
            if (action == Interactable.Action.Ungrab) RagdollHandClimb.climbFree = false;
        }
        void Snap(Holder holder)
        {
            if (holder.creature?.player)
            {
                RagdollHandClimb.climbFree = true;
            }
        }
        void Unsnap(Holder holder)
        {
            if (holder.creature?.player)
            {
                RagdollHandClimb.climbFree = false;
            }
        }

    }
}
