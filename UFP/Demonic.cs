using UnityEngine;
using ThunderRoad;
using System.Collections;

namespace UFP
{
    public class DemonicExplosion : MonoBehaviour
    {
        Item item;
        float explodePower, cooldownDuration, fireLerpDuration;
        private bool imbue = true;
        SpellCastCharge fireSpell;
        public void Awake()
        {
            item = GetComponent<Item>();
            fireSpell = Catalog.GetData<SpellCastCharge>("Fire");
            Imbue();
            Invoke("Imbue", 0.25f);
        }
        public void Setup(float explodePower, float cooldownDuration, float fireLerpDuration)
        {
            this.explodePower = explodePower;
            this.cooldownDuration = cooldownDuration;
            this.fireLerpDuration = fireLerpDuration;
        }

        void Imbue()
        {
            foreach (Imbue imbue in item.imbues)
            {
                imbue.Transfer(fireSpell, imbue.maxEnergy);
                imbue.energy = imbue.maxEnergy;
            }
        }

        public void Explosion()
        {
            imbue = false;
            GameManager.local.StartCoroutine(ItemImbue());
            Catalog.GetData<ItemData>("DemonicExplosionVFX").SpawnAsync(item1 =>
            {
                item1.transform.position = item.flyDirRef.transform.position;
                item1.physicBody.isKinematic = true;
            });
            foreach (Collider collider in Physics.OverlapSphere(item.transform.position, 10f))
            {
                if (collider.GetComponentInParent<Item>() is Item item && !item.mainHandler)
                {
                    Vector3 direction = (collider.transform.position - item.gameObject.transform.position).normalized;
                    collider.attachedRigidbody?.AddForce(direction * explodePower, ForceMode.Impulse);
                }
                if (collider.GetComponentInParent<Creature>() is Creature creature && !creature.isPlayer)
                {
                    Vector3 direction = (collider.transform.position - this.item.gameObject.transform.position).normalized;
                    collider.attachedRigidbody?.AddForce(direction * explodePower, ForceMode.Impulse);
                    creature.TryElectrocute(100, 10, true, true, Catalog.GetData<EffectData>("ImbueFire"));
                    creature.Kill();
                    if (!creature.isKilled) creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                }
            }

        }

        public void OnCollisionEnter(Collision collision)
        {
            if (imbue) Imbue();
            if (item.mainHandler?.playerHand?.controlHand == null) return;
            if (item.mainHandler.playerHand.controlHand.alternateUsePressed && imbue)
            {
                Explosion();
            }
        }
        IEnumerator ItemImbue()
        {
            foreach (Imbue imbue in item.imbues) imbue.energy = 0;
            yield return new WaitForSeconds(cooldownDuration);
            float time = fireLerpDuration;
            while (time > 0)
            {
                time -= Time.deltaTime;
                foreach (Imbue imbue in item.imbues)
                {
                    imbue.Transfer(fireSpell, 1);
                    imbue.energy = Mathf.Lerp(imbue.maxEnergy, 0, time / fireLerpDuration);
                }
                yield return null;
            }
            imbue = true;
            EffectInstance effect = Catalog.GetData<EffectData>("DemonicChargeEffect").Spawn(item.transform);
            effect.Play();
            effect.onEffectFinished += effect2 => effect2.Despawn();
        }
    }


    public class DemonicExplosionItem : ItemModule
    {
        public float explodePower, cooldownDuration, fireLerpDuration;
        public override void OnItemLoaded(Item item)
        {
            item.gameObject.AddComponent<DemonicExplosion>().Setup(explodePower, cooldownDuration, fireLerpDuration);
            base.OnItemLoaded(item);
        }
    }
}