echo compiling shaders...
set "ShaderOutput=..\Shaders\ShaderCache"

if not exist "%ShaderOutput%" mkdir "%ShaderOutput%"

C:\VulkanSDK\1.4.341.1\Bin\glslc.exe ..\Shaders\simple_shader.vert -o "%ShaderOutput%\simple_shader.vert.spv"
C:\VulkanSDK\1.4.341.1\Bin\glslc.exe ..\Shaders\simple_shader.frag -o "%ShaderOutput%\simple_shader.frag.spv"

echo shaders compiled successfully