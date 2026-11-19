using UnityEngine;

namespace ShinobiLegacy.Items
{
    public sealed class PlayerWallet : MonoBehaviour
    {
        [SerializeField] private int startingCoins = 500;
        public int Coins { get; private set; }
        private void Awake() { Coins = Mathf.Max(0, startingCoins); }
        public bool Spend(int amount)
        {
            if (amount < 0 || Coins < amount) return false;
            Coins -= amount;
            return true;
        }
        public void Add(int amount) { if (amount > 0) Coins += amount; }
    }
}