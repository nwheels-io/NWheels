NWheels
=======

Re-inventing the wheels for the last time. 

[![Build status](https://ci.appveyor.com/api/projects/status/x0xcs9lfg4tee88s?svg=true)]
(https://ci.appveyor.com/project/felix-b/nwheels)

NWheels is A-to-Z architectural recipe and implementation framework for enterprise applications and data processing systems built on the Microsoft .NET platform. 

_Initiated by [Felix Berman](http://wesimplify.net/), a caffeine-driven software developer and hands-on architect._

Introduction
-----

All software systems, among with their unique features, face a lot of common problems to solve. In enterprise applications, according to author's experience, the commonality can reach as much as 80% of the code, related to unique features.

NWheels comes equipped with:

* Comprehensive set of adaptive solutions, which covers majority of the common problems to solve
* Rich programming model for unique features of concrete applications

Building an application with NWheels means:

* Writing minimalistic, conscise, expressive code
* Following field proven patterns and practices, which together comprise robustly architectured solution 

Architecture highlights
-----

**At the core**

* Inversion of control and dependency injection
* Testability, Reliability, Scalability

**Abstract**

* Application code is abstracted from technology stack, including user interface and data storage platforms

**Minimal**

* Minimalistic amount of code to write, test, and maintain
* Convention preferred over implementation - patterns captured as dynamic code compilers

**Consistent**

* Application programming model, which is exposed by NWheels, helps ensure that the architecture is consistently followed throughout the codebase  

**Adaptive**

* Late-compiled entity model and pluggable business logic components
* Easy customization and globalization
* Support for aspect-oriented programming and subject-oriented design of domain data

**Reusable**

* Ability to develop building blocks for common business domains (such as e-commerce or CRM). Domain building blocks  consist of entity model and business logic components. They make minimal assumptions, and are easily adaptable to requirements of concrete applications.

Project Status (July 2015)
-----

NWheels is currently under development. About half of planned infrastructure components and services exist. Detailed status can be [found here](https://github.com/felix-b/NWheels/wiki/Development-Status).

The core architecture is mostly stable, and the application programming model is starting to settle down. API changes are still possible without notice, and backward compatibility is not maintained. 

Early adopters are invited to try NWheels as a preview of future technology. Use for production environments and commercial projects is not currently recommended.

**Along with that, as of July 2015, there are three commercial projects**, in which we are closely involved, that develop applications on top of the NWheels framework. Of them, two are enterprise applications and one is data processing middleware. All three are expected to be in production by October 2015. These projects are closely supported and their feature requests are served at top priority.

Once we have three NWheels-based software systems running in production, we expect to be **confident enough to recommend NWheels as a platform for commercial software development**.

Feature Set
-----

**Core services**

* Configuration, diagnostics, authorization
* Dependency injection, component lifecycle management and hooks
* Globalization, customization
* Pub/Sub, Map/Reduce, scale out, fault tolerance

**Data access**

* Adaptive entity model, repository, unit of work
* Pluggable ORM/ODM & database, universal DML (C# for stored procedures), automated schema migrations
* Bulk & atomic operations, caching, in-memory data grid, static & dynamic reporting

**Business logic**

* Entity triggers, workflows, rule engines, transaction scripts
* Distributed actors, background jobs
* Custom components

**User interface**

* Common high-level UI description language (UIDL)
* No one style fits all: the apps look and behave in a way native to every platform 
* Implementations for web, native mobile, native desktop, Smart TV, IVR
* Custom implementations

**Communication endpoints**

* REST, SOAP, HTTP
* Proprietary protocols over TCP, UDP, UDT
* Messaging middleware connectors 
* Custom endpoints

Further Learning
-----

Please see [Documentation](https://github.com/felix-b/NWheels/wiki/Documentation)

Getting Involved
-----

First of all, **contributions are welcome** :)

Chances are that NWheels is a game changer in the field of enterprise software development: 

* For letting developers do less and get more done. 
* For letting executives deliver quicker and (both) with higher quality. 
* For letting stakeholders engage earlier and save rework cycles. 
* For letting entrepreneurs get more innovation for less resources. 
* For letting the world have more software services in less time.

If you would like to take part in this effort, please find further information in the [Guidelines section](https://github.com/felix-b/NWheels/wiki/Guidelines).
 
