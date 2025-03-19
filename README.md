# ![Logo](LibAtorch/logo_small.png) LibAtorch

## Introduction

Provides a simple way to control Atorch loads over (USB virtual) Serial ports. Available as [NuGet package](https://www.nuget.org/packages/CFNReader).

![Build Status](https://img.shields.io/github/actions/workflow/status/RobThree/LibAtorch/test.yml?branch=master&style=flat-square) [![Nuget version](https://img.shields.io/nuget/v/LibAtorch.svg?style=flat-square)](https://www.nuget.org/packages/LibAtorch/)

## Devices

This project should work with a bunch of Atorch loads. I've only tested it with the DL24-P. Please let me know if it works with your device as well.

## QuickStart

```c#
var load = new AtorchDevice(new SerialOptions { PortName = "COM4" });
await load.Open();
await load.SetCurrent(1.0);
await load.SetLoad(LoadState.On);
var current = await load.ReadCurrent();
await load.SetLoad(LoadState.Off);
load.Close();
```

This library is async whenever possible. Because the .Net serial port implementation leaves things to be desired communicating with the Atorch loads can be a bit flakey. Especially since the devices insist on keep sending a 'report' at some fixed interval which may interfere with requests/response being handled at the time.

This project isn't really ready for production use yet, but it's a start. Help is very much welcome. Which brings me to:

## Contributing

Yes please! I'm sure there are many things that can be improved. Please fork, make changes and submit a pull request. I'll be happy to review and merge them.