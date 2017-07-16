# Contribution guidelines

When contributing to this repository, please follow the guidelines below.

#### Table of contents

- [Communication](#communication)
- [Workflow](#workflow)
- [Submitting changes](#submitting-changes)
- [Version control structure](#version-control-structure)
- [Source code structure](#source-code-structure)
- [Coding conventions](#coding-conventions)
- [First-timers](#first-timers)

## Communication

There are two channels of communication:

- Slack
- Issues on GitHub

### Slack: 

- General announcements: channel `#general`
- Proposal of new ideas: channel `#idea`
- Overview of new technologies and enhancements, especially those that can be useful for NWheels: channel `#tech`.
- Anything else: channel `#random`.

Every contributor should be subscribed to NWheels project at Slack, [nwheels.slack.com](nwheels.slack.com).

- How to subscribe: please send an email to [team@nwheels.io](mailto:team@nwheels.io), with subject `Join NWheels team`. You will receive back an email from Slack with join link and instructions. 

### Issues on GitHub

All technical communication should be done in GitHub issues. Discussion thread of an issue serves focal point for suggestions, feedback, questions, clarifications, and documentation related to the issue. 

Issues are labeled in several dimensions:

- **Issue type** - exactly one must be labeled: 
  - `new feature`: significant piece of new functionality  
  - `enhancement`: enhancement to an existing feature
  - `bug`: a bug :)
  - `task`: something that needs to be done, which is not feature, enhancement, or bug
  - `question`: only answer is required; no changes need to be done 
  - `epic`: a parent issue that covers a large subject
- **Issue flags** - multiple can be labeled:
  - `tech debt`: something that should have been done, according to these guidelines and conventions, but was skipped or postponed.
  - `refactoring`: improvement in internal design of existing code, without change in public API or behavior.
  - `redesign`: revision of previous decisions, including changes of internal design, public APIs, and behavior.  
  - `duplicate`: the subject is already covered by an existing issue. Must have a comment that links to the existing issue.
  - `wontfix`: an issue we are not going to resolve. Must have a comment that states justification.
  - `invalid`: the issue is spam or violates guidelines. Such issues won't be handled. 
- **Related modules**: these labels provide useful breakdowns when searching. Multiple modules can be labeled on one issue. 
  - `module-xxxxx`: NWheels modules the issue is related to, which can be the Kernel, platforms, or frameworks. Examples: `module-kernel`, `module-ddd`, `module-messaging`.  
  - `adapter-xxxxx`: technology adapter modules the issue is related to; make it easy to search for anything related to a specific technology stack. Example: `adapter-redis`, `adapter-mongodb`.

## Workflow

### Bugs and enhancements

Bugs and enhancements can be reported by anyone. Please make sure you follow these rules: 

- Always search existing issues first. Maybe your issue was already reported or suggested as part of another issue.
- If not found, create a new issue, and label it as explained above. If you're going to resolve it, please assign it to yourself right off. 
- Project maintainers will review the issue and prioritize it. If you assigned the issue to yourself, it will be scheduled to current iteration. 
- In cases of critical issues, maintainers may take over and provide their own resolution. Take-overs will be communicated in the issue thread.  
- Everyone is welcome to join the discussion in the issue thread at any stage. Technical clarifications and problem reproductions may be of great help to those who's working on the issue. 

### New features

Please follow these steps when introducing new features to NWheels:

1. **Idea**: propose initial idea on Slack (`#idea` channel). 
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
- **Backlog** and **Proposals** are also milestones, but they are not related to iterations.

### Scrum board

We use GitHub _Project Boards_ feature to create Scrum boards

- Read [GitHub Help on Project Boards](https://help.github.com/articles/about-project-boards/#)

Every iteration milestone has an associated Scrum board, which visualizes milestone progress. Scrum boards are named after their associated milestones.

Scrum board has the following lanes:

- **TODO**: issues planned for the iteration
- **IN PROGRESS**: issues being actively worked on
- **ON-HOLD**: issues whose resolution was impeded
- **DONE**: resolved issues

Every contributor is required to update the Scrum board as necessary, when she starts or stops working on an issue.

## Submitting changes

All changes should be submitted through pull requests (PRs). 
- Read [GitHub help on Pull Requests](https://help.github.com/categories/collaborating-with-issues-and-pull-requests/)

We encourage submitting work-in-progress PRs, so that the community can review and react early. 

- Please prefix names of  work-in-progress PRs with "_WIP_" - for example, "_WIP - listen on 0.0.0.0 instead of localhost_". We will know that it's an ongoing effort and give feedback accordingly.
- Once you consider the PR ready, please remove the "_WIP_" prefix from its name. We will treat it as a candidate for merging. 

Please only submit changes on issues which are assigned to you and scheduled for current iteration milestone. 

1. Pick an unassigned issue from **TODO** lane of current iteration Scrum board.
1. Leave a comment in the issue thread, stating that you are going to work on the issue. Make sure no one has left such comment already.
1. Fork the repo, create a PR, and start working.
1. Project maintainers will assign the issue to you and move it to **IN PROGRESS** lane in the Scrum board.

Before a PR can be merged, it must:

- Pass CI builds
- Have an approval from at least one peer reviewer

## Version control structure

_TBD..._


## Source code structure

_TBD..._

## Coding conventions

_TBD..._


## First-timers

Contributing to an open source project for the first time can be hard and little overwhelming. Here we attempt to make this first step easier, and provide all the information you may need.  

- If you never contributed to an open source project before, you may find the following links useful:
  - http://www.firsttimersonly.com/
  - ...
  - ...

Well, where should you start?

1. Carefully read this guideline document.
1. Read our [Roadmap](docs/Wiki/roadmap.md). Look through **Contribution Areas** section and choose areas you're interested in contributing to.
1. Start from resolving some issues, preferably those labeled  `first-timers`: 
    - Go to current interation Scrum board
    - Pick an unassigned issue from **TODO** lane, which matches your areas of interest. 
1. Proceed as explained in [Submitting changes](#submitting-changes).
1. Please feel free to communicate your thoughts and reach out for help.
