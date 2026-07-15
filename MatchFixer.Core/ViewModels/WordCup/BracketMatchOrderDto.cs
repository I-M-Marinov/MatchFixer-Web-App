using MatchFixer.Common.Enums;

namespace MatchFixer.Core.ViewModels.WordCup
{
    public record BracketMatchOrderDto(
        int Id,
        string HomeTeam,
        string AwayTeam,
        string HomeLogo,
        string AwayLogo,
        int? HomeScore,
        int? AwayScore,
        WorldCupStage Stage,
        int RoundPosition
    );
}
