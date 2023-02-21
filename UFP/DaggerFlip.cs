using ThunderRoad;
using UnityEngine;

namespace UFP
{
    public class DaggerFlipItem : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            item.gameObject.AddComponent<DaggerFlip>();
            base.OnItemLoaded(item);
        }
    }

    public class DaggerFlip : MonoBehaviour
    {
        private Item item;

        private void Start()
        {
            item = GetComponent<Item>();
            item.OnHeldActionEvent += onHeld;
        }
        public void FlipDagger(Handle handle, RagdollHand ragdollHand)
        {
            ragdollHand.UnGrab(false);
            item.transform.Rotate(180f, 0.0f, 0.0f, Space.Self);
            ragdollHand.GrabRelative(handle);
        }

        private void onHeld(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.UseStart)
            {
                if (item.isPenetrating == false)
                {
                    FlipDagger(handle, item.mainHandler);
                }
            }

        }
    }
}
