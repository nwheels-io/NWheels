

## What is NWheels:

- [Ejectable]() way to develop software of any scale with tiny amount of code in [intent-as-code DSLs]()
- A pipeline that transpiles intents into working codebases in selected languages and technology stacks
- A set of architectural recipes that properly handle [many aspects of your software]() out of the box 
- Planned to be: an ecosystem with a community that maintains [pluggable DSLs]() and [technology adapters]()
- An ecosystem where [reusable domain verticals]() can be shared and consumed
- A future concept of software development

## Who NWheels is good for:

- **[Greenfield projects]()**: work both fast and right, starting on day one [[Explain]()]
- **[Startups]()**: game changer, you probably won't need angels [[Explain]()]
- **[Freelance]()**: complete projects lightning fast [[Explain]()]
- **[Prototyping or scaffolding]()**: instant visible results and feedback from business [[Explain]()]
- **[Designing domain models]()**: try what-if alternative designs in no time, picking up the winner [[Explain]()]
- **[Willing to go DDD]()**: DSLs allow for low-cost DDD, most projects would profit [[Explain]()]

## How it works:

- Application is expressed with intents coded in DSLs 
- Every layer and aspect has a dedicated DSL (e.g., REST API DSL, Authorization DSL, Monitoring DSL)
- The DSLs are based on C# in .NET Core: use existing cross-platform IDEs and tooling
- Every DSL is backed by a parser that digests coded intents into metadata structures
- Pluggable technology adapters translate metadata into working code and configurations in specific languages, targeting specific platforms, frameworks, or products 
- The generated code is well structured for easy maintenance, as if it was written by a competent developer
- Generated codebases include DevOps pipelines that release and monitor your software

## What are the drawbacks?

NWheels is a cutting edge approach, with all the associated consequences. But you can always use the Eject button.

## Eject button

If anything goes wrong, you are able to eject at any moment:

- drop the DSLs, and start maintaining generated codebase
- the generated code contains no traces of NWheels

## Can it actually work?

Two serious proprietary real-world applications were built with NWheels v1. Both are in production, and the concept had proven to be right. Yet we realized flaws in our implementation approach. Hence we've started NWheels v2, which comes to fix the flaws.
