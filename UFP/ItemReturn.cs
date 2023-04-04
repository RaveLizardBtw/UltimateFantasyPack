using ThunderRoad;
using UnityEngine;
namespace UFP
{
    public class ItemReturn : MonoBehaviour
    {
        Item item;
        float returnPower;
        RagdollHand lastHolder;
        public void Setup(float returnPower)
        {
            item = GetComponentInParent<Item>();
            this.returnPower = returnPower;
            item.OnGrabEvent += Grab;
        }
        void Grab(Handle handle, RagdollHand ragdollHand)
        {
            if (lastHolder) lastHolder.OnGrabEvent -= LastHolderGrab;
            lastHolder = ragdollHand;
            lastHolder.OnGrabEvent += LastHolderGrab;
        }

        void LastHolderGrab(Side side, Handle handle, float axisPosition, HandlePose orientation, EventTime eventTime)
        {
            if (handle.item && handle.item != item) lastHolder = null;
        }
        void Update()
        {
            item.physicBody.useGravity = !item.isFlying;
            if (lastHolder?.playerHand?.controlHand != null && lastHolder.playerHand.controlHand.gripPressed && !lastHolder.playerHand.controlHand.castPressed && !item.mainHandler && !item.holder && !lastHolder.grabbedHandle)
            {
                item.physicBody.AddForce((lastHolder.transform.position - item.transform.position).normalized * returnPower, ForceMode.VelocityChange);
                if ((item.transform.position - lastHolder.transform.position).sqrMagnitude < 1) lastHolder.Grab(item.GetMainHandle(lastHolder.side));
            }
        }
    }
    public class ItemReturnItem : ItemModule
    {
        public float returnPower;
        public override void OnItemLoaded(Item item)
        {
            item.gameObject.AddComponent<ItemReturn>().Setup(returnPower);
            base.OnItemLoaded(item);
        }
    }
}