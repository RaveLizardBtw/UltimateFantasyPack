using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace UltimateFantasyPack
{
    class HolyMono : MonoBehaviour
    {
        public Item item;

        public void Awake()
        {
            item = GetComponent<Item>();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if(action == Interactable.Action.AlternateUseStart)
            {
                foreach(Collider collider in Physics.OverlapSphere(Player.local.locomotion.transform.position, 50))
                {
                    collider.TryGetComponent<Creature>(out Creature creature);
                    if(creature != null &&!creature.isKilled && !creature.isPlayer)
                    {
                        creature.locomotion.allowMove = false;
                        GameManager.local.StartCoroutine(SpawnWeapon(creature, 1));
                    }
                }
            }
        }

        public IEnumerator SpawnWeapon(Creature creature, float delay)
        {
            yield return new WaitForSeconds(delay);
            Catalog.GetData<ItemData>("HolyAbilitySword").SpawnAsync(item =>
            {
                item.transform.position = creature.ragdoll.headPart.transform.position + Vector3.up * 10;
                item.Throw();
                item.transform.LookAt(creature.ragdoll.headPart.transform.position);
                item.rb.AddForce(-(item.transform.position - creature.ragdoll.headPart.transform.position) * 5, ForceMode.Impulse);
                item.Despawn(5);
            });
            yield return new WaitForSeconds(1 + delay);
            if(!creature.isKilled) { creature.locomotion.allowMove = true; }
        }

    }

    class Holy : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<HolyMono>();
        }
    }
}
