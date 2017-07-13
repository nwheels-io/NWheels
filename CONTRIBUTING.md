# Contribution guidelines

When contributing to this repository, please follow the guidelines below.

## Communication

There are two channels of communication:

- Slack
- Issues on GitHub

### Slack: 

- General announcements: channel `#general`
- Proposal of new ideas (see **Workflow** section below): channel `#idea`
- Overview of new technologies, especially those that can be useful for NWheels: channel `#tech`.
- Anything else: channel `#random`.

Every contributor should be subscribed to NWheels project at Slack ([nwheels.slack.com](nwheels.slack.com))

- How to subscribe: please send an email to [team@nwheels.io](mailto:team@nwheels.io), with subject `Join NWheels team`. You will receive back an email from Slack with join link and instructions. 

### Issues on GitHub

All technical communication should be done in GitHub issues (new ideas are initially discussed on Slack - explained in [Workflow](#workflow)). 

Beyond initial discussion of a new idea, every technical subject, feature, enhancement, or bugfix should be captured by an issue. Discussion thread of an issue serves focal point for suggestions, feedback, questions, clarifications, and documentation related to the issue. 

Issues are always assigned to one of milestones, as described in [Workflow](#workflow) section below.

Issues are labeled in several dimensions:

- **Issue type** - exactly one must be labeled: 
  - <div style="display:inline-block;padding:3px;border-radius:2px;background:#006b75">new feature</div>
  - <div style="display:inline-block;padding:3px;border-radius:2px;background:#006b75">enhancement</div>
  - <div style="display:inline-block;padding:3px;border-radius:2px;background:#006b75">task</div>
  - <div style="display:inline-block;padding:3px;border-radius:2px;background:#006b75">bug</div>
  - <div style="display:inline-block;padding:3px;border-radius:2px;background:#006b75">question</div>
  - <div style="display:inline-block;padding:3px;border-radius:2px;background:#006b75">epic</div>
- **Issue flags** - multiple can be labeled:
  - <div style="display:inline-block;padding:3px;border-radius:2px;background:#006b75">tech debt</div>
  - <div style="display:inline-block;padding:3px;border-radius:2px;background:#006b75">refactoring</div>
  - <div style="display:inline-block;padding:3px;border-radius:2px;background:#006b75">redesign</div>
  - <div style="display:inline-block;padding:3px;border-radius:2px;background:#006b75">duplicate</div>
  - <div style="display:inline-block;padding:3px;border-radius:2px;background:#006b75">wontfix</div>
  - <div style="display:inline-block;padding:3px;border-radius:2px;background:#006b75">invalid</div>
- **Related modules**
  - <div style="display:inline-block;padding:3px;border-radius:2px;background:#006b75">module-xxxxx</div>, <div style="display:inline-block;padding:3px;border-radius:2px;background:#006b75">adapter-xxxxx</div> designates modules  the issue is related to; multiple modules can be labeled on one issue

## Workflow

Enhancements introduced to NWheels should follow this worklfow:  

1. **Idea**: propose initial idea on Slack (`#idea` channel). 
    - If the idea was in general accepted, move next.
1. **Proposal**: create a new issue. Summarize discussion in Slack, and provide contents for further discussion. Label the issue as necessary, and assign it to `Proposals` milestone. 
    - From that moment on, all communications on the subject must go through the issue discussion thread.
    - Here the issue must be designed in more detail. Implications and corner cases must be discussed.
    - As soon as the issue is detailed enough, and positive feedback is received, maintainers will move it to `Backlog` milestone.
1. **Backlog**: here the issue is waiting to be assigned to an iteration milestone. Further discission and design are welcome at this stage. 
1. **Iteration milestone** (explained below): here the issue is actively worked on to completion. A pull request must be created, which captures changes made in context of this specific issue. The goal is to resolve the issue and merge the pull request.

### Iteration milestones

- All development is done in context of iteration milestones. 
- The purpose of iteration milestones is provide our users with timeframes for expected enhancements. 
- Iteration milestones are sequentially numbered, and added a title, which summaries milestone target. For example, "_01 First happy path_". 
- Scope-wise, milestones are planned much like iterations: issues are picked from the backlog, and assigned to the milestone. 
- Time-wise, milestones are defined a likely due date, but the date can change as we go. If a due date cannot be met, it is replaced with a new expected date.

### Special milestones

Two special milestones exist, as explained above. They are not related to iterations:

- Backlog
- Proposals

### Scrum board

We use GitHub _Project Boards_ feature to create Scrum boards

- Read [GitHub Help on Project Boards](https://help.github.com/articles/about-project-boards/#)

Every iteration milestone has an associated Scrum board, which visualizes milestone progress. Scrum boards are named the same as their associated milestones.

Scrum board has the following lanes:

- TODO: issues planned for the milestone
- IN PROGRESS:
- On Hold:
- DONE:

Every contributor is required to update the Scrum board as necessary, when she starts or stops working on an issue.

## Submitting changes

All changes should be submitted through pull requests (PRs). 
- Read [GitHub help on Pull Requests](https://help.github.com/categories/collaborating-with-issues-and-pull-requests/)

We encourage submitting work-in-progress PRs, so that the community can review and react early. 

- Please prefix names of  work-in-progress PRs with "_WIP_" - for example, "_WIP - listen on 0.0.0.0 instead of localhost_". We will know that it's an ongoing effort and give feedback accordingly.
- Once you consider the PR ready, please remove the "_WIP_" prefix from its name. We will treat it as a candidate to merge. 

Before a PR can be merged, it must:

- Pass CI builds
- Have an approval from at least one reviewer






## First-timers

Contributing to an open source project for the first time can be hard and little overwhelming. Here we attempt to make this first step easier, and provide all the information you may need.  

A good point to start is looking for issues labeled `first-timers`.  

If you never contributed to an open source project before, you may find the following links useful:

- http://www.firsttimersonly.com/
- ...
- ...

## Choosing what to work on

If you don't know what to work on, please go to the current Project (it is a Scrum board), and pick an issue from the `TODO` column.

We use message thread of the issue for all related communication. Please make sure you pass through this thread first. 

If you decide to start working on an issue, please leave a comment stating that. Make sure no one left such comment already. We will then assign the issue to you.  

## Proposing a new change

If you have a new idea for which you can't find an existing issue, you are welcome to suggest it. 

Start by discussing the idea with us on Slack at nwheels.slack.com. 

- If you aren't yet member of our NWheels team on Slack, please send and email to felixberman76@gmail.com, stating you'd like to join. We will email back an invitation.

## Coding Standard

## Code Review

## Code of Conduct

Please note we have a code of conduct, please follow it in all your interactions with the project.