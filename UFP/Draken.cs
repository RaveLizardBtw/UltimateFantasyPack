using UnityEngine;
using ThunderRoad;
using System.Collections;

namespace UFP
{
    public class DrakenFlame : MonoBehaviour
    {
        Item item;
        public bool Imbued = false;
        public float timer = 0f;
        public float timer1 = 0f;
        public float force;


        public void Awake()
        {
            item = GetComponent<Item>();
            item.OnHeldActionEvent += OnHeld;
        }

        public void Update()
        {
            timer -= Time.deltaTime;
            timer1 -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = 0f;
            }
            if (timer1 <= 0f)
            {
                timer1 = 0f;
                Imbued = false;
            }
            if (Imbued == true)
            {
                if (timer1 > 0f)
                {
                    foreach (Imbue imbue in item.imbues)
                    {
                        imbue.Transfer(Catalog.GetData<SpellCastCharge>("Fire").Clone(), imbue.maxEnergy);
                    }
                }
            }
            else if (Imbued == false)
            {
                item.GetCustomReference("FireMesh").gameObject.SetActive(false);
                item.GetCustomReference("NormalMesh").gameObject.SetActive(true);
                foreach (Imbue imbue in item.imbues)
                {
                    imbue.energy = 0;
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (Imbued == true)
            {
                if (collision.collider.gameObject.GetComponentInParent<Creature>() != null)
                {
                    if (collision.collider.gameObject.GetComponentInParent<Player>() == null)
                    {
                        collision.collider.GetComponentInParent<Creature>().ragdoll.SetState(Ragdoll.State.Inert);
                        collision.collider.GetComponentInParent<Creature>().TryElectrocute(50f, 5, true, true, Catalog.GetData<EffectData>("ImbueFireRagdoll", true));
                        Vector3 direction = (collision.collider.transform.position - item.gameObject.transform.position).normalized;
                        collision.collider.attachedRigidbody?.AddForce(direction * force * collision.collider.attachedRigidbody.mass, ForceMode.Impulse);
                    }
                }

                if (collision.collider.gameObject.GetComponentInParent<Item>() != null)
                {
                    Vector3 direction = (collision.collider.transform.position - item.gameObject.transform.position).normalized;
                    collision.collider.attachedRigidbody?.AddForce(direction * force * collision.collider.attachedRigidbody.mass, ForceMode.Impulse);
                }
            }
        }

        private void OnHeld(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                if (timer <= 0f)
                {
                    if (Imbued == false)
                    {
                        Imbued = true;
                        timer1 = 10f;
                        timer = 20f;
                        item.GetCustomReference("FireMesh").gameObject.SetActive(true);
                        item.GetCustomReference("NormalMesh").gameObject.SetActive(false);
                        EffectInstance effect = Catalog.GetData<EffectData>("DrakenChargeEffect").Spawn(item.transform);
                        effect.Play();
                        effect.onEffectFinished += effect2 => effect2.Despawn();
                    }
                }
            }

        }
    }

    public class DrakenFlameItem : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            item.gameObject.AddComponent<DrakenFlame>();
            base.OnItemLoaded(item);
        }
    }
}
