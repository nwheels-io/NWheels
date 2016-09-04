
## Milestones

#### Milestone Afra - completed

[![Build status](https://ci.appveyor.com/api/projects/status/x0xcs9lfg4tee88s?svg=true)]
(https://ci.appveyor.com/project/felix-b/nwheels)

The first milestone was fundamental in that it included development of core components, and validation of architectural concept, through building several real-world applications on top of it.

This milestone was mostly one-person project, with no community involved. 

The following was done:
- minimal set of core features was developed, enough for a simple typical enterprise application 
- two real-world applications were built on top of the platform
- architectural concept and feasibility of implementation were proven
- a lot of lessons learned

#### Milestone Boda - starting

This milestone starts with a fresh new codebase. 

Targets:
- document overall architecture and details of the planned features
- start building community of contributors
- work with the community to refine architecture and feature designs
- proceed with development of the platform, targeting .NET Core

The decision to start a new codebase was for these reasons:
- take full benefits of lessons learned in Milestone Afra
- write code clean from numerous deficiencies and technical debts found in Milestone Afra
- use a more elegant and friendly library for implementation-by-convention and late-compilation 
- target .NET Core
- let the community build knowledge and take ownership of the entire codebase



- Target .NET Core (milestone Afra targeted .NET Framework 4.5)
  - Develop and run in Linux, Windows, and OSX 
- Fix major deficiencies of milestone Afra:
  - design, implementation, and exposed APIs must be simpler and cleaner
  - technical debt must be kept low
  - test coverage - do it TDD way
  - documentation - architecture and features must be documented in Wiki and facilitate feedback and discussion.
- 
- Improvements in architecture and design:
  - decouple application framework(s) from core services - allow users create their own frameworks on top of the core services layer.
  - implement code generation mechanisms on top of Roslyn (milestone Afra depends on a great Reflection.Emit-based library named Hapil). Why:
    - Hapil has its own deficiencies; Roslyn does it better, anyway
    - pluggable code templates must be rethought and made much simpler
    - late compilation should preferably occur at deployment time and not at runtime, so Reflection.Emit is not of any benefit.
    - 
  -      


While milestone Afra was mostly one-person project with 2 more programmers involved in development of proprietary applications


 and switching to community-driven phase. It will be based on the lessons learned from milestone Afra, on well documented architecture and feature designs, and on feedback and contributions from community. 

#### Milestone Afra - completed

Proof-of-concept version was developed, and two proprietary real-world applications were built on top of it. This allowed validation of architectural concepts. This version of the platform was mostly one-person project, with 2 more developers involved in development of the applications. 
