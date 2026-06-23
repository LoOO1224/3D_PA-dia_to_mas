using System.Collections.Generic;

namespace DiaToMas.Models
{
    public class CurrencyWalletModel
    {
        private readonly Dictionary<string, int> _amountByCurrencyId = new();

        public IReadOnlyDictionary<string, int> AmountByCurrencyId => _amountByCurrencyId;

        public void SetAmount(string currencyId, int amount)
        {
            _amountByCurrencyId[currencyId] = amount < 0 ? 0 : amount;
        }

        public void AddAmount(string currencyId, int amount)
        {
            int currentAmount = GetAmount(currencyId);
            SetAmount(currencyId, currentAmount + amount);
        }

        public bool CanSpend(string currencyId, int amount)
        {
            return amount >= 0 && GetAmount(currencyId) >= amount;
        }

        public bool TrySpend(string currencyId, int amount)
        {
            if (!CanSpend(currencyId, amount))
            {
                return false;
            }

            SetAmount(currencyId, GetAmount(currencyId) - amount);
            return true;
        }

        public int GetAmount(string currencyId)
        {
            return _amountByCurrencyId.TryGetValue(currencyId, out int amount) ? amount : 0;
        }
    }
}
