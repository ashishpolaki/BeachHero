using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class GpuInstancedVisual : MonoBehaviour
    {
        private static readonly HashSet<int> warnedMaterials = new();

        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;

        private bool isRegistered;
        private bool isUsingSourceRendererFallback;
        private bool sourceRendererWasEnabled;

        public Matrix4x4 LocalToWorldMatrix => transform.localToWorldMatrix;
        internal bool ShouldRenderInstanced => isRegistered && !isUsingSourceRendererFallback && isActiveAndEnabled;

        private void Awake()
        {
            CacheComponents();
            sourceRendererWasEnabled = meshRenderer != null && meshRenderer.enabled;
        }

        private void OnEnable()
        {
            CacheComponents();
            sourceRendererWasEnabled = meshRenderer != null && meshRenderer.enabled;
            isUsingSourceRendererFallback = false;

            if (!sourceRendererWasEnabled || !CanUseInstancing() || !GpuInstancingManager.TryGetOrCreate(out GpuInstancingManager manager))
            {
                RestoreSourceRenderer();
                return;
            }

            isRegistered = manager.Register(this, meshFilter.sharedMesh, meshRenderer);
            if (isRegistered)
            {
                meshRenderer.enabled = false;
            }
            else
            {
                RestoreSourceRenderer();
            }
        }

        private void OnDisable()
        {
            Unregister();
            RestoreSourceRenderer();
        }

        private void OnDestroy()
        {
            Unregister();
        }

        private void OnValidate()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }
        }

        private bool CanUseInstancing()
        {
            if (!SystemInfo.supportsInstancing || meshFilter == null || meshFilter.sharedMesh == null || meshRenderer == null)
            {
                return false;
            }

            if (meshRenderer.HasPropertyBlock())
            {
                return false;
            }

            Mesh mesh = meshFilter.sharedMesh;
            Material[] materials = meshRenderer.sharedMaterials;
            int submeshCount = Mathf.Min(mesh.subMeshCount, materials.Length);
            if (submeshCount == 0)
            {
                return false;
            }

            for (int i = 0; i < submeshCount; i++)
            {
                Material material = materials[i];
                if (material != null && material.enableInstancing && material.shader != null && material.shader.isSupported)
                {
                    continue;
                }

                WarnInvalidMaterialOnce(material);
                return false;
            }

            return true;
        }

        internal void UseSourceRendererFallback()
        {
            isUsingSourceRendererFallback = true;
            RestoreSourceRenderer();
        }

        private void Unregister()
        {
            if (!isRegistered)
            {
                return;
            }

            if (GpuInstancingManager.TryGetExisting(out GpuInstancingManager manager))
            {
                manager.Unregister(this);
            }

            isRegistered = false;
        }

        private void RestoreSourceRenderer()
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = sourceRendererWasEnabled;
            }
        }

        private void WarnInvalidMaterialOnce(Material material)
        {
            int materialId = material != null ? material.GetInstanceID() : 0;
            if (!warnedMaterials.Add(materialId))
            {
                return;
            }

            string materialName = material != null ? material.name : "Missing Material";
            DebugUtils.LogWarning($"GPU instancing is unavailable for '{materialName}'. The original MeshRenderer will be used. Enable GPU Instancing and verify shader support.", this);
        }
    }
}
