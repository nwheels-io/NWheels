# The Concept

<img align="right" src="Docs/Images/nwheels-concept-thumb.png"/>

## What over How

We eliminate manual authoring and maintenance of mechanically repetitive parts throughout the application stack. We derive these from application models enriched with _what_ as opposed to _how_. The rich models allow generating sensible implementations of application layers and aspects, including UI, RESTful API, databases, authorization controls, microservices, container images, and more. 

The implementations are based on field-proven frameworks and products, such as ASP.NET Core and Elastic, to name just a couple. We generate glue that connects application models to underlying technology. Examples of generated pieces include Entity Framework code-first models, ASP.NET Core Web API controllers, React/Redux or Angular web apps, Dockerfiles, and many more.

Technology choices are made by picking pluggable technology adapters, according to requirements at hand. Generated pieces are well structured and clean, as if they were written manually according to corresponging technology best practices. 

With that, NWheels won't stand on your way wherever you want to touch the bare metals, or go creative and unique. Technology adapters easily step back and let you override generated pieces at any level. You can even eject NWheels at any time. After ejecting, your code remains coupled to NWheels programming models, but the generated code becomes part of your codebase, which you maintain and develop manually.
