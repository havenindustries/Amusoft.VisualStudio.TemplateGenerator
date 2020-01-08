# Visual Studio Multi project template generator

## CI

[![Build status](https://ci.appveyor.com/api/projects/status/e9147nfvdyl9d139?svg=true)](https://ci.appveyor.com/project/taori/amusoft-visualstudio-templategenerator)

## Download Release

https://github.com/taori/Amusoft.VisualStudio.TemplateGenerator/releases/latest

## Features

- Build multi project template from a raw visual studio solution
    - No need to export anything from within VS
- Multi project template generation
- Configure once, rebuild approach
    - No more updating and manually editing files over and over required
- Multiple output folders
    - Add the import folders to create your template and test the generation result
    - Drop the packed zip file straight into your extension without any effort
- Solution folder configuration
    - If your template is more complex, you can add template folders straight from the get go.
- Primary project configuration
    - Set up startup project for a template
- OpenInEditor configuration
    - Mark the files you want to be opened after a template was scaffolded and is being opened

## Entry screen

<img src="https://user-images.githubusercontent.com/5545184/50557093-0060d900-0ce2-11e9-8bf8-1b8076816853.png"/>

## Template configuration

<img src="https://user-images.githubusercontent.com/5545184/50557135-5d5c8f00-0ce2-11e9-9397-8808c04bdcdd.png"/>

## CLI support

you can also execute this program from the command line for the purpose of build automation.

```build configuration [configurationId]```
is the command for that.

## IMPORTANT NOTES

When you first run it and select a solution you most likely will have to save, go back to the list, 
and then select the solutiion again.  NOTE: this has been intentionally limited to pretty standard 
multi-project solutions.  If you have an unusual solution structure please go back to the original
version of this code (where I forked from).

## RECOMMENDATIONS

1) Clean your bin and obj folders for the solution you want to turn into a template
2) You can use the attached as a reference on how to get started
3) This is not a perfect solution but you can always tweak it to suit your needs

![Modified Example](configexample.png?raw=true "Xamarin Config Example")
