namespace MatchFixer.Common.DerbyLookup
{
	public static class DerbyLookup
	{
		public static readonly HashSet<(int, int)> DerbyMatches = new()
		{
			// Premier League
			(Math.Min(33, 50), Math.Max(33, 50)), // Manchester United vs Manchester City
			(Math.Min(33, 40), Math.Max(33, 40)), // Manchester United vs Liverpool
			(Math.Min(49, 50), Math.Max(49, 50)), // Chelsea vs Manchester City
			(Math.Min(42, 47), Math.Max(42, 47)), // Arsenal vs Tottenham
			(Math.Min(39, 48), Math.Max(39, 48)), // Wolves vs West Ham (London derby)
			(Math.Min(44, 45), Math.Max(44, 45)), // Burnley vs Everton (Northwest derby)
			(Math.Min(34, 39), Math.Max(34, 39)), // Newcastle vs Wolves

			// La Liga
			(Math.Min(529, 541), Math.Max(529, 541)), // Barcelona vs Real Madrid (El Clasico)
			(Math.Min(529, 530), Math.Max(529, 530)), // Barcelona vs Atletico Madrid
			(Math.Min(541, 530), Math.Max(541, 530)), // Real Madrid vs Atletico Madrid (Madrid derby)
			(Math.Min(547, 548), Math.Max(547, 548)), // Girona vs Real Sociedad

			// Bundesliga
			(Math.Min(157, 165), Math.Max(157, 165)), // Bayern Munich vs Borussia Dortmund (Der Klassiker)
			(Math.Min(165, 163), Math.Max(165, 163)), // Borussia Dortmund vs Mönchengladbach (Rhein derby)
			(Math.Min(169, 173), Math.Max(169, 173)), // Eintracht Frankfurt vs RB Leipzig

			// Serie A
			(Math.Min(489, 505), Math.Max(489, 505)), // AC Milan vs Inter Milan (Derby della Madonnina)
			(Math.Min(489, 496), Math.Max(489, 496)), // AC Milan vs Juventus
			(Math.Min(496, 487), Math.Max(496, 487)), // Juventus vs Lazio
			(Math.Min(496, 505), Math.Max(496, 505)), // Juventus vs Inter Milan 
			(Math.Min(497, 496), Math.Max(497, 496)), // AS Roma vs Juventus
			(Math.Min(497, 488), Math.Max(497, 488)), // AS Roma vs Sassuolo (Derby del Triveneto)

			// Ligue 1
			(Math.Min(85, 81), Math.Max(85, 81)), // PSG vs Marseille (Le Classique)
			(Math.Min(91, 84), Math.Max(91, 84)), // Monaco vs Nice (Côte d'Azur Derby)
			(Math.Min(80, 79), Math.Max(80, 79)), // Lyon vs Lille

			// Eredivisie
			(Math.Min(194, 209), Math.Max(194, 209)), // Ajax vs Feyenoord (De Klassieker)
			(Math.Min(197, 201), Math.Max(197, 201)), // PSV vs AZ Alkmaar

			// Liga Portugal
			(Math.Min(212, 211), Math.Max(212, 211)), // FC Porto vs Benfica (O Clássico)
			(Math.Min(211, 228), Math.Max(211, 228)), // Benfica vs Sporting CP (Lisbon Derby)
			(Math.Min(228, 212), Math.Max(228, 212)), // Sporting CP vs FC Porto

			// Polish Ekstraklasa
			(Math.Min(347, 339), Math.Max(347, 339)), // Lech Poznań vs Legia Warszawa
			(Math.Min(340, 336), Math.Max(340, 336)), // Górnik Zabrze vs Jagiellonia

			// Swiss Super League
			(Math.Min(565, 1013), Math.Max(565, 1013)), // Young Boys vs Grasshoppers
			(Math.Min(1011, 783), Math.Max(1011, 783)), // FC St. Gallen vs FC Zurich

			// Parva Liga (Bulgaria)
			(Math.Min(853, 646), Math.Max(853, 646)),   // CSKA Sofia vs Levski Sofia (Eternal Derby)
			(Math.Min(853, 566), Math.Max(853, 566)),   // CSKA Sofia vs Ludogorets (title rivalry)
			(Math.Min(646, 566), Math.Max(646, 566)),   // Levski Sofia vs Ludogorets
			(Math.Min(646, 854), Math.Max(646, 854)),   // Levski Sofia vs Slavia Sofia (Old Sofia Derby)
			(Math.Min(853, 854), Math.Max(853, 854)),   // CSKA Sofia vs Slavia Sofia (Sofia Derby)
			(Math.Min(634, 858), Math.Max(634, 858)),   // Botev Plovdiv vs Lokomotiv Plovdiv (Plovdiv Derby)
			(Math.Min(566, 634), Math.Max(566, 634)),   // Ludogorets vs Botev Plovdiv
			(Math.Min(566, 858), Math.Max(566, 858)),   // Ludogorets vs Lokomotiv Plovdiv
			(Math.Min(646, 634), Math.Max(646, 634)),   // Levski Sofia vs Botev Plovdiv
			(Math.Min(646, 858), Math.Max(646, 858)),   // Levski Sofia vs Lokomotiv Plovdiv
			(Math.Min(853, 634), Math.Max(853, 634)),   // CSKA Sofia vs Botev Plovdiv
			(Math.Min(853, 858), Math.Max(853, 858)),   // CSKA Sofia vs Lokomotiv Plovdiv

		};


		public static bool IsDerby(int teamAId, int teamBId)
		{
			var key = (Math.Min(teamAId, teamBId), Math.Max(teamAId, teamBId));
			return DerbyMatches.Contains(key);
		}
	}

}
