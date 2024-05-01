# Nirvana

## Table of Contents
- [Nirvana](#nirvana-engine)
- [Features](#features)
- [Controls](#controls)
- [Compilation and Running](#compilation-and-running)
- [Dependencies](#dependencies)
- [Dependency Installation](#dependency-installation)
- [License](#license)

## Nirvana Engine
Nirvana is a 3D game engine written using the C++ programming language
with OpenGL used as the graphics API

## Features
* 3D Animation
* 2D Textures
* 3D Camera
* Animated Models
* Audio Engine
* Font Renderer
* Transparency
* 2D and 3D Particle Systems
* Shadows
* Terrain
* Skybox
* 3D Object Materials
* 3D Water
* 3D Mouse Picker
* Minimap
* Chat Log

## Controls
* Arrow Keys - Rotate camera
* Left Click - Move character
* Right Click - Drag and drop in inventory
* Mouse Scroll Wheel - Zoom in/out
* F12 - Enter Fullscreen
* ESC - Exit Fullscreen

## Compilation and Running
* change directory to the 'src' folder of the project, then running the following commands:

```sh
make
./nirvana
```

* IMPORTANT: You will need to add a resource folder for
the engine to find the resources used in Main.
You can also create your own main.cpp and add your own resources.
Use the provided main.cpp to see how it is structured

## Dependencies
* GLFW

## Dependency Installation
* Debian Linux Distributions (e.g. Ubuntu):
	- install GLFW using commands: 

```sh
sudo apt install libgl-dev
sudo apt install libglfw3-dev
```

## License
Eliseo Copyright 2023
<br>
Code released under the [MIT License](LICENSE)
