using ThunderRoad;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UFP
{
    public static class Utils
    {
        public static RagdollPart GetRandomSlicePart(this Creature creature)
        {
            List<RagdollPart> parts = new List<RagdollPart>();
            for (var i = 0; i < creature.ragdoll.parts.Count; i++)
            {
                var part = creature.ragdoll.parts[i];
                if (part.sliceAllowed && ((int)part.type) > 0) parts.Add(part);
            }

            return parts.ElementAtOrDefault(Random.Range(0, parts.Count));
        }
    }
}
