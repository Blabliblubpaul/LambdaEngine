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

    private RenderCommand[] _renderCommands;
    private int _renderCommandCount;
    
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

        RenderingHelper.InitializeAssetLoader();

        LDebug.Log("RenderSystem - setup complete.");
    }

    public void OnStartup() {
        CreateGpuResources();
        
        _renderCommands = new RenderCommand[2048];
    }
    
    // TODO: Move to asset loading system stage
    private void CreateGpuResources() {
        RenderPipelineManager.Instance.CreateDefaultTexturePipeline(_device, _window);
        ShaderManager.Instance.ReleaseShaders();
        
        CreateSamplers();
        
        MoveTexturesToGpu();
    }

    public void OnExecute() {
        EmitRenderCommands();

        _renderCommands.Sort((a, b) => a.RenderKey.CompareTo(b.RenderKey));
        
        // TODO: create/resize spriteDataBuffer and spriteDataTransferBuffer
        // TODO: better: use a fixed sized buffer and cap batch size.
        
         ProcessRenderCommands();
    }

    private void EmitRenderCommands() {
        QueryCollection<PositionComponent, ScaleComponent, RectPrimitiveComponent, ColorComponent> rectPrimitives =
            _rectPrimitiveQuery.Execute<PositionComponent, ScaleComponent, RectPrimitiveComponent, ColorComponent>();

        QueryCollection<PositionComponent, ScaleComponent, SpriteComponent, ColorComponent> sprites =
            _spriteQuery.Execute<PositionComponent, ScaleComponent, SpriteComponent, ColorComponent>();

        _renderCommandCount = (int)(rectPrimitives.EntityCount + sprites.EntityCount);
        if (_renderCommands.Length < _renderCommandCount) {
            _renderCommands = new RenderCommand[_renderCommandCount];
        }

        int renderCmdIndex = 0;

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

        // Process Sprites
        foreach (ComponentRef<PositionComponent, ScaleComponent, SpriteComponent, ColorComponent>
                     entity in sprites.GetComponents()) {
            // TODO: figure out world/cmaera/etc translations
            Vector2 screenPos = Camera.WorldToScreenSpace(entity.Item0.Position);

            int textureId = entity.Item2.TextureId.AsInt32;
            // TODO: Fix out of bounds bug.
            Vector2 textureSize = new(_textures[textureId].Width, _textures[textureId].Height);
            Vector2 screenSize = textureSize * entity.Item1.Scale * Camera.Zoom;

            RenderKey key = new(entity.Item2.ZIndex, new RenderPipelineId(0), entity.Item2.TextureId, RenderCommandType.SPRITE);
            _renderCommands[renderCmdIndex++] = new(key, screenPos, screenSize, 0, entity.Item3.Color);
        }
    }

    private void ProcessRenderCommands() {
        Matrix4x4 cameraMatrix = Matrix4x4.CreateOrthographicOffCenter(0, WindowManager.WindowWidth, 
             WindowManager.WindowHeight, 0, 0, -1);

        IntPtr cmdBuf = SDL.AcquireGPUCommandBuffer(_device);
        if (cmdBuf == IntPtr.Zero) {
            throw new Exception("Failed to acquire command buffer.");
        }

        if (!SDL.WaitAndAcquireGPUSwapchainTexture(cmdBuf, _window, out IntPtr swapchainTexture, out _, out _)) {
            throw new Exception("Failed to acquire swapchain texture.");
        }

        if (swapchainTexture == IntPtr.Zero) {
            return;
        }
        
        SDL.GPUColorTargetInfo colorTargetInfo = new() {
            Texture = swapchainTexture,
            Cycle = false,
            LoadOp = SDL.GPULoadOp.Clear,
            StoreOp = SDL.GPUStoreOp.Store,
            ClearColor = new SDL.FColor(0, 0, 0, 1)
        };

        int rcIndex = 0;

        RenderKey currentKey = RenderKey.EMPTY;
        while (rcIndex < _renderCommands.Length) {
            currentKey = _renderCommands[rcIndex].RenderKey;
            int start = rcIndex;
            int end = rcIndex;

            int batchSize = 1;

            do {
                end++;
                batchSize++;
            } while (currentKey == _renderCommands[rcIndex++].RenderKey && batchSize < MAX_BATCH_SPRITES);
            
            // TODO: Currently one render pass is used per batch. Better: upload multiple batches before rendering.
            SpriteInstance* dataPtr =
                (SpriteInstance*)SDL.MapGPUTransferBuffer(_device, _spriteDataTransferBuffer, true);
            
            for (int i = start; i <= end; i++) {
                int texture = 0; // TODO: use textureId
                dataPtr[i].Position = _renderCommands[i].Position.AsVector3(); // TODO: use sprite pos
                dataPtr[i].Rotation = _renderCommands[i].Rotation;
                dataPtr[i].W = _renderCommands[i].ScreenSize.X;
                dataPtr[i].H = _renderCommands[i].ScreenSize.Y;
                
                // Use the entire texture.
                dataPtr[i].TexU = 0;
                dataPtr[i].TexV = 0;
                dataPtr[i].TexW = 1;
                dataPtr[i].TexH = 1;
                
                // TODO: Use sprite color
                dataPtr[i].Color = _renderCommands[i].Color.AsFColorRgba();
            }

            SDL.UnmapGPUTransferBuffer(_device, _spriteDataTransferBuffer);

            IntPtr copyPass = SDL.BeginGPUCopyPass(cmdBuf);

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
            
            IntPtr renderPass = SDL.BeginGPURenderPass(cmdBuf, new IntPtr(&colorTargetInfo), 1, IntPtr.Zero);
            SDL.BindGPUGraphicsPipeline(renderPass, _pipeline);
            
            // TODO: If one batch exceeds buffer size, maybe use multiple buffers?
            // Or use multiple buffer for different rendertypes?
            IntPtr[] buffers = new IntPtr[1];
            buffers[0] = _spriteDataBuffer;
            
            SDL.BindGPUVertexStorageBuffers(renderPass, 0, buffers, 1);
            
            // TODO: usee correct batch texture
            SDL.GPUTextureSamplerBinding tsBinding = new() {
                Texture = _textures[currentKey.TextureId.AsInt32].Handle,
                Sampler = _sampler
            };
            
            SDL.BindGPUFragmentSamplers(renderPass, 0, new IntPtr(&tsBinding), 1);
            SDL.PushGPUVertexUniformData(cmdBuf, 0, new IntPtr(&cameraMatrix), (uint)sizeof(Matrix4x4));
            
            // TODO: Use batch sizee
            SDL.DrawGPUPrimitives(renderPass, (uint)(batchSize * 6), 1, 0, 0);

            SDL.EndGPURenderPass(renderPass);
        }

        SDL.SubmitGPUCommandBuffer(cmdBuf);
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
}