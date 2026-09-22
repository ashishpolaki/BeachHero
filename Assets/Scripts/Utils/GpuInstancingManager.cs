using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BeachHero
{
    [DefaultExecutionOrder(10000)]
    public sealed class GpuInstancingManager : MonoBehaviour
    {
        // Matrix4x4 instance data also includes a world-to-object matrix by default,
        // which limits RenderMeshInstanced to 511 instances per call.
        private const int MaxInstancesPerDraw = 511;

        private static GpuInstancingManager instance;
        private static bool isQuitting;

        private readonly Dictionary<BatchKey, InstanceBatch> batches = new();
        private readonly Dictionary<GpuInstancedVisual, List<BatchKey>> registrations = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            isQuitting = false;
        }

        public static bool TryGetOrCreate(out GpuInstancingManager manager)
        {
            manager = instance;
            if (manager != null)
            {
                return true;
            }

            if (isQuitting || !SystemInfo.supportsInstancing)
            {
                return false;
            }

            GameObject managerObject = new GameObject(nameof(GpuInstancingManager));
            DontDestroyOnLoad(managerObject);
            instance = managerObject.AddComponent<GpuInstancingManager>();
            manager = instance;
            return true;
        }

        public static bool TryGetExisting(out GpuInstancingManager manager)
        {
            manager = instance;
            return manager != null;
        }

        public bool Register(GpuInstancedVisual visual, Mesh mesh, MeshRenderer sourceRenderer)
        {
            if (visual == null || mesh == null || sourceRenderer == null || registrations.ContainsKey(visual))
            {
                return false;
            }

            Material[] materials = sourceRenderer.sharedMaterials;
            int submeshCount = Mathf.Min(mesh.subMeshCount, materials.Length);
            if (submeshCount == 0)
            {
                return false;
            }

            for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
            {
                Material material = materials[submeshIndex];
                if (material == null || !material.enableInstancing || material.shader == null || !material.shader.isSupported)
                {
                    return false;
                }
            }

            List<BatchKey> visualBatches = new List<BatchKey>(submeshCount);
            for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
            {
                BatchKey key = new BatchKey(mesh, materials[submeshIndex], submeshIndex, sourceRenderer);
                if (!batches.TryGetValue(key, out InstanceBatch batch))
                {
                    batch = new InstanceBatch(key, sourceRenderer);
                    batches.Add(key, batch);
                }

                batch.Add(visual);
                visualBatches.Add(key);
            }

            registrations.Add(visual, visualBatches);
            return true;
        }

        public void Unregister(GpuInstancedVisual visual)
        {
            if (visual == null || !registrations.TryGetValue(visual, out List<BatchKey> visualBatches))
            {
                return;
            }

            for (int i = 0; i < visualBatches.Count; i++)
            {
                BatchKey key = visualBatches[i];
                if (!batches.TryGetValue(key, out InstanceBatch batch))
                {
                    continue;
                }

                batch.Remove(visual);
                if (batch.Count == 0)
                {
                    batches.Remove(key);
                }
            }

            registrations.Remove(visual);
        }

        private void LateUpdate()
        {
            foreach (InstanceBatch batch in batches.Values)
            {
                batch.Render();
            }
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private readonly struct BatchKey : IEquatable<BatchKey>
        {
            public readonly Mesh Mesh;
            public readonly Material Material;
            public readonly int SubmeshIndex;
            public readonly int Layer;
            public readonly ShadowCastingMode ShadowCastingMode;
            public readonly bool ReceiveShadows;
            public readonly LightProbeUsage LightProbeUsage;
            public readonly ReflectionProbeUsage ReflectionProbeUsage;
            public readonly uint RenderingLayerMask;
            public readonly int RendererPriority;
            public readonly MotionVectorGenerationMode MotionVectorMode;

            public BatchKey(Mesh mesh, Material material, int submeshIndex, MeshRenderer renderer)
            {
                Mesh = mesh;
                Material = material;
                SubmeshIndex = submeshIndex;
                Layer = renderer.gameObject.layer;
                ShadowCastingMode = renderer.shadowCastingMode;
                ReceiveShadows = renderer.receiveShadows;
                LightProbeUsage = renderer.lightProbeUsage;
                ReflectionProbeUsage = renderer.reflectionProbeUsage;
                RenderingLayerMask = renderer.renderingLayerMask;
                RendererPriority = renderer.rendererPriority;
                MotionVectorMode = renderer.motionVectorGenerationMode;
            }

            public bool Equals(BatchKey other)
            {
                return Mesh == other.Mesh &&
                       Material == other.Material &&
                       SubmeshIndex == other.SubmeshIndex &&
                       Layer == other.Layer &&
                       ShadowCastingMode == other.ShadowCastingMode &&
                       ReceiveShadows == other.ReceiveShadows &&
                       LightProbeUsage == other.LightProbeUsage &&
                       ReflectionProbeUsage == other.ReflectionProbeUsage &&
                       RenderingLayerMask == other.RenderingLayerMask &&
                       RendererPriority == other.RendererPriority &&
                       MotionVectorMode == other.MotionVectorMode;
            }

            public override bool Equals(object obj)
            {
                return obj is BatchKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = Mesh != null ? Mesh.GetInstanceID() : 0;
                    hash = (hash * 397) ^ (Material != null ? Material.GetInstanceID() : 0);
                    hash = (hash * 397) ^ SubmeshIndex;
                    hash = (hash * 397) ^ Layer;
                    hash = (hash * 397) ^ (int)ShadowCastingMode;
                    hash = (hash * 397) ^ ReceiveShadows.GetHashCode();
                    hash = (hash * 397) ^ (int)LightProbeUsage;
                    hash = (hash * 397) ^ (int)ReflectionProbeUsage;
                    hash = (hash * 397) ^ RenderingLayerMask.GetHashCode();
                    hash = (hash * 397) ^ RendererPriority;
                    hash = (hash * 397) ^ (int)MotionVectorMode;
                    return hash;
                }
            }
        }

        private sealed class InstanceBatch
        {
            private readonly BatchKey key;
            private RenderParams renderParams;
            private readonly List<GpuInstancedVisual> visuals = new();
            private readonly Matrix4x4[] matrices = new Matrix4x4[MaxInstancesPerDraw];
            private bool hasFailed;

            public int Count => visuals.Count;

            public InstanceBatch(BatchKey key, MeshRenderer sourceRenderer)
            {
                this.key = key;
                renderParams = new RenderParams(key.Material)
                {
                    layer = key.Layer,
                    renderingLayerMask = key.RenderingLayerMask,
                    shadowCastingMode = key.ShadowCastingMode,
                    receiveShadows = key.ReceiveShadows,
                    lightProbeUsage = key.LightProbeUsage,
                    reflectionProbeUsage = key.ReflectionProbeUsage,
                    rendererPriority = key.RendererPriority,
                    motionVectorMode = key.MotionVectorMode
                };
            }

            public void Add(GpuInstancedVisual visual)
            {
                visuals.Add(visual);
            }

            public void Remove(GpuInstancedVisual visual)
            {
                int index = visuals.IndexOf(visual);
                if (index < 0)
                {
                    return;
                }

                int lastIndex = visuals.Count - 1;
                visuals[index] = visuals[lastIndex];
                visuals.RemoveAt(lastIndex);
            }

            public void Render()
            {
                if (hasFailed)
                {
                    return;
                }

                int sourceIndex = 0;
                try
                {
                    while (sourceIndex < visuals.Count)
                    {
                        int instanceCount = 0;
                        while (sourceIndex < visuals.Count && instanceCount < MaxInstancesPerDraw)
                        {
                            GpuInstancedVisual visual = visuals[sourceIndex++];
                            if (visual == null || !visual.ShouldRenderInstanced)
                            {
                                continue;
                            }

                            matrices[instanceCount++] = visual.LocalToWorldMatrix;
                        }

                        if (instanceCount > 0)
                        {
                            Graphics.RenderMeshInstanced(renderParams, key.Mesh, key.SubmeshIndex, matrices, instanceCount);
                        }
                    }
                }
                catch (InvalidOperationException exception)
                {
                    hasFailed = true;
                    for (int i = 0; i < visuals.Count; i++)
                    {
                        visuals[i].UseSourceRendererFallback();
                    }

                    DebugUtils.LogError($"GPU instancing failed for '{key.Material.name}'. Falling back to MeshRenderer.\n{exception.Message}", key.Material);
                }
            }
        }
    }
}
