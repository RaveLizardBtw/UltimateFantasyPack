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
    public class Fireborn : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<FirebornMono>();
        }
    }
    public class FirebornMono : MonoBehaviour
    {
        float factor = Mathf.Pow(2, 2.5f);
        Color color = new Color(1 * factor, 0.5 * factor, 0 * factor);
        Item item;
        SpellCastCharge spell;
        MeshRenderer mesh;

        public void Start()
        {
            item = GetComponent<Item>();
            spell = Catalog.GetData<SpellCastCharge>("Fire");
            mesh = item.GetCustomReference<MeshRenderer>("mesh");
            mesh.material.SetColor("_EmissionColor", Color.Lerp(Color.red , Color.yellow, 0.2f));
        }

        public void Update()
        {
            foreach (Imbue imbue in item.imbues)
            {
                imbue.spellCastBase = spell;
                imbue.energy = imbue.maxEnergy;
            }
        }
    }
}
