using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace UFP
{

    public class DarkWeapons : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<DarkMono>();
        }

    }

    public class DarkMono : MonoBehaviour
    {
        Item item;
        bool active;
        Color purple = new Color(1, 0, 1, 1);
        Color clear = new Color(0, 0, 0);
        float cooldown = 15;
        Item portalItem;
        bool canUse;

        public void Start()
        {
            item = GetComponent<Item>();

            canUse = true;
            

            item.OnHeldActionEvent += Item_OnHeldActionEvent;
        }


        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            
            if (canUse && Vector3.Dot(item.physicBody.velocity, item.transform.right) > 0.7f && !active && action == Interactable.Action.AlternateUseStart)
            {
                Debug.Log("portal opened");
                active = true;
                Catalog.GetData<ItemData>("DarkPortal").SpawnAsync(item2 =>
                {
                    portalItem = item2;

                    if(Physics.Raycast(item.transform.position, -Vector3.up, out RaycastHit hit) && !hit.collider.GetComponentInParent<Item>())
                    {

                        portalItem.transform.position = hit.point + new Vector3(0, 0.1f, 0);
                    }

                    portalItem.transform.localScale = new Vector3(1, 5, 1);
                });
                StartCoroutine(CoolDown());
            }
            else if (action == Interactable.Action.AlternateUseStart && active)
            {
                Debug.Log("portal closed");
                Player.currentCreature.Teleport(portalItem.transform.position, Player.currentCreature.transform.rotation);
                portalItem.Despawn(0.1f);
                active = false;
            }
        }

        IEnumerator CoolDown()
        {
            canUse = false;
            yield return new WaitForSeconds(cooldown);
            canUse = true;
        }
    }

}
