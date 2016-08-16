Welcome to NWheels
=======

According to our observations, commonality in the needs of enterprise application projects is significantly higher than variability. We take this as an opportunity to cut project costs and timeframes, and reduce technical risks. 

see [Highlights](#highlights)

## Goal

NWheels is an infrastructural ecosystem and ongoing development effort, which is aimed to supply A-to-Z architectural recipes, runtime platform, and high productivity development framework for .NET-based enterprise applications. 

## Status

NWheels is an ongoing development, and has not yet delivered its platform for general availability. 

#### August 2016

The project has completed its first milestone Afra. Core set of platform components were developed, and several real-world applications were built on top of the platform, which allowed validation of architectural concept. 

#### Next Step 

The second milestone Boda, is starting with a fresh new codebase. This rewrite will be based on the lessons learned from milestone Afra, on well documented architecture and feature designs, and on contributions from community. 

## Highlights

NWheels is aimed to have the following characteristics:

- A-to-Z response to common demands:
  - one framework that covers all application layers and tiers: projects are not left to sweat over gluing multiple 3rd-party building blocks together
  - ready answers to common requirements and concerns, ranging from basic features like authorization, to advanced scenarios like elastic scalability
  - built-in support for DevOps procedures and easy integration with application lifecycle management
- Get more for doing less. For instance: 
  - code domain model, logic, and conceptual UI - and get whole layers such as UI apps, data persistence, and REST/backend APIs, automatically implemented by conventions.
  - define authorization rules - and have them automatically enforced through all application layers
  - define semantic logging messages - and get automatic metric collection, thresholds, circuit breakers, and alerts
- Proven architectures, approaches, and patterns, for dramatically less effort on your side. To name a few:
  - micro-services
  - hexagonal architecture 
  - domain-driven design 
- Innovative approaches:
  - prefer convention over implementation - automatic implementation of abstractions by pluggable pipelines of conventions
  - layered customization - multiple reusable orthogonal adaptations, which extend or alter domain model, logic, and conceptual UI, are plugged into customer-specific configurations
  - late compilation - model-based components are late-compiled against customized models and concrete technology stacks
  - building block domains - adaptive and reusable models, logic, and conceptual UI parts for common domains, such as e-commerce, CRM, booking, accounting.
- Platform at your service:
  - communication endpoints, backend APIs, messaging, workflows, scheduled jobs, and more
  - elastic on-demand scalability and failover redundancy
  - cloud, hybrid, and on-premise deployments
  - no need to depend on cloud vendor PasS - no vendor lock-in
- Ready DevOps/ALM toolchain
  - automated deployment to dev boxes and test/prod environments on premise, hybrid, and on cloud
  - runtime health monitoring, metric collection, and tools for production intelligence
  - continuous deployment and continuous integration with optional developer git flow, personal builds, and gated commits
  - product and agile process management
  - all of the above is cross-tracked for maximal visibility and decision support

### What types of applications can be built

#### Architectures

- Typical N-tier applications, which consist of:
  - application tier, composed of one or more micro-services that execute business logic
  - communication endpoints, exposed by application tier for client UI apps and B2B integrations
  - data tier, containing any number of databases, possibly of different vendors and technologies
  - UI apps on various presentation platforms, such as web browser (single-page app), desktop GUI, native mobile apps, and less common ones like Smart TV and IVR. 
- Serverless architecture is naturally achieved by:
  - giving up explicit service boundaries
  - letting domain models and logic be transparently hosted and scaled by the platform

#### SLA categories
  
- non-business-critical
- business-critical (9x5) and mission-critical (24x7)
- low-latency and high-throughput processing, e.g. trading
  
### Where it can run 

- Server side: the platform targets .NET Core, thus server-side components can run on Windows, Linux, or Mac servers.
- Client side: can run on variety of presentation platforms mentioned earlier.
 
### Read more

- Project Wiki - comprehensive information for both consumers and contributors
- Introduction
- Motivation and goals
- Feature explorer 

## Status - August 2016

NWheels is an ongoing development effort. 

Though several real-world applications exist that were built on top of the platform, the project has not yet delivered its product on the level appropriate for general availability. 

Moreover, Milestone II (see below) is starting with a fresh new codebase. Reasons:

- take full benefits of lessons learned in Milestone I
- write code clean from numerous deficiencies and technical debts found in Milestone I
- use a more elegant and friendly library for implementation-by-convention and late-compilation 
- target .NET Core
- let the community build knowledge and take ownership of the entire codebase

#### Milestone I - completed

[![Build status](https://ci.appveyor.com/api/projects/status/x0xcs9lfg4tee88s?svg=true)]
(https://ci.appveyor.com/project/felix-b/nwheels)

The first milestone was fundamental in that it included development of core components, and validation of architectural concept, through building several real-world applications on top of it.

This milestone was mostly one-person project, with no community involved. 

The following was done:
- minimal set of core features was developed, enough for a simple typical enterprise application 
- a couple of real-world applications were built on top of the platform
- architectural concept and feasibility of implementation were proven
- a lot of lessons learned

#### Milestone II - starting

This milestone starts with a fresh new codebase. 

Targets:
- document overall architecture and details of the planned features
- start building community of contributors
- work with the community to refine architecture and feature designs
- proceed with development of the platform, targeting .NET Core
