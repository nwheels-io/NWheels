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

