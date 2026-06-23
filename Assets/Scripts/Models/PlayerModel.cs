namespace DiaToMas.Models
{
    public class PlayerModel
    {
        public CurrencyWalletModel WalletModel { get; } = new();
        public PlayerInventoryModel InventoryModel { get; } = new();
    }
}
