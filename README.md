Linux|Windows|Coverage
-----|-------|--------
TBD|[![Build status](https://ci.appveyor.com/api/projects/status/qcm23727dm8dplk5/branch/master?svg=true)](https://ci.appveyor.com/project/felix-b/nwheels-bk3vs/branch/master)|[![Coverage Status](https://coveralls.io/repos/github/felix-b/NWheels/badge.svg?branch=master)](https://coveralls.io/github/felix-b/NWheels?branch=master)

Welcome to NWheels
=======

Based on our experience, commonality in the needs of enterprise application projects is significantly higher than variability. 

We take this as an opportunity to build a community-based ecosystem, which creates A-to-Z architectural recipes, ready technology stacks, concise programming models, and building blocks for common problem domains. 

We put those pieces together to turn enterprise application development into an easy win

### How it works

_DISCLAIMER: we're in the middle of development. Some features listed below may not yet exist, or be unstable_. 

You:|NWheels:
---|-------
Design your application as a set of microservices|Packages and deploys microservice containers to runtime environments. Handles scalability and fault tolerance independently of your cloud vendor.
Code and annotate business domains of the application, abstracted from concrete technology stacks.|Implements [hexagonal architecture](http://alistair.cockburn.us/Hexagonal+architecture) with _convention-over-implementation_ approach. Eliminates mechanical and repetitive coding of whole layers, replacing it with pluggable code generation. Examples: data access, serialization, network communication, RESTful APIs, GraphQL queries, etc.
Code and annotate conceptual UI models in C#, abstracted from concrete technology stacks. Use numerous UI themes. Directly tweak UI code and assets wherever unique touch is necessary.|Generates UI applications for target interaction platforms, including web, mobile, desktop, IVR, SmartTV, and IoT. Transparently handles UI model bindings to data and business capabilities, including reflection and enforcement of authorization requirements. 
Declare cross-cutting requirements like authorization and event logging, through concise C# programming models|Transparently implements and enforces the requirements throughout all execution paths. For instance, event logging includes such advanced features as BI measurements, usage statistics, circuit breakers, and built-in cost-free performance profiling.  
Pick technology stack for each microservice|Generates integration layers of domain objects with selected technology stacks. Generates concrete implementations of declarative models. Certain technology stacks automatically enable advanced distribution scenarios, such as elastic scalability and actor grids. 
Compose the product out of pluggable features. Use features for both core product and multiple customization layers. In the features, extend or override all aspects of system presentation and behavior.|Allows flexible vertical and horizontal composition of domain objects and user interfaces. Releases product features and customizations as pluggable NuGet packages into your project NuGet repo. Smoothly supports distributed development workflows and remote professional services outside of product vendor organization. 
When coding business domains and UI, reuse ready domain building block modules supplied by NWheels, and avoid reinventing the wheel.|Captures expertise in common problem domains (e.g. e-commerce, booking, CRM) into reusable _domain building block_ modules, based on well-established and field-proven designs. Makes building blocks inheriteble, extensible, and easily adjustable to specific application requirements.  

# Demo

Imagine a very simple application:
- A single page web app, which lets user enter her name, and submit it with a button. 
- A microservice, which handles the submission. The microservice exposes RESTful API invoked by the web app button. 
- Business logic (_transaction script_), which receives user's name, and responds with a greeting text. The greeting text is then displayed in the web app.

NWheels-based implementation is below 50 lines of C# code, all layers included.

## Running the demo 

### System requirements

- Running on your machine:
  - Linux, Windows, or macOS machine 
  - .NET Core SDK 1.1 or later ([download here](https://www.microsoft.com/net/download/core))

- Running in Docker (Linux container):
  ```bash
  $ docker run --name nwheels-demo -p 5000:5000 -it microsoft/dotnet:1.1-sdk /bin/bash
  ```

### Get sources and build

  ```bash
  $ git clone https://github.com/felix-b/NWheels.git nwheels
  $ cd nwheels/Source/
  $ dotnet restore
  $ dotnet build
  ```

### Run microservice

  ```bash
  $ dotnet NWheels.Samples.FirstHappyPath.HelloService/bin/Debug/netcoreapp1.1/hello.dll
  ```
  
### Open web application

- If running on your machine: 
  - Browse to [http://localhost:5000](http://localhost:5000)
- If running in docker container: 
  - Print container IP address:
    ```bash
    docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' nwheels-demo
    ```
  - Browse to http://_container_ip_address_:5000
 
## Source code explained

#### Program.cs - microservice entry point

It is super simple to bootstrap a microservice. Most of the time, you're all set with the defaults. For advanced scenarios, extensible API of `MicroserviceHostBuilder` lets you tailor technology stack to your requirements. 

```csharp
public static int Main(string[] args)
{
    var microservice = new MicroserviceHostBuilder("hello")
        .AutoDiscoverComponents()
        .UseDefaultWebStack(listenPortNumber: 5000)
        .Build();

    return microservice.Run(args);
}
```

#### HelloWorldTx.cs - business logic

Business logic for this demo is trivial. It is captured in a _transaction script component_ class. 

```csharp
[TransactionScriptComponent]
[SecurityCheck.AllowAnonymous]
public class HelloWorldTx
{
    [TransactionScriptMethod]
    public async Task<string> Hello(string name)
    {
        return $"Hello world, from {name}!";
    }
}
```

There's more under the hood, though. For instance, default web stack includes RESTful API endpoint, where transaction scripts are one type of supported resources. The endpoint transparently allows invocation of resources through HTTP and other protocols, subject to authorization requirements.

Here, `Hello` method can be invoked through HTTP request:

```HTTP
POST http://localhost:5000/tx/HelloWorld/Hello HTTP/1.1
User-Agent: Fiddler
Host: localhost:5000
Content-Length: 17

{"name": "NWheels"}
```
The endpoint will reply as follows:

```HTTP
HTTP/1.1 200 OK
Date: Wed, 05 Jul 2017 05:40:55 GMT
Content-Type: application/json
Server: Kestrel
Content-Length: 39

{"result":"Hello world, from NWheels!"}
```

### Authorization

It worths noting that `[SecurityCheck.AllowAnonymous]` attribute here is required to allow access without prior authentication and validation of claims. 

Authorization infrastructure of NWheels transparently enforces access control rules to resources, components, and data throughout all execution paths. The rules can either be declared with attributes (like in this example), or configured through access control API. Depending on application requirements, configuration through the API can either be hard-coded, or based on data in a persistent storage (e.g. DB).

#### HelloWorldApp.cs - web app

The next piece is user interface. NWheels dramatically boosts development and maintenance productivity by supporting declarative UI. The UI is declared through high-level conceptual models, abstracted from concrete technology stacks. 

The models focus on UI structure, navigation, and binding to business data and capabilities. Lower-level front-end/UX and client/server communication details are not concerned on this level. 

Auhtorization rules that control access to bound data and capabilities are automatically reflected in the user interface.

```csharp
[WebAppComponent]
public class HelloWorldApp : WebApp<Empty.SessionState>
{
    [DefaultPage]
    public class HomePage : WebPage<Empty.ViewModel>
    {
        [ViewModelContract]
        public class HelloWorldViewModel 
        {
            [FieldContract.Required]
            public string Name;
            [FieldContract.Semantics.Output, FieldContract.Presentation.Label("WeSay")]
            public string Message;
        }

        [ContentElement] 
        [TransactionWizard.Configure(SubmitCommandLabel = "Go")]
        public TransactionWizard<HelloWorldViewModel> Transaction { get; set; }

        protected override void ImplementController()
        {
            Transaction.OnSubmit.Invoke<HelloWorldTx>(
                tx => tx.Hello(Transaction.Model.Name)
            ).Then(
                result => Script.Assign(Transaction.Model.Message, result)
            );
        }
    }
}
```
Stunning high-usability user interfaces are created separately by UX experts in corresponding interaction platforms. The experts build UI technology stacks, and provide code generators that implement UI models on top of those stacks. User interfaces are allowed to have numerous themes and variations. 

Sometimes though, all this is not enough. Certain UI areas demand unique touch. In such cases, parts of generated platform-specific code and assets can be manually adjusted or replaced. 

Besides the web, we aim to support mobile native apps, desktop apps, SmartTV, IVR, and IoT platforms. 

# Architecture highlights

### A-to-Z solution

- Development, customization, deployment, operation, and maintenance aspects are covered altogether.
- Pre-implemented field-proven architectures, technology stacks, and automated toolchains are supplied, together with programming models for developers. 
- Unlike many other RAD platforms, user interface is fully covered and is first class citizen in  architecture and technology stack.
- This toolbox is all extensible. Experiment and introduce support for new architectures, technology stacks, and programming models.

### cross-platform

- NWheels-based applications are developed in C# and target cross-platform .NET Core (Linux/Windows/macOS servers). Legacy .NET Framework can also be targeted for Windows servers.

### all-in-C#

- Mechanical and repetitive coding is eliminated. Layers outside of core business logic are based on declarative and concise C# programming models. For instance, user interface, data access, claims-based authorization, and network communications including RESTful APIs are expressed through declarative models. 

### technology abstraction & full control

- Application code is abstracted from concrete technology. There is no need to gain expertise with numerous products and tools, or program against variety of platforms, languages, and frameworks. 

- Instead, technology-specific code generators transparently implement C# application models per concrete technology. These generators are supplied by pluggable _technology adapter modules_, contributed by experts in corresponding technology stacks.

- Manually-written technology-specific code is allowed where full control over the underlying technology stack is required.

### business logic & building blocks

- Robust structuring of application problem domain is ensured by combination of microservice architecture and _domain objects framework_. This framework flexibly scales from anemic domain models to fully-fledged domain-driven design.

- Many problem domains have well-formed field-proven designs. Such designs can be captured in _domain building block_ modules. Often applications can reuse domain building blocks, instead of reinventing the wheels. Due to great vertical and horizontal composition features of the domain objects framework, domain building blocks can easily be inherited, extended, and flexibly adjusted to specific application requirements.  

- Domain building blocks are contributed by developers with strong expertise in corresponding domains.

# Getting Involved

Impressed? We'd like having you onboard!

Community is a vital part of the NWheels project. Here we are building a welcoming and friendly ecosystem for contributors.

Please make yourself familiar with our [Code of Conduct]().

## Where to start

1. Run the demo (if you haven't yet done that)
1. Read our [Contribution Guidelines]() and [Coding Conventions]()
1. Join our team on Slack
1. Look through contribution areas listed below
1. Look for issues labeled `first-timers`

## Contribution areas

NWheels project would benefit from contributions in many different areas:

- **Kernel**: maintain and enhance _NWheels Kernel_, which is the main module that provides common base services critical to any application.
- **Architecture**: contribute _programming models_ and define interfaces with _technology adapter_ modules. This includes documentation, examples, and sample applications.
- **Technology stacks**: construct technology stacks, and contribute _technology adapter_ module for them.
- **Domains**: contribute _domain building block_ modules for common business domains, e.g. e-commerce, CRM, booking, trading, and many more.
- **UX**: contribute technology stacks related to user interaction platforms, including their corresponding _technology adapter_ modules.
- **Creatives**: this includes technical writing, graphics design and UX themes/variations, voice/music, advertisement.

#### On top of the above, we are especially interested in contributions to these areas:

- **Application Security**
  - analyzing source code, DevOps toolchain, and runtime environments generated by NWheels
  - analyzing threats and defining mitigations
  - discovering and fixing vulnerabilities
  - consluting on best application security practices  
- **Artificial Intelligence**
  - contributing programming models that integrate artificial intelligence in enterprise applications. 
  - architecting data collection for feeding AI models in reusable ways
  - contributing AI decisioning engines
  - expending UI programming models with AI-based interaction capabilities
- **Internet of Things** 
  - contributing specializations of UI programming models to different kinds of end-user devices
  - providing technology stack for direct communication with devices, or integration with device cloud platforms
  - contributing technology adapter modules for the above

# Status & Roadmap

Starting from February 2017, we are developing our second take at NWheels. 

### Current milestone: 01 - First Happy Path

- [Milestone](https://github.com/felix-b/NWheels/milestone/2)
- [Scrum board](https://github.com/felix-b/NWheels/projects/1)
- [Issues](https://github.com/felix-b/NWheels/milestone/2)

### Roadmap

Please [find the Roadmap here]().

### History

The first take at NWheels was named _Milestone Afra_. It is now in use by two proprietary real-world applications. Further development was abandoned for high technical debt, few architectural mistakes, and in favor of targeting cross-platform .NET Core.

### Concept proven

Applications built on top of NWheels milestone Afra shown us that the core concept is correct and robust. With that, we learned a lot of lessons, and faced few mistakes in architecture and implementation.

### Timeline

Year|Status
-|-
2013|Started development of Hapil library for code generation, which is an essential part of NWheels concept.
2014|Hapil library gained enough features. Started development of NWheels milestone Afra. Implemented server bootstrapping and metadata-based composition of domain objects. Added support for data persistence through Entity Framework.
2015|Development of NWheels milestone Afra continued. Added support for Mongo DB. Started development of model-based UI and web UI stack based on AngularJS and ASP.NET Web API.
2016|NWheels milestone Afra reached enough maturity to support full-stack development. Two proprietary real-world applications developed on top of NWheels milestone Afra: one released to production, one is in the beta stage. These applications proved that the concept of NWheels works, but taught us a few lessons.
2017|Further development of NWheels milestone Afra abandoned; started development of second take at NWheels, completely from scratch.
