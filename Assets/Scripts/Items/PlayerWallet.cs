using UnityEngine;

namespace ShinobiLegacy.Items
{
    public sealed class PlayerWallet : MonoBehaviour
    {
        [SerializeField] private int startingCoins = 500;
        public int Coins { get; private set; }
        public event System.Action CoinsChanged;
        private void Awake() { Coins = Mathf.Max(0, startingCoins); }
        public void SetCoins(int value) { Coins = Mathf.Max(0, value); CoinsChanged?.Invoke(); }
        public bool Spend(int amount)
        {
            if (amount < 0 || Coins < amount) return false;
            Coins -= amount;
            CoinsChanged?.Invoke();
            return true;
        }
        public void Add(int amount) { if (amount > 0) { Coins += amount; CoinsChanged?.Invoke(); } }
    }
}