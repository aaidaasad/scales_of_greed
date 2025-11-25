using UnityEngine;
using System.Collections.Generic;

public class Highlightable : MonoBehaviour
{
    public Material outlineMaterial;

    List<Renderer> validRenderers = new List<Renderer>();
    Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    void Awake()
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in allRenderers)
        {
            // ❌ 跳过特效类渲染器
            if (r is ParticleSystemRenderer) continue;
            if (r is TrailRenderer) continue;
            if (r is LineRenderer) continue;
#if UNITY_2022_1_OR_NEWER
            if (r.GetType().Name == "VFXRenderer") continue; // VisualEffect Graph 渲染器
#endif

            // ✔ 只处理 MeshRenderer / SkinnedMeshRenderer
            validRenderers.Add(r);
            originalMaterials[r] = r.materials;
        }
    }

    public void SetHighlight(bool enabled)
    {
        if (outlineMaterial == null) return;

        foreach (Renderer r in validRenderers)
        {
            if (enabled)
            {
                var baseMats = originalMaterials[r];
                Material[] newMats = new Material[baseMats.Length + 1];
                baseMats.CopyTo(newMats, 0);
                newMats[newMats.Length - 1] = outlineMaterial;
                r.materials = newMats;
            }
            else
            {
                r.materials = originalMaterials[r];
            }
        }
    }
}
