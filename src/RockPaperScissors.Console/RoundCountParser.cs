using System.Globalization;
using RockPaperScissors.Core;

namespace RockPaperScissors.Console;

public static class RoundCountParser
{
    /// <summary>Accepts a whole number within the range <see cref="GameSession"/> allows.</summary>
    public static bool TryParse(string? input, out int rounds)
    {
        if (int.TryParse(input?.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out rounds)
            && rounds >= GameSession.MinRounds
            && rounds <= GameSession.MaxRounds)
        {
            return true;
        }

        rounds = 0;
        return false;
    }
}
