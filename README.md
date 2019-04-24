<img align="left" src="docs/images/logo-132x164.png"/>

# Welcome to NWheels

[![Build Status](https://nwheels-io.visualstudio.com/nwheels.io/_apis/build/status?definition=1&branchName=master)](https://nwheels-io.visualstudio.com/nwheels.io/_build?definitionId=1)

An ejectable way to develop and maintain software of any scale with tiny amount of code. Every layer and aspect is coded in a dedicated domain-specific language (DSL). The DSLs are transpiled into working codebases in selected languages and technology stacks, which look as if they were coded manually. [Read full introduction](docs/intro.md).

CURRENT STATUS: incubation of **v2**. Field-tested **v1** can be found in [gen-1 branch](https://github.com/nwheels-io/NWheels/tree/gen-1).

# Getting started

WARNING: we're in early alpha, expect failures

## Prerequisites 

### Production



### Development

- Machine running Linux, macOS, or Windows
- .NET Core SDK 2.1 or higher
- (Recommended) C# IDE. The popular ones are VSCode, Visual Studio (Windows only), Rider

## Hello World

Create a minimalistic application that consists of a static web page saying "Hello World".

### Step 2. Create project

1. Create a new class library project:

    ```
    $ mkdir hello-world
    $ cd hello-wolrd
    $ dotnet new classlib -f netstandard 2.0
    $ rm *.cs
    ```

2. Add necessary packages:

    ```
    $ dotnet add package NWheels.Build
    $ dotnet add package NWheels.UI.Model
    $ dotnet add package NWheels.UI.Adapters
    $ dotnet add package NWheels.DevOps.Model
    $ dotnet add package NWheels.DevOps.Adapters
    ```

3. Open the project in an editor or IDE of your choice. For example, to open the project in VSCode, run:

    ```
    $ code .
    ```

### Step 2. Declare intents

1. We want to create a web page that displays "Hello, world"

    `HelloPage.cs`
    
    ```c#
    using NWheels.UI.Model;
    using NWheels.UI.Model.Web;
    
    namespace HelloWorld
    {
        class HelloPage : WebPage
        {
            TextContent Hello => "Hello, world!"
        }
    }
    ```

1. We want the page to be deployed as static HTML:

     `HelloEnvironment.cs`

    ```c#
    using NWheels.DevOps.Model;
    using NWheels.UI.Adapters.Web.StaticHtml;

    namespace HelloWorld
    {
        class HelloEnvironment : Environment<EmptyConfiguration>
        {
            StaticHtmlSite HelloSite => 
                new SinglePageWebApp<HelloPage>().AsStaticHtmlSite();
        }
    }
    ```

### Step 3. Run locally

In the console, run:

```
$ dotnet start
```

If everything goes right, a browser will pop up displaying our web page with the "Hello, world!" text. The console output should state this:

```
Starting local environment: HelloEnvironment
Starting web server for SinglePageWebApp<HelloPage>
- listening at http://localhost:3000
Local environment is up. Press ^C to shut down...
```

# DSLs and technology adapters

Below is the current list of supported DSLs, together with supported technology adapters.

Technology adapters that are links point to their source repos. Those which aren't links are planned, but the work hasn't yet started. The list of adapters will eventually be much larger.

DSL|Supported adapters|Planned next
---|---|---
UI | [web/React+Redux]() | web/Angular ; mobile/ReactNative ; desktop/Electron
REST API | [dotnet/ASP.NET Core]() | node/express
Domain | [dotnet]() | node
Actors | | dotnet/Akka.Net ; jvm/Akka
OxM | [dotnet/MongoDB]() | dotnet/EFCore ; node/mongoose ; node/sequelize
DB | [MongoDB]() | MySQL ; CosmosDB ; CloudStorage | [Elastic]() | TICK ; 
MQ | | Kafka
Distributed Cache | | Redis
Authorization | [dotnet]() | node
Testability | [dotnet/NUnit]() | node/jest
Extensibility | | dotnet ; web ; node
Customization | | dotnet ; web ; node
I18n | | dotnet ; web ; node
Monitoring | [Elastic]() | TICK ; LogzIO ; Azure DevOps ; Google StackDriver
BI | | Zoho Analytics ; Splunk
Deployment | [Docker+Kubernetes]() |
Microservices | [dotnet]() | node
CI/CD | [AppVeyor]() | CodeFresh
