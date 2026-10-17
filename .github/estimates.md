# Estimating issues

These rules apply to everyone who estimates an issue: the team (label `sp:N`) and Claude (label `ai:N`).

## Principles

- Story points are **relative**, not hours.
- An estimate always includes: code, tests, `dotnet build` without warnings, `dotnet test` green, and review.
- Allowed values: 1, 2, 3, 5, 8, 13. When in doubt between two values, choose the higher one.
- Cannot be estimated, or larger than 13: use `?` and explain what needs to be investigated or split first.

## Criteria

- **Size:** how many files, classes and layers (Core, ConsoleUI, Web, Tests).
- **Complexity:** new logic, or only changes to existing logic.
- **Uncertainty:** is it clear what needs to be done and how.
- **Risk:** does it touch the invariants in CLAUDE.md (history as the only state, fairness, rules only in the outcome matrix).

## Scale

### 1: trivial
- One file, no new logic, no design decision.
- Existing tests are sufficient, or one test changes.
- No uncertainty, no risk.
- Example: change a console text, update the README, replace a magic number with a named constant.

### 2: small
- Change one class, within one layer.
- A few new tests.
- The approach is clear.
- Example: accept `r`, `p`, `s` as shorthand input; tighten validation of the number of rounds.

### 3: contained
- One new class or interface implementation, within one layer.
- Its own test class.
- The spec is clear, only small design decisions.
- Example: `RandomComputerPlayer`; one new predictor, such as "player repeats their previous move".

### 5: medium
- Several classes, or changes in two layers (e.g. Core and ConsoleUI).
- Tests at several levels, possibly with fakes.
- Some design decisions that need to be defended.
- May touch an invariant, so an extra test is needed.
- Example: `PredictingComputerPlayer` with an exploration rate; letting the player choose an opponent in the console.

### 8: large
- A new project, or several layers at once.
- Clear uncertainty: the approach still needs to be partly investigated or agreed on.
- DESIGN.md needs to change.
- Example: Blazor web version on the same Core; the fairness test across all predictors.

### 13: too large to build as is
- Much is unknown, affects the architecture.
- A signal to split the issue before building starts.
- Example: online multiplayer over the network; storing history in a database.
