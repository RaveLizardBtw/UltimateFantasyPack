using System.Collections;
using ThunderRoad;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace UFP
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

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart && !onCooldown)
            {
                GameObject go = Object.Instantiate(spikeGhosts[Random.Range(0, spikeGhosts.Length)], ragdollHand.creature.ragdoll.transform.position + (ragdollHand.creature.ragdoll.transform.forward * iceForwardStart) - new Vector3(0, downOffset, 0), ragdollHand.creature.ragdoll.transform.rotation);
                go.GetComponentInChildren<ParticleSystem>().Play();
                Object.Destroy(go, 10f);
                GameManager.local.StartCoroutine(Cooldown());

                foreach (var collider in Physics.OverlapSphere(item.transform.position, 10.0f))
                {
                    if (collider.GetComponentInParent<Creature>() is Creature creature && !creature.isPlayer)
                    {
                        foreach (var parts in creature.ragdoll.parts)
                        {
                            parts?.physicBody?.rigidBody?.AddExplosionForce(blastForce, this.item.transform.position, 10.0f, 1.0f, ForceMode.VelocityChange);
                        }

                        if (!creature.isKilled)
                        {
                            creature?.ragdoll?.SetState(Ragdoll.State.Destabilized);
                            if (killIfStruck) creature.Kill();
                        }
                    }

                    if (collider.GetComponentInParent<Item>() is Item item && item != null && item != this.item)
                    {
                        item?.physicBody?.rigidBody?.AddExplosionForce(blastForce, this.item.transform.position, 10.0f, 1.0f, ForceMode.VelocityChange);

                        if (item.GetComponent<Breakable>() is Breakable breakable)
                        {
                            if (breakable != null)
                            {
                                breakable.Break();

                                foreach (var brokenItems in breakable?.subBrokenItems)
                                {
                                    brokenItems?.physicBody.rigidBody.AddExplosionForce(blastForce, this.item.transform.position, 10.0f, 1.0f, ForceMode.VelocityChange);
                                }

                                foreach (var brokenBodies in breakable?.subBrokenBodies)
                                {
                                    brokenBodies?.rigidBody.AddExplosionForce(blastForce, this.item.transform.position, 10.0f, 1.0f, ForceMode.VelocityChange);
                                }
                            }
                        }
                    }
                }

                /*foreach (Collider collider in Physics.OverlapSphere(item.transform.position, 10))
                {
                    if (collider.attachedRigidbody != Player.local.locomotion.rb && collider.attachedRigidbody != item.physicBody) collider.attachedRigidbody?.AddForce((collider.transform.position - item.transform.position).normalized * blastForce, ForceMode.Impulse);
                    if (collider.gameObject.GetComponentInParent<Creature>() is Creature creature && !creature.isPlayer)
                    {
                        if (!creature.isKilled) creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                        if (killIfStruck) creature.Kill();
                    }
                }*/
            }
        }

        private IEnumerator Cooldown()
        {
            onCooldown = true;
            yield return new WaitForSeconds(cooldown);
            onCooldown = false;
        }

        private void Register(int i) => Catalog.LoadAssetAsync<GameObject>(spikeAddressablesNames[i], go => spikeGhosts[i] = go, "Ultimate Fantasy Pack");
    }
}