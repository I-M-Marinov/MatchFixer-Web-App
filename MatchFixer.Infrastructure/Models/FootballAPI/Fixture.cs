using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchFixer.Infrastructure.Models.FootballAPI
{
	public class Fixture
	{
		public int Id { get; set; }
		public DateTimeOffset Date { get; set; }
	}
}
