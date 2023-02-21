using System;
using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace UltimateFantasyPack
{
    public class Goliath : ItemModule
    {
        public float delay = 3;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<GoliathMono>().settings = this;
        }
    }




    public class GoliathMono : MonoBehaviour
    {
        public Item item;
        public bool isHeld;
        public Goliath settings;
        public AudioSource crushSource;

        public bool IsHead(RagdollPart part)
        {
            if (part.type == RagdollPart.Type.Head)
            {
                return true;
            }
            else return false;
        }
        
        public void Awake()
        {
            item = GetComponent<Item>();
            if (item.mainHandler != null) { isHeld = true; }
            else { isHeld = false; }
            item.OnGrabEvent += Item_OnGrabEvent;
            item.OnUngrabEvent += Item_OnUngrabEvent;
        }

        private void Item_OnUngrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
        {
            isHeld = false;
        }

        private void Item_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            isHeld = true;
        }

        public void Update()
        {
            if (isHeld && item.mainHandler?.otherHand.grabbedHandle?.GetComponentInParent<Creature>() != null && item.mainHandler.playerHand.ragdollHand.otherHand.playerHand.controlHand.usePressed)
            {
                Creature creature = item.mainHandler.otherHand.grabbedHandle.GetComponentInParent<Creature>();
                if (item.mainHandler?.playerHand.ragdollHand.otherHand.grabbedHandle.GetComponentInParent<RagdollPart>().type == RagdollPart.Type.Head && item.mainHandler.otherHand.playerHand.controlHand.usePressed)
                {
                    GameManager.local.StartCoroutine(Delay(creature, 3));
                }
            }
        }
        public IEnumerator Delay(Creature creature, float delay)
        {
            yield return new WaitForSeconds(delay);
            if(item.mainHandler.otherHand.grabbedHandle.GetComponentInParent<RagdollPart>() is RagdollPart ragdollPart)
            {
                if (IsHead(ragdollPart) && ragdollPart.ragdoll.creature == creature)
                {
                    Decap(creature);
                }
            }
        }

        public void Decap(Creature creature)
        {
            Catalog.LoadAssetAsync<AudioClip>("UFP.HeadSplat", audioClip =>
            {
                GameObject obj = new GameObject();
                AudioSource source = obj.AddComponent<AudioSource>();
                source.clip = audioClip;
                source.transform.position = creature.ragdoll.headPart.transform.position;
                source.Play();
                GameManager.local.StartCoroutine(DespawnSound(obj, audioClip.length));
            }, "UFP");
            creature.ragdoll.headPart.sliceAllowed = true;
            creature.ragdoll.headPart.TrySlice();
            item.mainHandler.playerHand.ragdollHand.otherHand.playerHand.ragdollHand.UnGrab(false);
            creature.ragdoll.headPart.gameObject.SetActive(false);
            creature.Kill();
        }

        public IEnumerator DespawnSound(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            GameObject.Destroy(obj, delay);
        }
    }
}