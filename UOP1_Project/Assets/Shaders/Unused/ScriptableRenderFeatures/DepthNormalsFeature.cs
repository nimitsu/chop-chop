// This code is an adaptation of the open-source work by Alexander Ameye
// From a tutorial originally posted here:
// https://alexanderameye.github.io/outlineshader
// Code also available on his Gist account
// https://gist.github.com/AlexanderAmeye

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Profiling;

public class DepthNormalsFeature : ScriptableRendererFeature
{
    class DepthNormalsPass : ScriptableRenderPass
    {
        private const int _depthBufferBits = 32;
        private RTHandle _depthAttachmentHandle;
        private RenderTextureDescriptor _descriptor;

        private Material _depthNormalsMaterial;
        private FilteringSettings _filteringSettings;
        private readonly string _profilerTag = "DepthNormals Prepass";
        private readonly ShaderTagId _shaderTagId = new ShaderTagId("DepthOnly");
        private readonly ProfilingSampler _profilingSampler;

        public DepthNormalsPass(RenderQueueRange renderQueueRange, LayerMask layerMask, Material material)
        {
            _filteringSettings = new FilteringSettings(renderQueueRange, layerMask);
            _depthNormalsMaterial = material;
            _profilingSampler = new ProfilingSampler(_profilerTag);
        }

        public void Setup(RenderTextureDescriptor baseDescriptor)
        {
            baseDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            baseDescriptor.depthBufferBits = _depthBufferBits;
            baseDescriptor.msaaSamples = 1;
            _descriptor = baseDescriptor;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            _descriptor.width = renderingData.cameraData.cameraTargetDescriptor.width;
            _descriptor.height = renderingData.cameraData.cameraTargetDescriptor.height;
            
            RenderingUtils.ReAllocateHandleIfNeeded(ref _depthAttachmentHandle, _descriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "_CameraDepthNormalsTexture");
            
            ConfigureTarget(_depthAttachmentHandle);
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_depthNormalsMaterial == null)
            {
                Debug.LogError("DepthNormals material is null");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, _profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                var drawSettings = CreateDrawingSettings(_shaderTagId, ref renderingData, sortFlags);
                drawSettings.perObjectData = PerObjectData.None;
                drawSettings.overrideMaterial = _depthNormalsMaterial;

                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref _filteringSettings);

                cmd.SetGlobalTexture("_CameraDepthNormalsTexture", _depthAttachmentHandle);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.SetGlobalTexture("_CameraDepthNormalsTexture", Texture2D.blackTexture);
        }

        public void Dispose()
        {
            _depthAttachmentHandle?.Release();
        }
    }

    private DepthNormalsPass _depthNormalsPass;
    private Material _depthNormalsMaterial;

    public override void Create()
    {
        _depthNormalsMaterial = CoreUtils.CreateEngineMaterial("Hidden/Internal-DepthNormalsTexture");
        _depthNormalsPass = new DepthNormalsPass(RenderQueueRange.opaque, -1, _depthNormalsMaterial);
        _depthNormalsPass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_depthNormalsMaterial == null)
        {
            Debug.LogError("DepthNormals material is missing");
            return;
        }

        _depthNormalsPass.Setup(renderingData.cameraData.cameraTargetDescriptor);
        renderer.EnqueuePass(_depthNormalsPass);
    }

    protected override void Dispose(bool disposing)
    {
        _depthNormalsPass?.Dispose();
        CoreUtils.Destroy(_depthNormalsMaterial);
    }
}

