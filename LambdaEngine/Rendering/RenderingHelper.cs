using LambdaEngine.Debug;
using SDL3;

namespace LambdaEngine.Rendering;

internal static unsafe class RenderingHelper {
    private static string basePath;

    public static void InitializeAssetLoader() {
        basePath = SDL.GetBasePath();
    }

    public static IntPtr LoadShader(IntPtr device, string filename, uint samplerCount,
        uint uniformBufferCount, uint storageBufferCount, uint storageTextureCount) {
        SDL.GPUShaderStage stage;
        if (filename.EndsWith(".vert")) {
            stage = SDL.GPUShaderStage.Vertex;
        }
        else if (filename.EndsWith(".frag")) {
            stage = SDL.GPUShaderStage.Fragment;
        }
        else {
            LDebug.Log($"Unable to load shader \"{filename}\": invalid file format.", LogLevel.ERROR);
            return IntPtr.Zero;
        }

        string fullPath;
        SDL.GPUShaderFormat backendFormats = SDL.GetGPUShaderFormats(device);
        SDL.GPUShaderFormat format = SDL.GPUShaderFormat.Invalid;
        string entrypoint;

        if ((backendFormats & SDL.GPUShaderFormat.SPIRV) == SDL.GPUShaderFormat.SPIRV) {
            fullPath = $"{basePath}/Assets/Shaders/Compiled/SPIRV/{filename}.spv";
            format = SDL.GPUShaderFormat.SPIRV;
            entrypoint = "main";
        }
        else if ((backendFormats & SDL.GPUShaderFormat.MSL) == SDL.GPUShaderFormat.MSL) {
            fullPath = $"{basePath}/Assets/Shaders/Compiled/MSL/{filename}.msl";
            format = SDL.GPUShaderFormat.MSL;
            entrypoint = "main0";
        }
        else if ((backendFormats & SDL.GPUShaderFormat.DXIL) == SDL.GPUShaderFormat.DXIL) {
            fullPath = $"{basePath}/Assets/Shaders/Compiled/DXIL/{filename}.dxil";
            format = SDL.GPUShaderFormat.DXIL;
            entrypoint = "main";
        }
        else {
            SDL.Log("Unrecognized backend shader format!");
            return IntPtr.Zero;
        }

        nuint codeSize;
        IntPtr code = SDL.LoadFile(fullPath, out codeSize);
        if (code == IntPtr.Zero) {
            LDebug.Log($"Failed to read shader file \"{fullPath}\": {SDL.GetError()}", LogLevel.ERROR);
            return IntPtr.Zero;
        }

        SDL.GPUShaderCreateInfo shaderInfo = new() {
            Code = code,
            CodeSize = codeSize,
            Entrypoint = entrypoint,
            Format = format,
            Stage = stage,
            NumSamplers = samplerCount,
            NumUniformBuffers = uniformBufferCount,
            NumStorageBuffers = storageBufferCount,
            NumStorageTextures = storageTextureCount
        };

        IntPtr shader = SDL.CreateGPUShader(device, in shaderInfo);
        if (shader == IntPtr.Zero) {
            LDebug.Log($"Failed to create shader \"{fullPath}\": {SDL.GetError()}", LogLevel.ERROR);
            return IntPtr.Zero;
        }

        return shader;
    }

    public static SDL.Surface* LoadImage(string imageFilename, int desiredChannels) {
        SDL.PixelFormat format;

        string fullPath = $"{basePath}/Assets/Images/{imageFilename}";

        IntPtr result = SDL.LoadBMP(fullPath);
        if (result == IntPtr.Zero) {
            SDL.Log($"Failed to load BMP: {SDL.GetError()}");
            return null;
        }

        if (desiredChannels == 4) {
            format = SDL.PixelFormat.ABGR8888;
        }
        else {
            SDL.Log("Unexpected desired channels");
            SDL.DestroySurface(result);
            return null;
        }

        if (((SDL.Surface*)result)->Format != format) {
            IntPtr next = SDL.ConvertSurface(result, format);
            SDL.DestroySurface(result);

            result = next;
        }

        return (SDL.Surface*)result;
    }
}