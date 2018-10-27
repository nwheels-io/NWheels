NWheels
=====
An ejectable full-stack application framework, which lets you code intentions, and generates implementations per your technology choices. 

## Features

- Intentions-as-code
- Technology abstraction
- Testability of your application
- Extensibility of your application
- Extensibility of the framework
- Ready solutions to common requirements
- Production readiness and continuous delivery pipeline
- Ability to share and reuse intentions-as-code packages
- Ability to eject (take ownership of generated implementations)

## Why

- Reduce costs and timeframes
- Stop re-inventing the wheel 
- Work both fast and right, starting on day one
- Share and reuse adaptable solutions in business domains (e.g., e-commerce, CRM, marketing, fintech, ...)

## Demo

The demo implements a full-stack version of typical "Todo List" application. The source code can be found at https://github.com/nwheels-io/NWheels.Demos/TodoList.

> Note that technology choices made in the demo are just an example. In your applications, you can choose products and frameworks for which _technology adapter packages_ exist. 

Here is what you get for as little as 100 lines of _intentions as code_: 

- Single-page web app (React + Redux), including:
  - Responsive design (customizable stock templates / your own design)
  - Progressive behavior, including server-side rendering
  - Production build (Create React App: WebPack, Babel, ...)  
- Native Android & iOS apps (React Native)
- Stateless microservice (ASP.NET Core), which provides:
  - RESTful API (HTTP)
  - Input validation
  - Data access to MongoDB database 
  - Optimistic concurrency
  - Logging and telemetry (Elastic, see below)
- Production-ready backend environment (Docker & Kubernetes), including:
  - Deployment anywhere on Kubernetes cluster (the cluster must exist) 
  - Free SSL certificate (Let's Encrypt)    
  - Reverse proxy (NGINX) with SSL termination and request throttling
  - Monitoring and alerting (Elastic), including health/liveness checks 
  - High availability (DB redundancy, container orchestration) 
- Continuous delivery pipeline (Git + Jenkins), which include:
  - git repository (hosted anywhere)
  - tests: unit, API, end-to-end, stress/load (based on intentions-as-code specs)
  - code coverage metrics
  - DB migrations
  - deployment to mobile stores
  - deployment to backend environments 

Requirements

- Linux, macOS, or Windows
- .NET Core SDK 2.1 or higher
- Docker 18.06 or higher

Install NWheels CLI:

```
$ dotnet tool install --global NWheels.Cli
```

Clone and build demo project:
 
```
$ git clone https://github.com/nwheels-io/NWheels.Demos
$ cd NWheels.Demos/TodoList
$ dotnet build -c Release
```

Generate implementation on top of a concrete technology stack:

```
$ nwheels implement . --stackfile stackfiles/local-demo.yaml
```


```
$ docker-compose 
```

## Getting Started


## Architecture



> It is called **intentions-as-code**: an approach, which lets a software system be coded (in whole or in part) on the conceptual level,  

## Contributing
