@echo off
REM Unity 6 DX12 + laptop dual-GPU (RTX 4060 + AMD iGPU) hangs the editor on Play.
REM This launches the same project on Direct3D 11 so the window stays responsive.
start "" "D:\Extra\6000.5.2f1\Editor\Unity.exe" -projectPath "D:\Extra\Monster" -force-d3d11
