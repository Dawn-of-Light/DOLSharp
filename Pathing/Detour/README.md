Pathing with Detour
===================

Recast is accompanied by Detour, a path-finding and spatial reasoning toolkit. You can use any navigation mesh with Detour, but of course the data generated with Recast fits perfectly.

Detour offers a simple static navmesh data representation which is suitable for many simple cases. It also provides a tiled navigation mesh representation, which allows you to stream of navigation data in and out as the player progresses through the world and regenerate sections of the navmesh data as the world changes.

This project is based on [recastnavigation](https://github.com/recastnavigation/recastnavigation).

## How to use with DOL
You need to generate navmeshes (navigation meshes) with [Uthgard open source tools](https://github.com/thekroko/uthgard-opensource/tree/master/pathing) or download them from [Amtenael](https://amtenael.fr/pathing.7z) (some zones are missing).

You can create a "pathing" folder in your DOL folder (where you have DOLServer.exe) and copy the navmeshes.

Caution: if you use all navmeshes, you will need at least 5GB of RAM and DOL will be take some time to load.

## Build (Windows)
This guide will use Visual Studio 2022.

You will need to build the dll for that, you need some tools:
- Install [Visual Studio with "Desktop development with C++"](https://visualstudio.microsoft.com/downloads/)
- Install [CMake](https://cmake.org/download/)

About Linux, you can use your package manager to install theses tools: cmake, g++ or clang++ and gmake (often included in a "build-essentials" package).

1. Open CMake (cmake-gui)
2. In "Where is the source code", put the path for this folder (example: `C:/dev/DOLSharp/Pathing/Detour`)
3. In "Where to build the binaries", you can copy the source code path and add `/build` at the end
4. Click on "Configure"
  .. Accept to create the build folder
  .. Select "Visual Studio 17 2022" as generator
  .. Click on "Finish"
5. Click on "Generate" and "Open Project"
6. Visual Studio should open with DOL_Detour
7. Select "Release" instead of "Debug" and build the solution
8. You can copy dol_detour.dll from `Release` in your build folder to your DOL folder

## Build (Linux)
- Debian / Ubuntu: `sudo apt-get install build-essential cmake`
- Archlinux: `sudo pacman -Sy base-devel cmake`

1. Open a terminal in this path
2. `mkdir build && cd build`
3. `cmake -DCMAKE_BUILD_TYPE=Release .. && make`
4. Copy `libdol_detour.so` in your DOL folder
