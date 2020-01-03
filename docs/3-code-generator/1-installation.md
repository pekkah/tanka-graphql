## Code Generator

Writing code for the resolvers can get tedious when the application
gets more complicated. Code generator provides features for automatically
generating some of the the resolvers and providing strongly typed stubs when
generation is not possible.


### How it works

Code generator takes as an input a graphql schema definition language file (SDL)
and generates resolvers and models from it.

Code generator includes GraphQL MSBuild item type to connect the generator to SDL
files in your project. The actual generator is an dotnet tool which gets executed
by the MSBuild targets or it can be run manually when required.


### Install 

Code generator is distributed as NuGet packages:

- `tanka.graphql.generator` package contains the MSBuild targets and custom tasks,
- `tanka.graphql.generator.tool` package is the dotnet tool which handles the actual code
generation.


1. Install the tool

#### Locally

```bash
# new dotnet tool manifest
dotnet new tool-manifest

dotnet tool install tanka.graphql.generator.tool
```

#### Globally

```bash
dotnet tool install --global tanka.graphql.generator.tool
```

2. Install MSBuild tasks

```bash
dotnet add package tanka.graphql.generator
```


3. Add SDL file

Add SDL file to project and include it as
and GraphQL SDL file.

```xml
<ItemGroup>
    <GraphQL Include="Folder\SDL.graphql" />
</ItemGroup>
```

4. Generate code

Code is generated automatically every time the SDL files are changes. 
Rebuilding project will force the renegeration to be executed.
Generated code files are located in IntermediateOutput directory. They're 
automatically included in the compilation of the project and 
Visual Studio IntelliSense will work with them. 


