using Microsoft.EntityFrameworkCore;

using MatchFixer.Core.Contracts;
using MatchFixer.Infrastructure.Entities;
using MatchFixer.Infrastructure;

namespace MatchFixer.Core.Services
{
	public class WalletService : IWalletService
	{
		private readonly MatchFixerDbContext _dbContext;

		public WalletService(MatchFixerDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<bool> HasWalletAsync(Guid userId)
		{
			return await _dbContext.Wallets.AnyAsync(w => w.UserId == userId);
		}

		public async Task<Wallet> GetWalletAsync(Guid userId)
		{
			return await _dbContext.Wallets
				.Include(w => w.Transactions)
				.FirstOrDefaultAsync(w => w.UserId == userId);
		}

		public async Task<Wallet> CreateWalletAsync(Guid userId)
		{
			var existing = await _dbContext.Wallets
				.FirstOrDefaultAsync(w => w.UserId == userId);

			if (existing != null)
				return existing;

			var wallet = new Wallet
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				Balance = 0m,
				Currency = "EUR",
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _dbContext.Wallets.AddAsync(wallet);
			await _dbContext.SaveChangesAsync();

			return wallet;
		}
	}

}
