
## Making Updates

Updates should consist of 3 steps.

1. Create a branch to work on changes `git checkout -b [branch name]`
2. Add and commit changes (using semantic message format outlined below) `git add [filename]` `git commit -m`
3. Push changes onto branch `git push origin [branch name]`

### Merging Branches

Branches should be merged in order of creation onto the master branch

1. Checkout the master branch `git checkout master`
2. Fetch remote updates `git fetch --all`
3. Merge branch into master `git pull origin [branch name]`
4. Resolve merge conflicts
5. Add and commit changes `git add [filename]` `git commit -m`
6. Push changes to master `git push origin master`

### Semantic Commit Message Format

Each commit message consists of a header, a body and a footer. The header has a special format that includes a type and a subject:

`<type>(<optional scope>): <subject> <BLANK LINE> <optional body> // This may span multiple lines <BLANK LINE> <optional footer>`

***Note: The following guidelines are inspired by the [Angular commit message guidelines](https://github.com/angular/angular/blob/22b96b9/CONTRIBUTING.md#-commit-message-guidelines)***

#### Type

Type must be one of the following:

- ***deps***: Changes that add external packages or dependencies\
- ***build***: Changes that affects the world such as adding animations, models, or UI elements\ 
- ***ci***: Changes to our CI configuration files and scripts\ 
- ***docs***: Changes affecting documentation, such as the README and TODO files\ 
- ***feat***: A new feature, such as integrating VR controls, or adding/updating mechanics\ 
- ***fix***: A bug fix (should include reference to GitHub issue)\ 
- ***perf***: A change which improves in-game performance\ 
- ***refactor***: A code change that neither fixes a bug nor adds a feature\ 
- ***style***: A code change which does not affect its meaning (white-space, formatting, missing semi-colons, etc)\ 
- ***test***: Adding missing tests or correcting existing tests\

### Scope

***Note:*** Optional

A scope MAY be provided after a type. A scope MUST consist of a noun describing a section of the codebase surrounded by parenthesis, e.g., build (level 1):

#### Subject

The subject contains a succinct description of the change:

- use the imperative, present tense: "change" not "changed" nor "changes" 
- don't capitalize the first letter 
- no fullstop (.) at the end

#### Body

Just as in the subject, use the imperative, present tense: "change" not "changed" nor "changes". The body should include the motivation for the change and contrast this with previous behavior.

#### Footer

The footer should contain any information about Breaking Changes and is also the place to reference git issues that this commit Closes.

A commit that has a footer BREAKING CHANGE:, or appends a ! after the type/scope, introduces a game breaking change (correlating with MAJOR in semantic versioning).
