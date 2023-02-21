using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ThunderRoad;

namespace UFP
{
    public class FelPower : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<FelMono>();
        }

    }

    public class FelMono : MonoBehaviour
    {
        Item item;
        bool cooldown;
        bool active;
        AudioSource scream;
        AudioSource lightUp;

        MeshRenderer mesh;
        GameObject Fire;
        public void Start()
        {
            item = GetComponent<Item>();
            cooldown = false;
            active = false;
            mesh = item.GetCustomReference<MeshRenderer>("mesh");
            Fire = item.GetCustomReference("Fire").gameObject;
            scream = item.GetCustomReference<AudioSource>("mesh");
            lightUp = item.GetCustomReference<AudioSource>("LightUp");

            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
        }

        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if (active && collisionInstance.targetCollider.GetComponentInParent<Creature>() is Creature creature)
            {
                AbilityCancel();
                creature.brain.SetState(Brain.State.Idle);
                creature.brain.currentTarget = null;
                creature.SetFaction(2);
                creature.SetColor(Color.green, Creature.ColorModifier.EyesSclera, true);
                creature.SetColor(Color.green, Creature.ColorModifier.EyesIris, true);
                scream.Play();
                
            }
        }

        private IEnumerator Cooldown()
        {
            yield return Yielders.ForSeconds(10);
            AbilityCancel();
            yield return Yielders.ForSeconds(20);
            cooldown = false;
        }

        public void AbilityCancel()
        {
            Fire.SetActive(false);
            mesh.material.SetColor("_EmissionColor", Color.black);
            active = false;
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart && cooldown == false)
            {
                active = true;
                cooldown = true;
                mesh.material.SetColor("_EmissionColor", Color.green);
                Fire.SetActive(true);
                StartCoroutine(Cooldown());
                lightUp.Play();
            }
            
            
        }
    }
}
