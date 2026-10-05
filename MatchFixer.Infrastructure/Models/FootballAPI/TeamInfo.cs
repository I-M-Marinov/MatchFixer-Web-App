using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchFixer.Infrastructure.Models.FootballAPI
{
	public class TeamInfo
	{
		public string Name { get; set; }
		public string Logo { get; set; }
		// API-Football: true = this team won, false = lost, null = draw / undecided
		public bool? Winner { get; set; }
	}
}
