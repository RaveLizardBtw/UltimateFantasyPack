using System.Collections.Generic;
using System.Reflection;
using IngameDebugConsole;
using ThunderRoad;
using UnityEngine;

namespace UFPScrpts
{
    public static class Extensions
    {
        public static T GetField<T>(this object instance, string fieldName)
        {
            if (instance == null) return default;
            var type = instance.GetType();
            FieldInfo[] myField = type.GetFields(
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic);
            Debug.Log(fieldName);
            for (int i = 0; i < myField.Length; i++)
            {
                Debug.Log(myField[i].Name +string.CompareOrdinal(myField[i].Name, fieldName));
                if (myField[i].Name ==fieldName)
                {
                    Debug.Log("Returning variable");
                    return (T)myField[i].GetValue(instance);
                }
            }

            return default;
        }
    }
    public class UFPDebug : CustomData
    {
        public bool isActive;


        public static UFPDebug local;
        public Item activeOni;
        public override void OnCatalogRefresh()
        {
            base.OnCatalogRefresh();
            if (local == null) local = this;
            DebugLogConsole.AddCommand<string>("MaterialDump", "Dumps all material values of selected item",  MatDump, "varName");
            DebugLogConsole.AddCommand<string, string, float>("MaterialValueChange", "Changes value of a material", MaterialValChange, "itemName", "var", "value");
        }
        [ConsoleMethod("MaterialDump", "Dumps all Materials", "varName")]
        public void MatDump(string name)
        {
            var item = Extensions.GetField<Item>(this, name);
            Debug.Log($"Item ID is currently {item.itemId}");
            var f = item.GetCustomReference<MeshRenderer>("BladeMesh");c
            Debug.Log($"Mesh has {f.materials.Length}");
            var e = f.material.shader;
            for (int i = 0; i < e.GetPropertyCount(); i++)
            {
                Debug.Log(e.GetPropertyName(i));
            }
        }

        [ConsoleMethod("MaterialValueChange", "Changes value of a material", "itemName", "var", "value")]
        public void MaterialValChange(string itemName, string var, float value)
        {
            var item = Extensions.GetField<Item>(this, itemName);
            var f = item.GetCustomReference<MeshRenderer>("BladeMesh").material;
            f.SetFloat(var, value);
        }
    }
}