using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace UltimateFantasyPack
{
    class Gnomish : ItemModule
    {
        public float Cooldown = 10;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<GnomishMono>().settings = this;
        }
    }

    class GnomishMono : MonoBehaviour
    {
        public Item item;
        public Gnomish settings;
        private bool onCooldown = false;
        private bool active = false;

        public void Awake()
        {
            item = GetComponent<Item>();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if(action == Interactable.Action.AlternateUseStart)
            {
                GameManager.local.StartCoroutine(Ability(settings.Cooldown));
            }
        }

        public IEnumerator Ability(float cooldown)
        {
            onCooldown = true;
            item.GetCustomReference("Sparks").gameObject.SetActive(true);
            active = true;
            yield return new WaitForSeconds(cooldown);
            item.GetCustomReference("Sparks").gameObject.SetActive(false);
            active = false;
            onCooldown = false;

        }

        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if (active && collisionInstance.targetCollider.GetComponentInParent<Creature>() is Creature creature)
            {
                creature.TryElectrocute(50f, 5, true, true, Catalog.GetData<EffectData>("ImbueLightningRagdoll", true));
            }
        }
    }
}
