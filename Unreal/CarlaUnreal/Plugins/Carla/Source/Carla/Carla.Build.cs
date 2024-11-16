// Copyright 1998-2017 Epic Games, Inc. All Rights Reserved.

using System;
using System.IO;
using UnrealBuildTool;
using System.Diagnostics;
using EpicGames.Core;

public class Carla :
  ModuleRules
{
  [CommandLine("-verbose")]
  bool Verbose = false;

  [CommandLine("-carsim")]
  bool EnableCarSim = false;

  [CommandLine("-chrono")]
  bool EnableChrono = false;

  [CommandLine("-pytorch")]
  bool EnablePytorch = false;

  [CommandLine("-ros2")]
  bool EnableRos2 = false;

  [CommandLine("-ros2-demo")]
  bool EnableRos2Demo = false;

  [CommandLine("-osm2odr")]
  bool EnableOSM2ODR = false;



  public Carla(ReadOnlyTargetRules Target) :
    base(Target)
  {
    PrivatePCHHeaderFile = "Carla.h";
    bEnableExceptions = true;
    bUseRTTI = true;
    
    void AddDynamicLibrary(string library)
    {
      PublicAdditionalLibraries.Add(library);
      RuntimeDependencies.Add(library);
      PublicDelayLoadDLLs.Add(library);
      Console.WriteLine("Dynamic Library Added: " + library);
    }
    
    foreach (var Definition in File.ReadAllText(Path.Combine(PluginDirectory, "Definitions.def")).Split(';'))
      PrivateDefinitions.Add(Definition.Trim());

    foreach (var Option in File.ReadAllText(Path.Combine(PluginDirectory, "Options.def")).Split(';'))
    {
      string Trimmed = Option.Trim();
      switch (Trimmed)
      {
        case "ROS2":
          EnableRos2 = true;
          break;
        case "ROS2_DEMO":
          EnableRos2 = true;
          EnableRos2Demo = true;
          break;
        case "OSM2ODR":
          EnableOSM2ODR = true;
          break;
        default:
          Console.WriteLine($"Unknown option \"{Trimmed}\".");
          break;
      }
    }

    Action<bool, string, string> TestOptionalFeature = (enable, name, definition) =>
    {
      if (enable)
        PrivateDefinitions.Add(definition);
      Console.WriteLine(string.Format("{0} is {1}.", name, enable ? "enabled" : "disabled"));
    };
    
    Action<string> AddIncludeDirectories = (str) =>
    {
      if (str.Length == 0)
        return;
      var paths = str.Split(';');
      if (paths.Length == 0)
        return;
      PublicIncludePaths.AddRange(paths);
    };

    TestOptionalFeature(EnableCarSim, "CarSim support", "WITH_CARSIM");
    TestOptionalFeature(EnableChrono, "Chrono support", "WITH_CHRONO");
    TestOptionalFeature(EnablePytorch, "PyTorch support", "WITH_PYTORCH");
    TestOptionalFeature(EnableOSM2ODR, "OSM2ODR support", "WITH_OSM2ODR");

    PrivateDependencyModuleNames.AddRange(new string[]
    {
      "AIModule",
      "AssetRegistry",
      "CoreUObject",
      "Engine",
      "Foliage",
      "HTTP",
      "StaticMeshDescription",
      "ImageWriteQueue",
      "Json",
      "JsonUtilities",
      "Landscape",
      "Slate",
      "SlateCore",
      "PhysicsCore",
      "Chaos",
      "ChaosVehicles"
    });

    if (EnableCarSim)
    {
      PrivateDependencyModuleNames.Add("CarSim");
      PrivateIncludePathModuleNames.Add("CarSim");
    }

    PublicDependencyModuleNames.AddRange(new string[]
    {
      "Core",
      "RenderCore",
      "RHI",
      "Renderer",
      "ProceduralMeshComponent",
      "MeshDescription"
    });

    if (EnableCarSim)
      PublicDependencyModuleNames.Add("CarSim");

    if (Target.Type == TargetType.Editor)
      PublicDependencyModuleNames.Add("UnrealEd");
      
    PublicIncludePaths.Add(ModuleDirectory);

    foreach (var Path in File.ReadAllText(Path.Combine(PluginDirectory, "Includes.def")).Split(';'))
      if (Path.Length != 0)
        PublicIncludePaths.Add(Path.Trim());

    foreach (var Path in File.ReadAllText(Path.Combine(PluginDirectory, "Libraries.def")).Split(';'))
      if (Path.Length != 0)
        PublicAdditionalLibraries.Add(Path.Trim());

    if (EnableOSM2ODR)
    {
      // @TODO
      PublicAdditionalLibraries.Add("");
    }

    if (EnableChrono)
    {
      // @TODO
      var ChronoLibraryNames = new string[]
      {
        "ChronoEngine",
        "ChronoEngine_vehicle",
        "ChronoModels_vehicle",
        "ChronoModels_robot",
      };
    }

    if (EnableRos2)
    {
      TestOptionalFeature(EnableRos2, "Ros2 support", "WITH_ROS2");
      TestOptionalFeature(EnableRos2Demo, "Ros2 demo", "WITH_ROS2_DEMO");

      string CarlaPluginSourcePath = Path.GetFullPath(ModuleDirectory);
      string CarlaPluginBinariesLinuxPath = Path.Combine(CarlaPluginSourcePath, "..", "..", "Binaries", "Linux");
      AddDynamicLibrary(Path.Combine(CarlaPluginBinariesLinuxPath, "libcarla-ros2-native.so"));
      RuntimeDependencies.Add(Path.Combine(CarlaPluginBinariesLinuxPath, "libfoonathan_memory-0.7.3.so"));
      RuntimeDependencies.Add(Path.Combine(CarlaPluginBinariesLinuxPath, "libfastcdr.so"));
      RuntimeDependencies.Add(Path.Combine(CarlaPluginBinariesLinuxPath, "libfastcdr.so.1"));
      RuntimeDependencies.Add(Path.Combine(CarlaPluginBinariesLinuxPath, "libfastcdr.so.1.1.1"));
      RuntimeDependencies.Add(Path.Combine(CarlaPluginBinariesLinuxPath, "libfastrtps.so"));
      RuntimeDependencies.Add(Path.Combine(CarlaPluginBinariesLinuxPath, "libfastrtps.so.2.11"));
      RuntimeDependencies.Add(Path.Combine(CarlaPluginBinariesLinuxPath, "libfastrtps.so.2.11.2"));
    }

    if (!bEnableExceptions)
    {
      PublicDefinitions.AddRange(new string[]
      {
        "ASIO_NO_EXCEPTIONS",
        "BOOST_NO_EXCEPTIONS",
        "LIBCARLA_NO_EXCEPTIONS",
        "PUGIXML_NO_EXCEPTIONS",
      });
    }

    if (!bUseRTTI)
    {
      PublicDefinitions.AddRange(new string[]
      {
        // "BOOST_DISABLE_ABI_HEADERS", // ?
        "BOOST_NO_RTTI",
        "BOOST_TYPE_INDEX_FORCE_NO_RTTI_COMPATIBILITY",
      });
    }
  }
}
