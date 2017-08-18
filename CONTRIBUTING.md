# Contribution guidelines

When contributing to this repository, please follow the guidelines below. 

#### Table of contents

- [Communication](#communication)
- [Workflow](#workflow)
- [Version control structure](#version-control-structure)
- [Repository structure](#repository-structure)
- [Coding conventions](#coding-conventions)
- [Peer reviews](#peer-reviews)
- [Submitting changes](#submitting-changes)
- [First-timers read here](#first-timers-read-here)

## Communication

There are two channels of communication:

- [Slack](#slack)
- [Issues on GitHub](#issues-on-github)

### Slack: 

- General announcements: channel `#general`
- Proposal of new ideas: channel `#idea`
- Overview of new technologies and enhancements, especially those that can be useful for NWheels: channel `#tech`.
- Questions and answers, helping new (and not only) contributors: channel `#help`. 
- Anything else: channel `#random`.

Every contributor should be subscribed to NWheels project at Slack, [nwheels.slack.com](nwheels.slack.com).

- How to subscribe: please send an email to [team@nwheels.io](mailto:team@nwheels.io), with subject `Join NWheels team`. You will receive back an email from Slack with join link and instructions. 

### Issues on GitHub

All technical communication should be done in GitHub issues. Discussion thread of an issue serves focal point for suggestions, feedback, questions, clarifications, and documentation related to the issue. 

Issues are labeled in several dimensions:

- **Issue type** - one of: 
  - `new feature`: significant piece of new functionality  
  - `enhancement`: enhancement to an existing feature
  - `bug`: a bug :)
  - `task`: something that needs to be done, which is not feature, enhancement, or bug
  - `question`: only answer is required; no changes need to be done 
  - `epic`: a parent issue that covers a large subject
- **Issue flags** - multiple can be labeled:
  - `tech debt`: something that should have been done, according to the guidelines and conventions, but was skipped or postponed.
  - `refactoring`: improvement in internal design of existing code, without change in public API or behavior.
  - `redesign`: revision of previous decisions, including changes of internal design, public APIs, and behavior.  
  - `beginners-friendly`: an issue that is easy to handle for beginner contributors.
  - `duplicate`: the subject is already covered by an existing issue. Must have a comment that links to the existing issue.
  - `wontfix`: an issue we are not going to resolve. Must have a comment that states justification.
  - `invalid`: the issue is spam or violates guidelines. Such issues won't be handled. 
- **Related modules**: these labels provide useful breakdowns when searching. Multiple modules can be labeled on one issue. 
  - `module-xxxxx`: NWheels modules the issue is related to, which can be the Kernel, platforms, or frameworks. Examples: `module-kernel`, `module-ddd`, `module-messaging`.  
  - `adapter-xxxxx`: technology adapter modules the issue is related to; make it easy to search for anything related to a specific technology stack. Example: `adapter-redis`, `adapter-mongodb`.

## Workflow

### Bugs and enhancements

Bugs and enhancements can be reported by anyone. Please make sure you follow these rules: 

- Always search for existing issues first. Maybe your issue was already reported or suggested as part of another issue.
- If not found, create a new issue, and label it as explained above. If you're going to resolve it, please assign it to yourself right off. 
- Project maintainers will review the issue and prioritize it. Regardless of the priority, if you assigned the issue to yourself, it will be scheduled to current iteration. 
- In cases of critical issues, maintainers may take over and provide their own resolution. Take-overs will be communicated in the issue thread.  
- Everyone is welcome to join the discussion in the issue thread at any stage. Technical clarifications and problem reproductions may be of great help to those who's working on the issue. 

### New features

Please follow these steps when introducing new features to NWheels:

1. **Idea**: it is recommended to first propose initial idea on Slack (`#idea` channel). 
    - If the idea was in general accepted, move next.
1. **Proposal**: create a new issue in GitHub. Summarize discussion that took place on Slack, and provide contents for further discussion. Label the issue as necessary, and assign it to `Proposals` milestone. 
    - From that moment on, all communications on the subject must go through the issue discussion thread.
    - While at `Proposals` stage, the issue must be designed in more detail. Implications and corner cases must be considered.
    - As soon as the issue is detailed enough, and positive feedback is received, maintainers will move it to `Backlog` milestone.
1. **Backlog**: here the issue is waiting to be assigned to an iteration milestone. Further discission and design are welcome at this stage. 
1. **Iteration milestone** (explained below): here the issue is actively worked on to completion. A pull request must be created, which captures changes made in context of this specific issue. The goal is to resolve the issue and merge the pull request (see [Submitting changes](#submitting-changes) for more detials).

### Iteration milestones

- All changes are introduced in context of iteration milestones. 
- The purpose of iteration milestones is provide our users with timeframes for expected enhancements. 
- Iteration milestones are sequentially numbered, and added a title, which summaries milestone target. For example, "_01 First happy path_". 
- Scope-wise, milestones are planned much like iterations: issues are picked from the backlog, and assigned to the milestone. 
- Time-wise, milestones are defined a likely due date, but the date can change as we go. If a due date cannot be met, it is replaced with a new expected date.
- **Backlog** and **Proposals** are also milestones, but they don't represent iterations.

### Scrum board

We use GitHub _Project Boards_ feature to create [Scrum](https://en.wikipedia.org/wiki/Scrum_\(software_development\)) boards.

- Read [GitHub Help on Project Boards](https://help.github.com/articles/about-project-boards/#)

Every iteration milestone has an associated Scrum board, which visualizes milestone progress. Scrum boards are named after the milestones.

Scrum board has the following lanes:

- **TODO**: issues planned for the iteration, usually not yet assigned
- **IN PROGRESS**: assigned issues being actively worked on
- **ON-HOLD**: issues whose resolution was impeded
- **DONE**: resolved issues

## Version control structure

- Current iteration milestone is in the `master` branch. 
- Direct push to `master` is not allowed. The only way of introducing commits to `master` is by merging pull requests. 
- Every completed milestone is labeled with an annotated tag containing milestone number, like this: `milestone-01`.
- Branches starting with `pr-` represent pull requests created by repo owner (`felix-b`). Those branches are removed as soon as their corresponding PRs are merged.
- Branch `afra` stores the _milestone Afra_ version of NWheels.

## Repository structure

The repo has three subfolders:
- `Sources` - all sources of the NWheels project, including samples
- `Docs` - documentation materials, including wiki markdown, presentations, and documentation-related creatives 
- `Tools` - 3rd party utulities and tools used for NWheels development; stored as binaries, sources, or Git submodules.

### Source folder structure

- The sources are located in the `./Source` subfolder of the repo. 
- Single solution file named `nwheels.sln` includes all projects. 
- All projects reside in direct subfolders of the `./Source` folder. 
- Project subfolders are named after the projects they contain. 

```
/Source
 |
 +-- nwheels.sln
 |
 +-- /NWheels.Api
 |    |
 |    +-- NWheels.Api.csproj
 |    |
 |   ...
 |
 +-- /NWheels.Implementation
 |    |
 |    +-- NWheels.Implementation.csproj
 |    |
 |   ...
...
```

### Projects vs. modules

NWheels modules are divided into two major categories:

Category|Description|Projects
---|---|---
Core modules|Provide microservice facilities and programming models, abstracted from concrete technology stacks|Module is entirely encapsulated by a C# project.
Technology adapter modules|Pluggable adapters to concrete technology stacks.|Module is encapsulated by a C# project. In addition, it may contain _technology-specific projects or objects_, which are developed with a different set of programming languages and tools.

### Technology-specific projects and objects
  
Technology-specific projects and objects are placed in a subfolder under the C# project. 

In the example below, module `NWheels.Uidl.Adapters.WebAngular` adapts UI port to web technology stack based on the Angular framework. Front-end projects and assets are located inside a subfolder named `ClientSide`. 
```
+-- /NWheels.Uidl.Adapters.WebAngular
|    |
|    +-- NWheels.Uidl.Adapters.WebAngular.csproj
|    |
|    +-- /ClientSide
|         |
|        ...
```

## Coding conventions

The following conventions apply to C# projects. 

- Technology adapter modules may contain projects or objects, which are developed with a different set of programming languages and tools. They are beyond the scope of these conventions. 

### Development environment

We currently develop on Windows, with the following tools installed on dev machine:

- Visual Studio 2017 Community Edition or higher, v15.2 or later
- .NET Core SDK 1.1 or later 

### Development on Linux and macOS

We are looking forward to migrating to Visual Studio Code. This will allow development to be done on Linux and macOS.

Currently, the following features are missing in VSCode, stopping us from migrating right away:

- Integrated test runner and result browser - [vscode issue #9505](https://github.com/Microsoft/vscode/issues/9505)
- Support of Roslyn-based code analyzers and fixes, with a rich set of bug detectors and refactorings - [omnisharp-vscode issue #43](https://github.com/OmniSharp/omnisharp-vscode/issues/43)

### Online services

We use the following online services, provided for free:

- [NuGet.org](https://www.nuget.org/) - package repository (planned)
- [AppVeyor](https://ci.appveyor.com/project/felix-b/nwheels-bk3vs/branch/master) - continuous integration build on Windows
- [Travis CI](https://travis-ci.org/) (planned) - continuous integration build on Linux
- [CodeCov](https://codecov.io/github/felix-b/NWheels) - test coverage reporting

### Testing

Automated testing is a vital part of the project. Number of subjects that NWheels covers is large enough to make manual testing impossible. Considering that NWheels is aimed to serve a basis for numerous applications, we cannot afford functionality that is not automatically verifiable by continuous delivery pipeline. 

For this reason, we practice [test-driven development](
https://en.wikipedia.org/wiki/Test-driven_development) (TDD). Testing is an integral part of any change made to production code. 

In particular, all C# code must be covered with automatic tests. We use code coverage metric to validate that. Any combination of the following is counted towards the total coverage:

- **Unit tests**: testing pieces of logic in isolation from their environment.
  -  _Example: unit-testing of a string formatting utility is isolated from other components, let alone real microservice containers, databases, communication middleware, etc_.
- **Integration tests**: testing connection between ports and adapters, with real techology stacks attached. 
  - _Example: integration tests of an ORM adapter should run against a real database_.
- **System tests**: running sample microservice(s), testing them  through their endpoints (API calls or messages).
  -  _Example: system tests of an in-memory data grid platform should run a sample microservice implemented on top of the in-memory data grid. The microservice should run in real containers, with real communication endpoints and databases. The tests are performed by feeding the microservice with input messages, and validating its output messages through communication endpoints._
- **End-to-end (e2e) tests**: running a whole sample application including microservices and UI apps, testing it through UI automation. E2e tests differ from sytstem tests in that the system is tested through user interface. 
  - _Example: UI platform should be tested by running a sample application in a real UI environment (e.g. on a mobile device), with real microservice containers and databases. The tests are performed by automation of UI inputs and validation of UI outputs_. 

We attempt to achieve 100% coverage. To account for corner cases that cannot be reliably tested in an automated way, we define the minimum of 80% coverage as the requirement. Automated testing is a vital part of NWheels project. 

Note: additional kinds of tests include stress/load testing, penetration testing, etc. They are not part of the TDD methodology. Development and maintenance of such tests is tracked by separate task issues. 

TDD tests are developed with [xUnit framework](https://xunit.github.io/). Each kind of tests has dedicated helpers supplied by NWheels [TODO: provide link to wiki](). According to test kind, each test class must be annotated with one of the following trait attributes:
- `[Trait("Purpose", "UnitTest")]`
- `[Trait("Purpose", "IntegrationTest")]`
- `[Trait("Purpose", "SystemTest")]`
- `[Trait("Purpose", "EndToEndTest")]`


#### Structure of test projects

Tests reside in dedicated projects, separate from production code. Each test projects is structured according to one of two conventions:
- **Convention 1: All kinds of tests reside in one project**. 
  - Naming 
      - Add `.Tests` suffix to production project name. 
      - Example: `NWheels.Domains.Crm` -> `NWheels.Domains.Crm.Tests` 
  - Folder structure 
     - Project root contains a folder for each test kind (`Unit`, `Integration`, `System`, `EndToEnd`)
     - Inside test kind folder, the subfolders reproduce folder structure of the production project.
     - Example:
        ```
        +-- /NWheels.Domains.Crm
        |    |
        |    +-- NWheels.Domains.Crm.csproj
        |    |
        |    +-- /Parties
        |    +-- /Contacts
        |    
        +-- /NWheels.Domains.Crm.Tests
        |    |
        |    +-- NWheels.Domains.Crm.Tests.csproj
        |    |
        |    +-- /Unit
        |    |    |
        |    |    +-- /Parties
        |    |    +-- /Contacts
        |    |
        |    +-- /EndToEnd
        |    |    |
        |    |    +-- /Parties
        |    |    +-- /Contacts
        |   ...
        ```
- **Convention 2: Each kind of tests resides in a separate project**. 
  - Naming 
      - Add test kind suffix to production project name (`.UnitTests`, `.IntegrationTests`, `.SystemTests`, `.EndToEndTests`)
      - Example: `NWheels.Domains.Crm` -> `NWheels.Domains.Crm.UnitTests`, `NWheels.Domains.Crm.EndToEndTests`. 
  - Folder structure 
     - Project root contains subfolders that reproduce folder structure of the production project.
     - Example:
        ```
        +-- /NWheels.Domains.Crm
        |    |
        |    +-- NWheels.Domains.Crm.csproj
        |    |
        |    +-- /Parties
        |    +-- /Contacts
        |    
        +-- /NWheels.Domains.Crm.UnitTests
        |    |
        |    +-- NWheels.Domains.Crm.UnitTests.csproj
        |    |
        |    +-- /Parties
        |    +-- /Contacts
        |    
        +-- /NWheels.Domains.Crm.EndToEndTests
        |    |
        |    +-- NWheels.Domains.Crm.EndToEndTests.csproj
        |    |
        |    +-- /Parties
        |    +-- /Contacts
        ```
Single-project convention is preferred when multiple projects separated by test kind are too thin.

### Coding requirements

- Follow [SOLID](https://en.wikipedia.org/wiki/SOLID_(object-oriented_design)) for object-oriented design
- Follow [KISS](https://en.wikipedia.org/wiki/KISS_principle), [YAGNI](https://en.wikipedia.org/wiki/You_aren%27t_gonna_need_it), [DRY](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself).
- Prefer [convention over configuration](https://en.wikipedia.org/wiki/Convention_over_configuration)
- Write code with no [smells](https://en.wikipedia.org/wiki/Code_smell); when changing existing code, [refactor](https://martinfowler.com/books/refactoring.html) as necessary
- Write simple, readable, self-descriptive code
  - when you are coding, don't think about writing the code; think about reading it.
  - choose self-explanatory names
  - write short methods; code of every single method should be trivial to understand
  - whenever possible, make objects immutable; immutable objects don't add side effects to the system. 
  - prefer readability over optimization; do not optimize, unless you've measured a performance bottleneck.
- Delegate validation of your intentions to compiler, as much as possible
  - wherever possible, apply `readonly` to fields
  - use least possible visibility for types and members 
- Beware of performance. Avoid performance killers (e.g. Reflection), especially in performance-sensitive parts of code
  - Performance-sensitive parts are those standing on execution paths counted towards system throughput (e.g. request handling pipeline in a web server).
- Cover production code with tests. Coverage of at least 80% is required. 

### C# coding style

Many aspects of coding style are automatically enforced by Visual Studio. There are settings and snippet files that have to be imported - see [First-timers](#first-timers) for details.  

1. **Tabs and line endings**

   - Indentation size is 4.
   - Use spaces for indentation. No tabs please!
   - Configure Git to checkout Windows-style, commit Unix-style line endings (set `core.autocrlf` to `true`).

1. **Common guidelines**. All types and members must follow common .NET guidelines for design and naming.
    - Read [Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)

1. **Fields**. Private fields naming convention:
   - `_camelCase` for instance fields
   - `_s_camelCase` for static fields
   
   Non-private fields are only allowed if they are `readonly`. Otherwise, they should be encapsulated in properties.

1. **Member separator lines**. For better readability, we separate members, or groups of single-line members, with a horizontal separator line.
   - Separator line is typed as a line comment consisting of the dash characters: `//---`.  It starts at current indent in code, and continues up to column 160. It is preceded and followed by an empty line.
   - Example (read sources for more examples):
     ```
     public void MethodOne() 
     {
     } 
 
     //---------------------------------------------------------
     
     public void MethodTwo() 
     {
     }
     ```

    - We defined code snippets for VS2017, as follows:
      - `000 + TAB` - insert separator line at indent level 0
      - `111 + TAB` - insert separator line at indent level 1
      - `222 + TAB` - insert separator line at indent level 2
      - `333 + TAB` - insert separator line at indent level 3
      - `444 + TAB` - insert separator line at indent level 4

1. **Curly braces**. Curly braces are required for all flow control statements (`if`, `for`, etc), even when they enclose single statement.  Curly braces are opened on the same line for inlined declarations:
   - anonymous method
   - collection initializer

   In all other cases, curly braces are opened on a new line.

## Peer reviews

- All code must pass review. 
- Code reviews are done on pull requests.
- Work-in-progress pull requests can be reviewed and commented by anyone. 
- For completed PRs, at least one approved code review from one of project maintainers is required, before the PR can be merged. 
  - _A note to maintainers: we should periodically look for completed PRs (those not starting with "WIP"), which have no assigned reviewer. Then one of us self-assigns as a reviewer_.

## Submitting changes

All changes should be submitted through pull requests (PRs). 
- Read [GitHub help on Pull Requests](https://help.github.com/categories/collaborating-with-issues-and-pull-requests/)

Please comment in the PR thread, referring to the issue being handled by the PR. For example:
```
This PR handles #68 - listen on 0.0.0.0 instead of localhost
```

### Work in progress

We encourage submitting work-in-progress PRs, so that other contributors can review and react early. 

- Please prefix names of  work-in-progress PRs with "_WIP_" - for example, "_WIP - listen on 0.0.0.0 instead of localhost_". We will know that it's an ongoing effort and give feedback accordingly.
- Once you consider the PR ready, please remove the "_WIP_" prefix from its name. We will treat it as a candidate for merging. 

### Walkthrough

Please only submit changes on issues which are assigned to you and scheduled for current iteration milestone. Below are step-by-step instructions.

1. Pick an unassigned issue from the **TODO** lane of current iteration Scrum board.
1. Leave comment "_Assign to me_" in the issue thread, which states that you are going to work on the issue. Make sure no one has left this comment already.
1. Fork the repo, branch off `master`, create a work-in-progress PR, and start working. This step is explained in more detail in [First-timers](#first-timers) section.
1. Project maintainers will assign the issue to you and move it to **IN PROGRESS** lane in the Scrum board.
1. Commit & push frequently, and watch for feedback comments and reviews. They can appear on threads of either the PR, or the original issue. 
1. When done, remove the _WIP_ prefix from PR name, and wait for a maintainer's review. Follow up with requested changes, if any.
1. Once you get an approved review from a maintainer, you're done. Maintainers will merge the PR.

### Requirements for merge

Before a PR can be merged, it must meet the following requirements:

- The code must follow [coding conventions](#coding-conventions) (explained above).
- The code must be covered by tests. 
  - If the PR includes changes to existing functionality, the tests that cover changed functionality must be changed accordingly.
  - Any appropriate combination of unit/integration/system tests is accepted, and their union coverage is counted. 
  - Coverage summary is automatically posted by CodeCov on the PR thread. 
  - Coverage of 80% is the required minimum (validated by CodeCov).
  - Coverage of less than 100% must be justified - comment on the PR thread (e.g., a race condition that cannot be reliably reproduced).
  - PR is not allowed to drop the coverage by more than 5% compared to its base in `master` (validated by CodeCov).  
- The PR must pass CI builds attached to NWheels repo. Their details appear on the PR issue thread.
- The PR must have a review with approval by one of project maintainers.

## First-timers Read Here

Contributing to an open source project for the first time can be hard and little overwhelming. Here we attempt to make this first step easier, and provide all the information you may need.  

- If you never contributed to an open source project before, you may find the following links useful:
  - http://www.firsttimersonly.com/
  - https://opensource.guide/how-to-contribute/

### Installing development tools

You will need:

- A Windows machine
- Visual Studio 2017 Community Edition or higher, v15.2 or later
  - Make sure .NET Core development components are selected in the installation
  - NWheels currently requires .NET Core SDK v1.1 or later

In Visual Studio, import the following:

- Language settings file
  - location in repo: `Docs\CodingConventions\VisualStudioSettings\coding-conventions.vssettings`
  - contents: naming conventions and code styling for C# language.
  - how to import: go to main menu > Tools > Import and Export Settings
- Code snippet files
  - location in repo: `Docs\CodingConventions\VisualStudioCodeSnippets`
  - contents: code snippets that insert member separator lines.
  - how to import: go to main menu > Tools > Code Snippets Manager 

### Forking the repository

All your work will be done on your fork of the main repository.

- Read [GitHub Help on Forking a Repo](https://help.github.com/articles/fork-a-repo/)

Once you have the fork, clone the fork to your machine. For the following explanations, we assume the clone is located at `C:\NWheels\` - replace with your path as necessary.

Now let's build the fork.
```
> C:
> cd C:\NWheels\Source
> git checkout master
> dotnet restore
> dotnet build
```
If your environment was set up correctly, the build should be successful.

### Working on your first issue

1. Carefully read all sections above.

1. Pick an unassigned issue from the **TODO** lane of current iteration Scrum board. 
    - Current iteration milestone is listed in the [main README](README.md#current-status) in the **Current Status** section.
    - Go to [Projects](https://github.com/felix-b/NWheels/projects) and click on the project of the current milestone.
    - If you are not sure what to choose, please read through our [Roadmap](Docs/Wiki/roadmap.md#contribution-areas), section **Contribution Areas**.
    - Prefer issues that are labeled `beginners-friendly`.

1. Leave comment "_Assign to me_" in the issue thread, which states that you are going to work on the issue. Make sure no one has left this comment already.
   - One assignee is responsible for working on the issue at any given time. 
   - Project maintainers will assign the issue to you, and move it to **IN PROGRESS** lane in the Scrum board.

1. Create topic branch for the issue, as follows (replace _my-issue_ with a name that represents the issue):
   ```
   git checkout -b my-issue --track origin/master
   ```

1. After some initial work is done, create a pull request by pushing your changes to your fork repository. 
   - Read [GitHub help on Pull Requests](https://help.github.com/categories/collaborating-with-issues-and-pull-requests/) 
   - Push to a new remote branch, named the same as your local topic branch:
      ```
      git push origin HEAD:my-issue
      ```     
   - For subsequent pushes to go there automatically, set the upstream branch:
      ```
      git branch -u origin/my-issue
      ```
   - Go to your fork repo in the GitHub website, and create a pull request (PR). If you are going to do more commits for the issue, start the name of the PR with "_WIP_", which stands for _work in progress_. 
   - Comment on the PR thread, referring to the issue being resolved by the PR. For example:
     ```
     This PR handles #68 - listen on 0.0.0.0 instead of localhost
     ```

1. Continuous integration builds will run on every push to the PR branch. The builds are listed under the Checks section in the end of PR thread. Fix build failures, if any occur.     

1. Continue working and pushing more commits. Make sure your changes comply with the [coding conventions](#coding-conventions). Keep CI builds passing. Keep test coverage above 80%. Watch for comments and reviews from other contributors. Be responsive to them.
   - Reviews will be posted on the PR thread. 
   - Comments can be posted on either PR or the issue thread.
   - Test coverage summary will be automatically posted by CodeCov on the PR thread. 

1. When you consider the issue to be resolved, rename the pull request and remove the "_WIP_" prefix. Watch for a review from one of project maintainers. Follow up with answers and changes, if any are requested.

1. Once you get an approved review from a project maintainer, you're done. Congratulations! Your changes will be merged into the main NWheels repo. The issue will be moved to **DONE** lane in the Scrum board. 

### Reaching out for help 

Please feel free to reach out for help. We are building a  friendly and welcoming community. We'll be glad to assist you.

- Post questions related to the issue on the issue thread
- Post questions related to submitting and merging your changes on the PR thread
- Post questions related to working procedures on Slack, channel `#help`. 
   
Good luck!
