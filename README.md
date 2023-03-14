![Logo](./docs/assets/logo_small.png)

# SporeMaestro

Everything one could need for fungal spore microscopy  

## [View the Diagram](https://bradmartin333.github.io/SporeMaestro/)

- [ ] Automatic spore detection and measurements
- [ ] Identification database integration
- [ ] Clean data summary exporting
- [ ] Continuous data logging
- [ ] Calibration to USAF Target
- [ ] Image capture and focus stacking
- [ ] Motion triggered capture
- [ ] Autofocus tooling
- [ ] Support for external GPIO (Lighting, electronics, triggers)

## Why make this?

Fungi indentification, microscopy and free software go really well together.  
I believe that making this tool will enable people to delve into these topics with ease
and help grow the already flourishing community.
I also want to experiment with my own electronics dependent fungal spore research, which is why there is GPIO support.

## How is it made?

- [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [Raylib](https://www.raylib.com/)
- [ImageSharp](https://sixlabors.com/products/imagesharp/)
- [FlashCap](https://github.com/kekyo/FlashCap) 
- [Serilog](https://serilog.net/)

### Extra things used here

- [Icons](https://iconmonstr.com/)
- [Recursive Diagram](https://github.com/mitxela/recursive)

## Preliminary Todos
- [x] copy over old projects
- [x] handle console input
- [x] update image in window
- [x] apply processors to image
- [x] implement USB2
- [x] implement FLIR
- [ ] implement MJPEG
- [ ] camera type / camera index selector
- [ ] tool panel with buttons for testing