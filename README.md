# ⚡ Avalonia-NativeAOT-SingleFile

![image](https://github.com/user-attachments/assets/d8c5bb48-f9e0-4048-8f37-330f985fae30)

A comprehensive guide and reference implementation for publishing
[Avalonia UI](https://avaloniaui.net/) applications as **Native AOT** single-file
executables.

This repository demonstrates how to:

- Configure an Avalonia app for Native AOT compilation using `Directory.Build.props`
- Produce **truly self-contained static link** executables that run without the .NET runtime
- Minimize the final binary size (down to ~15 MB with UPX)
- Overcome common Avalonia-specific AOT pitfalls (XAML, reflection, native
  dependencies, source generator)

> 💡 **New to Native AOT?**
> Start with the **[Minimal Sample](#-minimal-sample)** below —
> it strips everything down to the bare essentials
> so you can understand the core AOT configuration in minutes.

## 🚀 Minimal Sample

The **[`MinimalAotSample`](MinimalAotSample/)** directory contains
a deliberately minimal Avalonia application created from the **official App
template**.

It uses only two things:

- The standard **Avalonia App template**
- A **`Directory.Build.props`** file to control Native AOT publishing

No source generators, no advanced features — just the cleanest possible
demonstration of getting from zero to a fully native, self-contained executable.

**The real power of this sample lies in its approach:** you add a single
`Directory.Build.props` file, and your entire project gains Native AOT
capabilities **without touching a single line of application code**. This is
what we mean by *non-invasive* – your Debug development workflow remains
completely unchanged, and AOT kicks in only when you explicitly publish for
Release.

### What the Minimal Sample shows

| Concept               | File                                     | What you learn                                  |
|-----------------------|------------------------------------------|-------------------------------------------------|
| Project AOT settings  | `MinimalAotSample/Directory.Build.props` | How to enable `PublishAot` for projects at once |
| Project configuration | `MinimalAotSample.csproj`                | Do not need to change anything                  |

### Quick start — publish the minimal sample

```bash
# 1. Clone the repository
git clone https://github.com/peaceshi/Avalonia-NativeAOT-SingleFile.git
cd Avalonia-NativeAOT-SingleFile

# 2. Publish for Windows x64 (Native AOT single file)
dotnet publish MinimalAotSample/MinimalAotSample.csproj -r win-x64 -c Release 

# 3. Run the output
MinimalAotSample.exe
```

## 🔧 Why Directory.Build.props?

### Simplicity & Non-Invasiveness

The entire AOT configuration in this repository is driven by a single file at
the root: Directory.Build.props. This approach is deliberately chosen for its
simplicity and non-invasiveness:

- ✅ One file, all projects – No need to sprinkle AOT settings into every
.csproj. Just drop the Directory.Build.props at the solution root and
every project inherits the configuration automatically.

- ✅ Zero code changes – Your application source code, project structure,
and everyday Debug builds remain completely untouched. Native AOT is only
activated when you explicitly publish with -c Release.

- ✅ Easy to adopt, easy to remove – Adding AOT support is as simple as
placing the file in your repo. To go back, just delete it. No messy rollback
of individual project files.

- ✅ Hierarchical control – You can place additional
Directory.Build.props in sub-folders to fine-tune settings for specific
projects without breaking the global defaults. The minimal sample shows
exactly how this works.

- ✅ Beginner-friendly – Even if you are new to Avalonia or .NET Native AOT,
you don’t need to learn complex MSBuild internals. The Directory.Build.props
file reads almost like a checklist of required settings, making it a
self-documenting reference.

In short, this method gives you Native AOT as a transparent layer on top of
your application. Your app stays clean, your development loop stays fast, and
your release pipeline becomes a one-liner.

## 📚 References

- [Avalonia Native AOT Deployment](https://docs.avaloniaui.net/docs/deployment/native-aot) — Official Avalonia
documentation

- [.NET Native AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/) — Microsoft's comprehensive guide

- [Trimming and reflection](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/prepare-libraries-for-trimming) —
How to handle reflection in trimmed/AOT apps

- [Avalonia Samples](https://github.com/AvaloniaUI/Avalonia.Samples) — Official sample applications, including Native AOT
examples

## 📦 Upstream static libraries

This project relies on two upstream repositories that provide **statically
linked native libraries** for Avalonia UI. These libraries eliminate the need
to ship separate native DLLs alongside your single-file executable.

| Repository                                                         | Description                                                                                                                                                                                                                                        |
|--------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **[SkiaSharp.Static](https://github.com/2ndlab/SkiaSharp.Static)** | A statically linked build of **SkiaSharp** for Avalonia UI. Replaces the default dynamically linked SkiaSharp so that the entire rendering engine can be embedded into your native binary.                                                         |
| **[ANGLE.Static](https://github.com/2ndlab/ANGLE.Static)**         | A statically linked build of **ANGLE** for Avalonia UI. ANGLE translates OpenGL ES calls to native platform APIs (Direct3D on Windows, Metal on macOS, Vulkan on Linux), and the static version allows it to be fully linked into your executable. |

> 💡 Both repositories reference **this project** as their sample
> implementation.  
> Together, they form the foundation that makes true
> single-file Native AOT publishing possible for Avalonia applications —  
> no external runtime, no scattered DLLs, just one self-contained binary.

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.txt) file
for details.
