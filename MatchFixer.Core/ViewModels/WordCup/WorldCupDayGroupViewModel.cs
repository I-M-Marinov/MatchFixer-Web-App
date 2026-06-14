using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchFixer.Core.ViewModels.WordCup
{
	public class WorldCupDayGroupViewModel
	{
		public DateOnly Date { get; set; }

		public List<WorldCupMatchCardViewModel> Matches { get; set; }
			= new();
	}
}
