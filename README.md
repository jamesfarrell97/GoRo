## Repository Guidelines

The following guidelines should be followed while working on this repository

### Branch Guidelines

Branches should conform to the following guidelines:

- Branches should only focus on creating, updating or removing one feature or game element at a time
- Branches should be named according to the feature that they are creating, updating, or removing - `e.g. environment_update`

### Commit Guidelines

Commits should conform to the following guidelines:

- Each commit should only focus on completing a single task
- Each commit should closely follow the outlined commit message format
- Each file or folder should be given a descriptive name that easily identifies their purpose

### Semantic Commit Message Format

Each commit message consists of a header, a body and a footer. The header has a special format that includes a type and a subject:

`<type>(<optional scope>): <subject> <BLANK LINE> <optional body> // This may span multiple lines <BLANK LINE> <optional footer>`

***Note: The following guidelines are inspired by the [Angular commit message guidelines](https://github.com/angular/angular/blob/22b96b9/CONTRIBUTING.md#-commit-message-guidelines)***

#### Type

Type must be one of the following:

- ***deps***: Changes that add external packages or dependencies\ 
- ***build***: Changes that affect the project build, such as project settings, project version, etc.\ 
- ***world***: Changes that affect the world, such as adding models, animations, sounds, or particle effects\ 
- ***ui***: Changes that affect the user interface, such as adding menu's or UI elements\ 
- ***docs***: Changes affecting documentation, such as the README and TODO files\ 
- ***asset***: A new asset, such as an imported model, sound, or animation\ 
- ***feat***: A new feature, such as integrating VR controls, or adding/updating mechanics\ 
- ***fix***: A bug fix (should include reference to GitHub issue)\ 
- ***perf***: A change which improves the performance of some feature\ 
- ***refactor***: A code change that neither fixes a bug nor adds a feature\ 
- ***style***: A code change which does not affect its meaning (white-space, formatting, missing semi-colons, etc)\ 
- ***test***: Adding missing tests or correcting existing tests\ 

#### Scope

***Note:*** Optional

A scope MAY be provided after a type. A scope MUST consist of a noun describing a section of the codebase surrounded by parenthesis - `e.g. build (level 1)`:

#### Subject

The subject contains a succinct description of the change:

- use the imperative, present tense - think of it as issuing an instruction: "change" not "changed", "create" not "creates" 
- don't capitalize the first letter 
- no full stop (.) at the end

#### Body

Just as in the subject, use the imperative, present tense: "change" not "changed" nor "changes". The body should include the motivation for the change and contrast this with previous behaviour

#### Footer

The footer should contain additional information about the commit, such as breaking changes, references to scrum tasks, and references to git issues and pull requests:

- A commit which introduces a game-breaking change, should include BREAKING CHANGE: in the footer, or should append a ! after the type/scope
- A commit which updates the status of a scrum task, should include `[task name] [task progress]`, where `[task progress]` is any of `in progress, to test, done`
- A commit which updates or closes a git issue or should include `issue #[number]`
- A commit which merges a pull request should include `pull request #[number]`

## Making Updates

Updates should consist of 3 steps

1. Create a branch to work on changes `git checkout -b [branch_name]`
2. Add and commit changes (using semantic message format outlined below) `git add [filename]` `git commit`
3. Push changes onto branch `git push origin [branch_name]`

### Creating Pull Requests

Once a branch has been pushed onto the repository, a pull request should be created by the branch developer

1. Name the pull request according to the branch name `environment_update` becomes `Environment Update`
2. Assign at least one other developer to review your request
3. Address any issues that have been identified by the reviewer

### Approving Pull Requests

Pull requests should only be approved based upon the following criteria

- The branch conforms with the outlined branch guidelines
- Each commit conforms with the outlined commit guidelines
- The branch provides useful functionality that adds to the project
- The branch has been tested to ensure that the added functionality works as intended
- The branch has been tested to ensure that it does not break other sections of the project

### Merging Branches

Approved branches should be merged in order of their creation onto the master branch

1. Checkout the master branch `git checkout master`
2. Fetch remote updates `git fetch --all`
3. Update local branch `git pull origin master`
3. Merge branch into master `git pull origin [branch_name]`
4. Resolve merge conflicts
5. Add and commit changes `git add [filename]` `git commit`
6. Push changes to master `git push origin master`

## Notes

Notes and other additional information relevant to this project's development

### Acknowledgements

- Thank you to james-dowell at WeMakeWaves who provided us with permission to adapt their PyRow code which helped us develop a communication interface between our game and the Concept2 Performance Monitor.
- Thank you to Scott Hamilton and Domenico De Vivo from Concept2 who provided us with support and technical insight to assist with our development.

### References

- This README.md is heavily inspired by a README from a project that James worked on while taking part in a work placement.
- The environment assets in this repository are taken directly from the BoatAttack repository by Unity-Technologies.
