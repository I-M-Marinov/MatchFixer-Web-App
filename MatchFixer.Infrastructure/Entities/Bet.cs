using System.ComponentModel.DataAnnotations;

namespace MatchFixer.Infrastructure.Entities
{
	public class Bet
	{
		[Key]
		public Guid Id { get; set; }

		[Required(ErrorMessage = "Amount is required.")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
		public decimal Amount { get; set; }

		[Required(ErrorMessage = "Pick is required.")]
		[RegularExpression("Home|Draw|Away", ErrorMessage = "Pick must be 'Home', 'Draw', or 'Away'.")]
		public string Pick { get; set; }

		[Required(ErrorMessage = "Bet time is required.")]
		public DateTime BetTime { get; set; }

		[Required(ErrorMessage = "User is required.")]
		public Guid UserId { get; set; }
		public ApplicationUser User { get; set; }

		[Required(ErrorMessage = "Match event is required.")]
		public Guid MatchEventId { get; set; }
		public MatchEvent MatchEvent { get; set; }
	}
}
