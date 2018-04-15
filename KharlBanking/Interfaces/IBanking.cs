namespace KharlBanking.Interfaces
{
    public interface IBanking
    {
		string AccountNumber { get; set; }
		string AccountName { get; set; }
		decimal Balance { get; set; }
		decimal Amount { get; set; }
		byte[] RowVersion { get; set; }
	}
}
