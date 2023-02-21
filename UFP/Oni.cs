using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;

namespace UFPScrpts
{
    public class Oni : ItemModule

    {
        public float damageRequired;
        public float materialIncrease;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<OniMono>();
        }
    }

    public class OniMono : MonoBehaviour
    {
        private Item item;
        private EffectData lightningData;
        private CollisionInstance instance;
        private Oni module;
        private bool isActive;
        private Material oniMaterial;
        private Material originalMaterial;
        private Renderer bladeRenderer;
        private GameObject VFX;
        private float damageSiphoned;
        private List<Creature> creaturesSiphoning = new List<Creature>();
        private List<RagdollPart> partsToSlice = new List<RagdollPart>();

        public void Start()
        {
            item = GetComponent<Item>();
            if(UFPDebug.local != null)UFPDebug.local.activeOni = item;
            bladeRenderer = item.GetCustomReference<MeshRenderer>("BladeMesh");
            VFX = item.GetCustomReference("Particles").gameObject;
            Catalog.LoadAssetAsync<Material>("UFP.OniMat", mat => { oniMaterial = mat;}, "OniMaterial");
            
            VFX.gameObject.SetActive(false);
            lightningData = Catalog.GetData<EffectData>("OniLightning");
            isActive = false;
            item.OnUngrabEvent += ItemOnOnUngrabEvent;
            module = item.data.GetModule<Oni>();
            damageSiphoned = 0f;
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandlerOnOnCollisionStartEvent;
            item.OnHeldActionEvent += ItemOnOnHeldActionEvent;
            instance = new CollisionInstance(new DamageStruct(DamageType.Slash, 0.3f));
        }

        private void ItemOnOnHeldActionEvent(RagdollHand ragdollhand, Handle handle, Interactable.Action action)
        {
            if (damageSiphoned >= module.damageRequired && (action == Interactable.Action.AlternateUseStart))
            {
                AbilityStart();
            }
        }

        private void ItemOnOnUngrabEvent(Handle handle, RagdollHand ragdollhand, bool throwing)
        {
            AbilityStop();
        }
        public void AbilityStart()
        {
            foreach (ColliderGroup group in item.colliderGroups)
            {
                foreach (Collider collider in group.colliders)
                {
                    collider.isTrigger = true;
                }
            }
            
            VFX.gameObject.SetActive(true);
            item.mainCollisionHandler.OnCollisionStartEvent -= MainCollisionHandlerOnOnCollisionStartEvent;
            partsToSlice.Clear();
            creaturesSiphoning.Clear();
            item.mainCollisionHandler.OnTriggerEnterEvent += MainCollisionHandlerOnOnTriggerEnterEvent;
            isActive = true;
            StartCoroutine(AbilityCooldown());
        }
        public void MaterialValChange(string var, float value)
        {
            var f = bladeRenderer.material;
            f.SetFloat(var, value);
        }
        public IEnumerator AbilityCooldown()
        {
            yield return Yielders.ForSeconds(10f);
            AbilityStop();
        }
        public void AbilityStop()
        {
            damageSiphoned = 0f;
            item.mainCollisionHandler.OnTriggerEnterEvent -= MainCollisionHandlerOnOnTriggerEnterEvent;
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandlerOnOnCollisionStartEvent;
            foreach (RagdollPart part in partsToSlice)
            {
                part.TrySlice();
            }

            foreach (var creature in partsToSlice.Select(part => part.ragdoll.creature))
            {
                if(!creature.isKilled) creature.Kill();
            }
            partsToSlice.Clear();
            creaturesSiphoning.Clear();
            foreach (ColliderGroup group in item.colliderGroups)
            {
                foreach (Collider collider in group.colliders)
                {
                    collider.isTrigger = false;
                }
            }

            bladeRenderer.materials[1] = originalMaterial;
            MaterialValChange("_Power", 0f);
            VFX.gameObject.SetActive(false);
            isActive = false;
        }

        private void MainCollisionHandlerOnOnTriggerEnterEvent(Collider other)
        {
            if (other.GetComponentInParent<RagdollPart>() is RagdollPart part)
            {
                var creature = part.ragdoll.creature;
                if (part.sliceAllowed || part.isSliced|| creature.isPlayer|| part.type == RagdollPart.Type.Torso) return;
                var e = lightningData.Spawn(item.transform);
                //Catalog.GetData<EffectData>("OniSlice").Spawn(item.transform).Play();
                e.SetIntensity(10f);
                e.SetSource(item.transform);
                e.SetTarget(creature.ragdoll.targetPart.transform);
                e.Play();
                partsToSlice.Add(part);
                if (creature.isKilled || creaturesSiphoning.Contains(creature)) return;
                creaturesSiphoning.Add(creature);
            }
        }

        private void MainCollisionHandlerOnOnCollisionStartEvent(CollisionInstance collisioninstance)
        {
            if (collisioninstance.targetCollider.GetComponentInParent<Creature>())
            {
                if (collisioninstance.damageStruct.damage == Mathf.Infinity) return;
                damageSiphoned += (collisioninstance.damageStruct.damage * 10f);
                MaterialValChange("_Power", damageSiphoned / module.materialIncrease);
                Debug.Log($"Collision Damage dealt {collisioninstance.damageStruct.damage} towards the siphon! Siphon is now at {damageSiphoned}");
            }

        }

        private void Update()
        {
            if (isActive && creaturesSiphoning != null && creaturesSiphoning.Count > 0)
            {
                foreach (Creature creature in creaturesSiphoning)
                {

                    instance.damageStruct.hitRagdollPart = Player.local.creature.ragdoll.rootPart;
                    creature.Damage(instance);
                    if(Player.currentCreature.currentHealth < Player.currentCreature.maxHealth) Player.currentCreature.Heal(instance.damageStruct.damage, Player.currentCreature);
                }
            }
        }
    }
}
