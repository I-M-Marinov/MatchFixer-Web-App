using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchFixer.Infrastructure.Entities
{
	public class Team
	{
		[Key]
		public Guid Id { get; set; }

		public int? TeamId { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[Required]
		[Url]
		public string LogoUrl { get; set; }

		// Navigation properties for matches
		public ICollection<MatchResult> HomeMatches { get; set; } = new List<MatchResult>();
		public ICollection<MatchResult> AwayMatches { get; set; } = new List<MatchResult>();
	}
}
