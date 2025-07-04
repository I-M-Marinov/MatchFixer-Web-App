﻿using MatchFixer.Common.Enums;

namespace MatchFixer.Core.ViewModels.Wallet
{
	public class WalletTransactionViewModel
	{
		public DateTime CreatedAt { get; set; }
		public decimal Amount { get; set; }
		public string DisplayTime { get; set; }
		public WalletTransactionType TransactionType { get; set; }
		public string Description { get; set; }
		public DateTime? ClearedHistoryTime { get; set; }
	}
}
