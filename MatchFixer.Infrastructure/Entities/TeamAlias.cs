using System.ComponentModel.DataAnnotations;


namespace MatchFixer.Infrastructure.Entities
{
	public class TeamAlias
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(150)]
		public string Alias { get; set; } = null!;

		public Guid TeamId { get; set; }
		public Team Team { get; set; } = null!;

		public string? Source { get; set; } 
	}

}
