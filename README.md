<img align="left" src="docs/images/logo-132.png"/>

## Welcome to NWheels

CURRENT STATUS: the `master` branch is **GEN2**, which is in incubation phase. The field-proven **GEN1** is in the `gen-1` branch. Yet **GEN1** has several flaws in the concept, which **GEN2** comes to fix.


# How it works

<img src="docs/images/nwheels-hexagon-2.png" align="right">

- **Intents as code**: express your application with minimal amount of code clean from technology details
- **Programming model DSLs**: APIs on top of which _intents_ are coded; on the outer side, the _intents_ are digested into _metadata_ for _technology adapters_.
- **Technology adapters**: pluggable code generators, which take _metadata_ as input from _programming models_, and generate code for concrete platforms and frameworks
- **Generated implementations**: production-ready human-maintainable codebase, including CI/CD pipeline

Maintain your code in DSLs in the long run, because DSLs are great cost saver and velocity booster. Alternatively, use NWheels as a scaffolding or prototyping tool. 

You can eject at any moment: just drop the DSLs and start maintaining generated codebase. You should only eject when you absolutely have to.

# Getting started

## Installation

### Prerequisites

- A machine running Linux, macOS, or Windows
- .NET Core SDK 2.1 or higher

### Install CLI

```
$ dotnet tool install -g nwheels --version 2.0.0-alpha1
```

## Demo

```
$ git clone https://github.com/nwheels-io/NWheels.git nwheels
$ cd nwheels/demos/01-todo-list
$ dotnet run
```

## Creating your own application

```
$ dotnet new nwheels MyFirstApp
$ cd MyFirstApp
$ dotnet run
```


