namespace MatchFixer.Core.Exceptions;

public class WalletLockedException : Exception
{
	public WalletLockedException(string? message = null)
		: base(message ?? "Your wallet is currently locked.")
	{
	}
}

