namespace RockPaperScissors.Core;

/// <param name="Create">
/// Builds a fresh computer player for one game. The interface passes in the Random,
/// so Core never creates one and tests can pass a seeded one.
/// </param>
public sealed record OpponentOption(string Name, string Description, Func<Random, IComputerPlayer> Create);
