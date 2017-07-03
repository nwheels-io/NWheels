Welcome to NWheels
=======

[![Build status](https://ci.appveyor.com/api/projects/status/qcm23727dm8dplk5/branch/master?svg=true)](https://ci.appveyor.com/project/felix-b/nwheels-bk3vs/branch/master)

Based on our experience, commonality in the needs of enterprise application projects is significantly higher than variability. 

We take this as an opportunity to build a community-based ecosystem, which creates A-to-Z architectural recipes, ready technology stacks, concise programming models, and adaptable domain designs. 

We put those parts together, and turn enterprise application development into an easy win.

# How it works

### application lifecycle management

- Development, customization, deployment, operation, and maintenance aspects are covered altogether.
- Pre-implemented field-proven architectures, technology stacks, and automated toolchains are supplied, together with programming models for developers.
- This toolbox is all extensible. Experiment and introduce support for new architectures, technologies, and programming models.

### cross-platform

- NWheels-based applications are developed in C# and target cross-platform .NET Core (Linux/Windows/macOS servers). Legacy .NET Framework can also be targeted for Windows servers.

### all-in-C#

- Mechanical and repetitive coding is eliminated. Layers outside of core business logic are based on declarative and concise C# programming models. For instance, user interface, data access, claims-based authorization, and network communications including RESTful APIs are expressed through declarative models. 

### technology abstraction & full control

- Application code is abstracted from concrete technology. There is no need to gain expertise with numerous products and tools, or program against variety of platforms, languages, and frameworks that comprise technology stacks of a complete system. 

- Instead, technology-specific code generators transparently implement application C# models per concrete technology. These generators are supplied by pluggable _technology adapter modules_, contributed by experts in corresponding technologies.

- Manually-written technology-specific code is allowed where full control over the underlying technology is required.   

### business logic & building blocks

- Robust structuring of application problem domain is ensured by combination of microservice architecture and _domain objects framework_. This framework flexibly scales from anemic domain models to fully-fledged domain-driven design.

- Many problem domains already have well established designs. These designs are captured in _domain building block_ modules, which can be reused by applications. Due to great vertical and horizontal composition features of the domain objects framework, domain building blocks can easily be inherited, extended, and flexibly adjusted to specific application requirements.  

- Domain building blocks are contributed by developers with strong expertise in corresponding domains.

# Demo

Imagine a very simple application:
- A single page web app, which lets user enter her name, and submit it with a button. 
- A microservice, which handles the submission. The microservice exposes RESTful API invoked by the web app button. 
- Business logic (_transaction script_), which receives user's name, and responds with a greeting text. The greeting text is then displayed by the web app.

NWheels-based implementation is below 50 lines of C# code, all layers included.

## Running the demo locally

### System requirements

- Linux, Windows, or macOS machine (see list of OS versions supported by .NET Core)
- .NET Core SDK 1.0 or later (download here)

### Run microservice
  ```
  $ git clone https://github.com/felix-b/NWheels.git nwheels
  $ cd nwheels
  $ dotnet build
  $ dotnet run Source/NWheels.Samples.FirstHappyPath/bin/debug/netcoreapp1.1/hello.dll
  ```
### Open web application

Browse to [http://localhost:5000](http://localhost:5000)

## Source code explained

#### Program.cs - microservice entry point

It is super simple to bootstrap a microservice. Most of the time, you're all set with the defaults. For advanced scenarios, extensible API of `MicroserviceHostBuilder` lets you tailor technology stack to your requirements. 

```
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

```
[TransactionScriptComponent, SecurityCheck.AllowAnonymous]
public class HelloWorldTx
{
    [TransactionScriptMethod]
    public async Task<string> Hello(string name)`
    {
        return $"Hello world, from {name}!";
    }
}
```

There's more under the hood. For instance, default web stack includes RESTful API endpoint, where transaction scripts are one type of supported resources. The endpoint transparently allows invocation of resources through HTTP and other protocols, subject to authorization requirements.

Here, `Hello` method can be invoked through HTTP `POST` request to `http://localhost:5000/tx/HelloWorld/Hello`, with JSON body:

```
{name: "nwheels"}
```

The endpoint will reply with JSON:

```
{result: "Hello world, from nwheels!"}
```

Authorization requirements can either be declared in-place (the `[SecurityCheck.AllowAnonymous]` attribute in this example), or configured as sets of rules through authorization API. The rules can be hard-coded or loaded from a persistent storage (e.g. DB). Authorization rules are transparently enforced throughout all  execution paths.

#### HelloWorldApp.cs - web app

The next piece is user interface. NWheels dramatically boosts development and maintenance productivity by supporting declarative UI. The UI is declared through high-level conceptual models, abstracted from concrete technology stacks. 

The models focus on UI structure, navigation, and binding to business data and capabilities. Lower-level front-end/UX and client/server communication details are not concerned on this level.
```
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

# Getting Involved

Community is a vital part of the NWheels project. We aim to build a friendly and welcoming contribution ecosystem around our project. 

NWheels would benefit from contributions in many different areas:
- technology stack expertise
- business domain expertise
- infrastructure and DevOps
- general quality software development




# How it works

- NWheels-based applications are developed in C# and target cross-platform .NET Core.
- The [Hexagonal architectural approach](http://alistair.cockburn.us/Hexagonal+architecture) (aka _Ports and Adapters_) is at the heart of NWheels architecture
- Application logic and declarative models are written solely in C#, abstracted from concrete technology stack
  - Abstraction allows applications outlive underlying technologies they were originally built on.
  - Nevertheless, abstraction can be bypassed wherever full control over underlying technology is required. 
  - We aim to hit the 20/80 ratio, where 80% of requirements are implemented through declarative models, requiring only 20% of development effort.
- Application problem domains can inherit and adapt pre-existing *_building block domains_*, contributed by experts in those domains.
- Concrete technology stacks are pluggable through *_technology adapter modules_*. These modules are contributed by experts in corresponding technologies. 
- C# declarative models are projected onto concrete technology stack at runtime or during deployment. At that moment, the models are translated into technology-specific code by technology-specific code generators. The code generators are supplied by technology adapter modules.


## Demo

The demo includes a simplest web application backed by one microservice. It runs on Linux, Windows, or macOS. 


## How it Works

- [Hexagonal architectural approach](http://alistair.cockburn.us/Hexagonal+architecture) (aka _Ports and Adapters_) is at the heart of NWheels architecture
- Application logic and models are written solely in C#, abstracted from concrete technology stack
  - Such abstraction can be bypassed wherever full control over underlying technology is required. 
  - We aim to hit the 20/80 ratio, where 80% of requirements are implemented through declarative models, requiring only 20% of development effort.
- Application problem domains can inherit and adapt from pre-existing _building blocks_, contributed by experts in corresponding domains.
- Concrete technology stacks are pluggable through adapter modules. Technology adapter modules are contributed by experts in corresponding technologies. 
- C# declarative models are projected onto concrete technology stack at runtime or during deployment. At that moment, the models are translated into technology-specific code by technology-specific code generators.

