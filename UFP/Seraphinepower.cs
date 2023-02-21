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
    public class SeraphinePower : ItemModule
        {
            public override void OnItemLoaded(Item item)
            {
                base.OnItemLoaded(item);
                item.gameObject.AddComponent<SeraphineMono>();
        }


    }
    public class SeraphineMono : MonoBehaviour 
    {
        Item item;
        Animator animator;
        AudioSource audio;

        public void Start()
        {
            item = GetComponent<Item>();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            animator = item.GetCustomReference<Animator>("animator");
            audio = item.GetCustomReference<AudioSource>("animator");
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
         if (action == Interactable.Action.AlternateUseStart)
            {
                animator.SetBool("Activated", !animator.GetBool("Activated"));
                audio.enabled = !audio.enabled;
            }
        }
    }

}

