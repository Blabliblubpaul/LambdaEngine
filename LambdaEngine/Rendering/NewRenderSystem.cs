using System.Numerics;
using System.Runtime.InteropServices;
using LambdaEngine.Components.Rendering;
using LambdaEngine.Components.Rendering.PrimitiveComponents;
using LambdaEngine.Components.Transform;
using LambdaEngine.Core;
using LambdaEngine.Core.Queries;
using LambdaEngine.Core.Queries.ComponentRef;
using LambdaEngine.Core.Queries.QueryCollection;
using LambdaEngine.Debug;
using LambdaEngine.Interfaces;
using SDL3;

namespace LambdaEngine.Rendering;

// TODO: Add a Camera and World Space
// TODO: Add sprite rendering
// TODO: Add text rendering
// TODO: Add animation systems
// TODO: Add sprite atlases
public unsafe class NewRenderSystem : ISystem {
    public static readonly NewRenderSystem Instance = new();

    private readonly List<IRenderCommandCollector> _commandCollectors = new(8);
    
    private const int MAX_BATCH_VERTICES = 32768;
    private const int MAX_BATCH_SPRITES = 1024;
    private EcsWorld _world;
    
    private IntPtr _window, _device;

    private EcsQuery _rectPrimitiveQuery;
    private EcsQuery _spriteQuery;

    private IntPtr _pipeline;

    private IntPtr _sampler;
    private GpuTexture[] _textures;

    private IntPtr _spriteDataTransferBuffer;
    private IntPtr _spriteDataBuffer;

    private float[] _uCoords = [0.0f, 0.5f, 0.0f, 0.5f];
    private float[] _vCoords = [0.0f, 0.0f, 0.5f, 0.5f];

    private List<RenderCommand> _worldRenderCommands;
    private List<RenderCommand> _transparentRenderCommands;
    private List<RenderCommand> _uiRenderCommands;
    private List<RenderCommand> _debugRenderCommands;
    
    // private RenderCommand[] _renderCommands;
    // private int _renderCommandCount;
    
    private NewRenderSystem() { }

    // TODO: Move to system bootstrap
    internal void InitSdl() {
        _window = WindowManager.WindowHandle;
        
        SetupDeviceAndSwapchain();
        
        ShaderManager.Instance.Init(_device);
    }

    public void OnSetup(LambdaEngine engine, EcsWorld world) {
        _world = world;

        _rectPrimitiveQuery = EcsQuery.Create(_world)
            .Include<PositionComponent>()
            .Include<ScaleComponent>()
            // .Include<RotationComponent>()
            .Include<RectPrimitiveComponent>()
            .Include<ColorComponent>()
            .Build();

        _spriteQuery = EcsQuery.Create(_world)
            .Include<PositionComponent>()
            .Include<ScaleComponent>()
            // .Include<RotationComponent>()
            .Include<SpriteComponent>()
            .Include<ColorComponent>()
            .Build();
        
        foreach (IRenderCommandCollector collector in _commandCollectors) {
            collector.Setup(this, world);
        }

        RenderingHelper.InitializeAssetLoader();

        _worldRenderCommands = new(2048);
        _transparentRenderCommands = new(2048);
        _uiRenderCommands = new(1024);
        _debugRenderCommands = new(1024);

        LDebug.Log("RenderSystem - setup complete.");
    }

    public void OnStartup() {
        CreateGpuResources();
        
        // _renderCommands = new RenderCommand[2048];
    }
    
    // TODO: Move to asset loading system stage
    private void CreateGpuResources() {
        RenderPipelineManager.Instance.CreateDefaultTexturePipeline(_device, _window);
        ShaderManager.Instance.ReleaseShaders();
        
        CreateSamplers();
        
        MoveTexturesToGpu();
        
        CreateSpriteDataBuffers();
    }

    public void OnExecute() {
        EmitRenderCommands();

        // _renderCommands.Sort((a, b) => a.RenderKey.CompareTo(b.RenderKey));
        
        // TODO: create/resize spriteDataBuffer and spriteDataTransferBuffer
        // TODO: better: use a fixed sized buffer and cap batch size.
        
         ProcessRenderCommands();
    }

    public (uint Width, uint height) GetGpuTextureSize(int textureId) {
        return new(_textures[textureId].Width, _textures[textureId].Height);
    }

    public void RegisterRenderCommandCollector(IRenderCommandCollector collector) {
        _commandCollectors.Add(collector);
    }

    public void RegisterRenderCommandCollector<T>() where T : IRenderCommandCollector, new() {
        _commandCollectors.Add(new T());
    }

    public void RegisterRenderCommand(RenderPass pass, RenderCommand cmd) {
        switch (pass) {
            case RenderPass.WORLD:
                _worldRenderCommands.Add(cmd);
                break;
            
            case RenderPass.TRANSPARENT:
                _transparentRenderCommands.Add(cmd);
                break;
            
            case RenderPass.UI:
                _uiRenderCommands.Add(cmd);
                break;
            
            case RenderPass.DEBUG:
                _debugRenderCommands.Add(cmd);
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(pass), pass, null);
        }
    }

    // TODO: Allow the registration of subsystems for registering render commands.
    // TODO: Use List<T> for cmds per pass, prealloctae for ~2048 objects
    private void EmitRenderCommands() {
        _worldRenderCommands.Clear();
        foreach (IRenderCommandCollector collector in _commandCollectors) {
            collector.Execute();
        }

        // TODO: Handle frustum culling

        // TODO: implement rect primitives
        // Process Rect Primitives
        /*
        foreach (ComponentRef<PositionComponent, ScaleComponent, RectPrimitiveComponent, ColorComponent> entity in
                 rectPrimitives.GetComponents()) {
            Vector2 screenPos = Camera.WorldToScreenSpace(entity.Item0.Position);
            Vector2 screenSize = entity.Item2.Size * entity.Item1.Scale * Camera.Zoom;

            SDL.FRect dest = new() {
                X = screenPos.X - screenSize.X * 0.5f,
                Y = screenPos.Y - screenSize.Y * 0.5f,
                W = screenSize.X,
                H = screenSize.Y
            };

            renderCommands.Add(RenderCommand.RectPrimitiveCommand(entity.Item2.ZIndex, dest, entity.Item3.Color));
        }
        */
    }

    private void ProcessRenderCommands() {
        Matrix4x4 cameraMatrix = Matrix4x4.CreateOrthographicOffCenter(0, WindowManager.WindowWidth, 
             WindowManager.WindowHeight, 0, 0, -1);

        IntPtr cmdBuf = SDL.AcquireGPUCommandBuffer(_device);
        if (cmdBuf == IntPtr.Zero) {
            throw new Exception("Failed to acquire GPU command buffer.");
        }

        if (!SDL.WaitAndAcquireGPUSwapchainTexture(cmdBuf, _window, out IntPtr swapchainTexture, out _, out _)) {
            throw new Exception("Failed to acquire GPU swapchain texture.");
        }

        if (swapchainTexture == IntPtr.Zero) {
            return;
        }
        
        SDL.GPUColorTargetInfo clearColorTargetInfo = new() {
            Texture = swapchainTexture,
            Cycle = false,
            LoadOp = SDL.GPULoadOp.Clear,
            StoreOp = SDL.GPUStoreOp.Store,
            ClearColor = new SDL.FColor(0, 0, 0, 1)
        };
        
        SDL.GPUColorTargetInfo colorTargetInfo = new() {
            Texture = swapchainTexture,
            Cycle = false,
            LoadOp = SDL.GPULoadOp.Load,
            StoreOp = SDL.GPUStoreOp.Store
        };
        
        // TODO: Render Passes: World, Transparent (world), UI, Debug

        RenderPassContext worldContext = new(CollectionsMarshal.AsSpan(_worldRenderCommands), clearColorTargetInfo, cameraMatrix, cmdBuf, swapchainTexture);
        RenderPassContext transparentContext = new(CollectionsMarshal.AsSpan(_transparentRenderCommands), colorTargetInfo, cameraMatrix, cmdBuf, swapchainTexture);
        RenderPassContext uiContext = new(CollectionsMarshal.AsSpan(_uiRenderCommands), colorTargetInfo, cameraMatrix, cmdBuf, swapchainTexture);
        RenderPassContext debugContext = new(CollectionsMarshal.AsSpan(_debugRenderCommands), colorTargetInfo, cameraMatrix, cmdBuf, swapchainTexture);
        
        RenderPipelinePass(worldContext);
        
        RenderPipelinePass(transparentContext);
        
        RenderPipelinePass(uiContext);
        
        RenderPipelinePass(debugContext);

        SDL.SubmitGPUCommandBuffer(cmdBuf);
    }

    private void RenderPipelinePass(RenderPassContext context) {
        // TODO: Only do one gpu render pass per logical render pass

        int renderCommandIndex = 0;

        RenderKey currentKey = RenderKey.EMPTY;
        
        // TODO: Assign/(create?) correct pipeline based on renderkey
        _pipeline = RenderPipelineManager.Instance._defaultTexturePipeline;

        // Process all passes
        int cmdCount = _worldRenderCommands.Count;

        Span<RenderCommand> renderCommands = context.renderCommands;
        
        while (renderCommandIndex < cmdCount) {
            currentKey = renderCommands[renderCommandIndex].RenderKey;
            int start = renderCommandIndex++;
            int end = start + 1;

            int batchSize = 1;

            while (renderCommandIndex < cmdCount - 1 && currentKey == renderCommands[renderCommandIndex + 1].RenderKey && batchSize < MAX_BATCH_SPRITES) {
                end++;
                batchSize++;
                renderCommandIndex++;
            }
            
            // TODO: Currently one render pass is used per batch. Better: upload multiple batches before rendering.
            SpriteInstance* dataPtr =
                (SpriteInstance*)SDL.MapGPUTransferBuffer(_device, _spriteDataTransferBuffer, true);

            if (dataPtr == null) {
                throw new Exception($"Failed to map GPU transfer buffer: {SDL.GetError()}");
            }

            int dataIndex = 0;
            for (int i = start; i < end; i++) {
                dataPtr[dataIndex].Position = renderCommands[i].Position.AsVector3(); // TODO: use sprite pos
                dataPtr[dataIndex].Rotation = renderCommands[i].Rotation;
                dataPtr[dataIndex].W = renderCommands[i].ScreenSize.X;
                dataPtr[dataIndex].H = renderCommands[i].ScreenSize.Y;
                
                // Use the entire texture.
                dataPtr[dataIndex].TexU = 0;
                dataPtr[dataIndex].TexV = 0;
                dataPtr[dataIndex].TexW = 1;
                dataPtr[dataIndex].TexH = 1;
                
                // TODO: Use sprite color
                dataPtr[dataIndex].Color = renderCommands[i].Color.AsFColorRgba();
                
                dataIndex++;
            }

            SDL.UnmapGPUTransferBuffer(_device, _spriteDataTransferBuffer);

            IntPtr copyPass = SDL.BeginGPUCopyPass(context.gpuCommandBuffer);

            // TODO: Use offset, according to the todo above.
            SDL.UploadToGPUBuffer(copyPass, new() {
                TransferBuffer = _spriteDataTransferBuffer,
                Offset = 0
            }, new() {
                Buffer = _spriteDataBuffer,
                Offset = 0,
                Size = (uint)(batchSize * sizeof(SpriteInstance))
            }, true);
            
            SDL.EndGPUCopyPass(copyPass);

            SDL.GPUColorTargetInfo colorTargetInfo = context.colorTargetInfo;
            
            IntPtr renderPass = SDL.BeginGPURenderPass(context.gpuCommandBuffer, new IntPtr(&colorTargetInfo), 1, IntPtr.Zero);

            SDL.BindGPUGraphicsPipeline(renderPass, _pipeline);
            
            // TODO: If one batch exceeds buffer size, maybe use multiple buffers?
            // Or use multiple buffer for different rendertypes?
            IntPtr[] buffers = new IntPtr[1];
            buffers[0] = _spriteDataBuffer;
            
            SDL.BindGPUVertexStorageBuffers(renderPass, 0, buffers, 1);
            
            // TODO: use correct batch texture
            SDL.GPUTextureSamplerBinding tsBinding = new() {
                Texture = _textures[currentKey.TextureId.AsInt32].Handle,
                Sampler = _sampler
            };
            
            SDL.BindGPUFragmentSamplers(renderPass, 0, new IntPtr(&tsBinding), 1);
            
            // TODO: Consider pushing the camera data only once per frame
            Matrix4x4 cameraMatrix = context.cameraMatrix;
            SDL.PushGPUVertexUniformData(context.gpuCommandBuffer, 0, new IntPtr(&cameraMatrix), (uint)sizeof(Matrix4x4));
            
            SDL.DrawGPUPrimitives(renderPass, (uint)(batchSize * 6), 1, 0, 0);

            SDL.EndGPURenderPass(renderPass);
        }
    }

    public void OnShutdown() {
        SDL.ReleaseWindowFromGPUDevice(_device, _window);
        SDL.DestroyGPUDevice(_device);
        
        LDebug.Log("RenderSystem - shutdown complete.");
    }

    private void SetupDeviceAndSwapchain() {
        _device = SDL.CreateGPUDevice(SDL.GPUShaderFormat.SPIRV | SDL.GPUShaderFormat.DXIL, true, null);
        if (_device == IntPtr.Zero) {
            throw new Exception("Failed to create GPU device.");
        }

        if (!SDL.ClaimWindowForGPUDevice(_device, _window)) {
            throw new Exception("Failed to claim window.");
        }
        
        SDL.GPUPresentMode presentMode = SDL.GPUPresentMode.VSync;
        if (SDL.WindowSupportsGPUPresentMode(_device, _window, SDL.GPUPresentMode.Immediate)) {
            presentMode = SDL.GPUPresentMode.Immediate;
        }
        else if (SDL.WindowSupportsGPUPresentMode(_device, _window, SDL.GPUPresentMode.Mailbox)) {
            presentMode = SDL.GPUPresentMode.Mailbox;
        }

        SDL.SetGPUSwapchainParameters(_device, _window, SDL.GPUSwapchainComposition.SDR, presentMode);
    }

    // TODO: Create a set of default samplers, allow user defined samplers.
    private void CreateSamplers() {
        _sampler = SDL.CreateGPUSampler(_device, new() {
            MinFilter = SDL.GPUFilter.Nearest,
            MagFilter = SDL.GPUFilter.Nearest,
            MipmapMode = SDL.GPUSamplerMipmapMode.Nearest,
            AddressModeU = SDL.GPUSamplerAddressMode.ClampToEdge,
            AddressModeV = SDL.GPUSamplerAddressMode.ClampToEdge,
            AddressModeW = SDL.GPUSamplerAddressMode.ClampToEdge
        });
    }

    private void MoveTexturesToGpu() {
        TextureManager textureManager = TextureManager.Instance;
        int textureCount = textureManager._textures.Count;

        _textures = new GpuTexture[textureCount];

        ulong requiredBufferSize = 0;
        for (int i = 0; i < textureCount; i++) {
            SDL.Surface* texture = (SDL.Surface*)textureManager._textures[i];
            
            requiredBufferSize += (ulong)(texture->Width * texture->Height * 4);
        }

        // TODO: Do not hardcap texture size
        if (requiredBufferSize > uint.MaxValue) {
            throw new Exception($"Accumulated texture size ({requiredBufferSize}byte) is too large.");
        }
        
        IntPtr textureTransferBuffer = SDL.CreateGPUTransferBuffer(_device, new() {
            Usage = SDL.GPUTransferBufferUsage.Upload,
            Size = (uint)requiredBufferSize
        });

        byte* textureTransferPtr = (byte*)SDL.MapGPUTransferBuffer(_device, textureTransferBuffer, false);
        
        for (int i = 0; i < textureCount; i++) {
            SDL.Surface* texture = (SDL.Surface*)textureManager._textures[i];

            uint textureSize = (uint)(texture->Width * texture->Height * 4);
            NativeMemory.Copy((void*)texture->Pixels, textureTransferPtr, textureSize);
            textureTransferPtr += textureSize;
            
            IntPtr textureHandle = SDL.CreateGPUTexture(_device, new() {
                Type = SDL.GPUTextureType.TextureType2D,
                Format = SDL.GPUTextureFormat.R8G8B8A8Unorm,
                Width = (uint)texture->Width,
                Height = (uint)texture->Height,
                LayerCountOrDepth = 1,
                NumLevels = 1,
                Usage = SDL.GPUTextureUsageFlags.Sampler
            });
            
            _textures[i] = new(textureHandle, (uint)texture->Width, (uint)texture->Height);
        }
        
        SDL.UnmapGPUTransferBuffer(_device, textureTransferBuffer);
        
        IntPtr uploadCmdBuf = SDL.AcquireGPUCommandBuffer(_device);
        IntPtr copyPass = SDL.BeginGPUCopyPass(uploadCmdBuf);

        uint offset = 0;
        for (int i = 0; i < textureCount; i++) {
            SDL.Surface* texture = (SDL.Surface*)textureManager._textures[i];
            
            SDL.UploadToGPUTexture(copyPass, new() {
                TransferBuffer = textureTransferBuffer,
                Offset = offset
            }, new() {
                Texture = _textures[i].Handle,
                W = _textures[i].Width,
                H = _textures[i].Height,
                D = 1
            }, false);

            offset += _textures[i].Width * _textures[i].Height * 4;
        }
        
        SDL.EndGPUCopyPass(copyPass);
        SDL.SubmitGPUCommandBuffer(uploadCmdBuf);
        
        SDL.ReleaseGPUTransferBuffer(_device, textureTransferBuffer);

        textureManager.hadInit = true;
        textureManager.ReleaseTextures();
    }

    private void CreateSpriteDataBuffers() {
        _spriteDataTransferBuffer = SDL.CreateGPUTransferBuffer(_device, new() {
            Usage = SDL.GPUTransferBufferUsage.Upload,
            Size = (uint)sizeof(SpriteInstance) * MAX_BATCH_SPRITES
        });
        
        if (_spriteDataTransferBuffer == IntPtr.Zero) {
            throw new Exception($"Failed to create GPU transfer buffer: {SDL.GetError()}");
        }

        // TODO: Ensure vec3/vec4 are 16byte aligned?
        _spriteDataBuffer = SDL.CreateGPUBuffer(_device, new() {
            Usage = SDL.GPUBufferUsageFlags.GraphicsStorageRead,
            Size = (uint)sizeof(SpriteInstance) * MAX_BATCH_SPRITES
        });
        
        if (_spriteDataBuffer == IntPtr.Zero) {
            throw new Exception($"Failed to create GPU buffer: {SDL.GetError()}");
        }
    }
}