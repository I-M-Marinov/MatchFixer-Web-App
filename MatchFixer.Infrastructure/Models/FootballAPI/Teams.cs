using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchFixer.Infrastructure.Models.FootballAPI
{
	public class Teams
	{
		public TeamInfo Home { get; set; }
		public TeamInfo Away { get; set; }
	}
}
