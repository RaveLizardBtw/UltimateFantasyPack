using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ThunderRoad;
using ThunderRoad.AI;
using UnityEngine.VFX;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.Collections;

namespace UltimateFantasyPack
{
    public class IceSpike : ItemModule
    {
        public float cooldown, iceForwardStart, downOffset, blastDistance, blastForce;
        public bool killIfStruck;
        public string[] spikeAddressablesNames;

        bool onCooldown;
        GameObject[] spikeGhosts;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            Debug.Log(spikeAddressablesNames.Length);
            spikeGhosts = new GameObject[spikeAddressablesNames.Length];
            for (int i = 0; i < spikeGhosts.Length; i++) Register(i);
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
        }

        void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart && !onCooldown)
            {
                GameObject go = Object.Instantiate(spikeGhosts[Random.Range(0, spikeGhosts.Length)], ragdollHand.creature.ragdoll.transform.position + (ragdollHand.creature.ragdoll.transform.forward * iceForwardStart) - new Vector3(0, downOffset, 0), ragdollHand.creature.ragdoll.transform.rotation);
                go.GetComponentInChildren<ParticleSystem>().Play();
                Object.Destroy(go, 10f);
                GameManager.local.StartCoroutine(Cooldown());
                foreach (Collider collider in Physics.OverlapSphere(item.transform.position, 10))
                {
                    if (collider.attachedRigidbody != Player.local.locomotion.rb && collider.attachedRigidbody != item.rb) collider.attachedRigidbody?.AddForce((collider.transform.position - item.transform.position).normalized * blastForce, ForceMode.Impulse);
                    if (collider.gameObject.GetComponentInParent<Creature>() is Creature creature && !creature.isPlayer)
                    {
                        if (!creature.isKilled) creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                        if (killIfStruck) creature.Kill();
                    }
                }
            }
        }

        IEnumerator Cooldown()
        {
            onCooldown = true;
            yield return new WaitForSeconds(cooldown);
            onCooldown = false;
        }
        void Register(int i) => Catalog.LoadAssetAsync<GameObject>(spikeAddressablesNames[i], go => spikeGhosts[i] = go, "Ultimate Fantasy Pack");
    }
}
