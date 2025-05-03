using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchFixer.Infrastructure.Models.FootballAPI
{
	public class FixtureData
	{
		public Fixture Fixture { get; set; }
		public League League { get; set; }
		public Teams Teams { get; set; }
		public Goals Goals { get; set; }
	}
}
