Welcome to NWheels
=======

[![Build status](https://ci.appveyor.com/api/projects/status/qcm23727dm8dplk5/branch/master?svg=true)](https://ci.appveyor.com/project/felix-b/nwheels-bk3vs/branch/master)

Based on our experience, commonality in the needs of enterprise application projects is significantly higher than variability. 

We take this as an opportunity to build a community-based ecosystem, which creates A-to-Z architectural recipes, ready technology stacks, concise programming models, and adaptable domain designs. 

When put together, those parts turn application development into an easy win.

# Demo (C#)

Imagine a very simple application:
- A single page web app, which lets user enter her name, and submit it with a button. 
- A microservice, which exposes RESTful API invoked by the web app button. 
- Business logic (_transaction script_), which receives user's name, and responds with a greeting text. The greeting text is then displayed by the web app.

## Running the demo

### System requirements

- Linux, Windows, or macOS machine (see OSes supported by .NET Core)
- .NET Core 1.0 runtime

### Run microservice
  ```
  $ git clone https://github.com/felix-b/NWheels.git nwheels
  $ cd nwheels
  $ dotnet build
  $ dotnet run Source/NWheels.Samples.FirstHappyPath/bin/debug/netcoreapp1.1/hello.dll
  ```
### Open web application

Browse to [http://localhost:5000](http://localhost:5000)

## Demo source code

The above demo consists of one microservice, which is a C# console application for .NET Core. All including all, it is below 50 source lines of code.
 
#### Program.cs - microservice entry point
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
```
[TransactionScriptComponent]
public class HelloWorldTx
{
    [TransactionScriptMethod]
    public async Task<string> Hello(string name)
    {
        return $"Hello world, from {name}!";
    }
}
```
#### HelloWorldApp.cs - web app
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
            [FieldContract.Required, FieldContract.Semantics.Input]
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


# Architecture

### cross-platform

- NWheels-based applications are developed in C# and target cross-platform .NET Core. The legacy .NET Framework can also be targeted on Windows only.

### all-in-C#

- Mechanical and repetitive coding is eliminated. Layers outside of core business logic are based on declarative and concise C# programming models. For example, user interface, data access, claims-based authorization, and network communications including RESTful APIs are expressed through declarative models. 

### technology abstraction

- Application code is abstracted from concrete technology. Usually, there is no need to program against variety of platforms, languages, and frameworks, which comprise technology stacks of a complete system. Instead, technology-specific code generators transparently implement application models per concrete technology. 

### full control

- Manually-written technology-specific code is allowed for cases that require full control over the underlying technology.

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

## Getting Involved

Community is a vital part of the NWheels project. We aim to build a friendly and welcoming contribution ecosystem around our project. 

NWheels would benefit from contributions in many different areas:
- technology stack expertise
- business domain expertise
- infrastructure and DevOps
- general quality software development

