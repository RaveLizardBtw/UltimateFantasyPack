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
        Item item;
        SpellCastCharge spell;

        public void Start()
        {
            item = GetComponent<Item>();
            spell = Catalog.GetData<SpellCastCharge>("Fire");
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
