
# Xamarin Templates

This repo holds all the Xamarin project and item templates for Visual Studio and Visual Studio for Mac. These templates are designed to be used with the [.NET Core Template Engine](https://github.com/dotnet/templating).

**[List of available templates](https://github.com/xamarin/xamarin-templates/wiki/Current-Templates)**

## Getting Started

### Requirements
- NET Core 2.0 SDK
- Android API 25+ SDK

### Creating a new template
The easiest way to get started is to use Visual Studio to create a new project of the template you want to port, then do the following:
1. Create a `.template.config` folder in the project's root directory and add a `template.json` file
2. Edit the `template.json` to configure the template. See the [wiki](../../wiki) for common symbols and identifiers to use.
3. Run `install.sh` or `install.ps1` to install the template and test it


## Quick Links
[Template Engine Wiki](https://github.com/dotnet/templating/wiki)

[Template Samples](https://github.com/dotnet/dotnet-template-samples)

[How to create your own templates for dotnet new](https://blogs.msdn.microsoft.com/dotnet/2017/04/02/how-to-create-your-own-templates-for-dotnet-new/)


