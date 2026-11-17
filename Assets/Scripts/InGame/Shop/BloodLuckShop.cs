using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BloodLuck
{
    // The wave manager decides when to open the shop.
    public class BloodLuckShop : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Button[] cardButtons = new Button[3];
        [SerializeField] private Button diceButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private InfusionCardView[] cardViews = new InfusionCardView[3];
        [SerializeField] private ShopWalletView walletView;
        [SerializeField] private BloodyDiceRoll diceRoll;
        [SerializeField] private GameObject diceOverlay;
        [SerializeField] private Button diceRollButton;
        [SerializeField] private Button diceReturnButton;
        [SerializeField] private TMP_Text diceCostLabel;

        [Header("Shop Data")]
        [Tooltip("Only for the standalone Shop_Prototype scene. In-game the wallet is passed to OpenShopAsync.")]
        [SerializeField] private SlotPullWallet wallet;
        [SerializeField] private List<InfusionDefinition> infusionPool = new List<InfusionDefinition>();
        [Tooltip("HP charged for each cost unit of an infusion.")]
        [SerializeField, Min(1)] private int hpPerCostUnit = 10;

        private const int DiceCostUnits = 2;

        private IShopWallet _wallet;

        // Future combat/visual systems can listen without changing shop logic.
        public event Action<InfusionDefinition> Purchased;
        public event Action Closed;

        private readonly List<InfusionDefinition> offers = new List<InfusionDefinition>();
        private readonly HashSet<InfusionDefinition> owned = new HashSet<InfusionDefinition>();
        private bool diceRolling;
        private bool diceHasRolled;
        private bool diceRewardGranted;
        private InfusionDefinition pendingDiceReward;

        private void Awake()
        {
            if (shopPanel == null || diceButton == null ||
                cardButtons == null || cardButtons.Length != 3)
            {
                Debug.LogError("BloodLuckShop: Assign the shop panel, dice button, and exactly three card buttons.", this);
                enabled = false;
                return;
            }

            foreach (var button in cardButtons)
            {
                if (button != null) continue;
                Debug.LogError("BloodLuckShop: A card button is missing.", this);
                enabled = false;
                return;
            }

            shopPanel.SetActive(false);
            for (int i = 0; i < cardButtons.Length; i++)
            {
                int slot = i;
                cardButtons[i].onClick.AddListener(() => Buy(slot));
            }
            diceButton.onClick.AddListener(BuyDice);
            if (leaveButton != null) leaveButton.onClick.AddListener(CloseShop);
            if (diceRollButton != null) diceRollButton.onClick.AddListener(RollPreparedDice);
            if (diceReturnButton != null) diceReturnButton.onClick.AddListener(ReturnFromDice);
            if (wallet != null) BindWallet(wallet);
            if (diceOverlay != null) diceOverlay.SetActive(false);
        }

        private void BindWallet(IShopWallet source)
        {
            _wallet = source;
            if (walletView != null) walletView.Bind(source);
        }

        /// <summary>Opens the shop against the given wallet and completes when the player leaves it.</summary>
        public async UniTask OpenShopAsync(IShopWallet source, CancellationToken ct)
        {
            BindWallet(source);
            var closed = new UniTaskCompletionSource();
            void OnClosed() => closed.TrySetResult();
            Closed += OnClosed;
            try
            {
                OpenShop();
                if (!shopPanel.activeSelf) return; // failed to open (disabled)
                await closed.Task.AttachExternalCancellation(ct);
            }
            finally
            {
                Closed -= OnClosed;
            }
        }

        public void OpenShop()
        {
            if (!enabled || _wallet == null || shopPanel.activeSelf || diceRolling) return;
            offers.Clear();
            pendingDiceReward = null;
            diceRolling = false;
            diceHasRolled = false;
            diceRewardGranted = false;
            var available = AvailableInfusions();
            while (offers.Count < cardButtons.Length && available.Count > 0)
            {
                int selected = WeightedPick(available);
                offers.Add(available[selected]);
                available.RemoveAt(selected);
            }
            shopPanel.SetActive(true);
            AudioManager.Instance?.PlayShopMusic();
            RefreshButtons();
        }

        public void CloseShop()
        {
            if (!enabled || !shopPanel.activeSelf || diceRolling) return;
            shopPanel.SetActive(false);
            AudioManager.Instance?.PlayGameplayLoop();
            Closed?.Invoke();
        }

        public void ResetForRun()
        {
            owned.Clear();
            offers.Clear();
            pendingDiceReward = null;
            diceRolling = false;
            diceHasRolled = false;
            diceRewardGranted = false;
            if (diceOverlay != null) diceOverlay.SetActive(false);
            shopPanel.SetActive(false);
        }

        private List<InfusionDefinition> AvailableInfusions()
        {
            var result = new List<InfusionDefinition>();
            foreach (var infusion in infusionPool)
                if (infusion != null && (infusion.Stackable || !owned.Contains(infusion)) && !result.Contains(infusion))
                    result.Add(infusion);
            return result;
        }

        // Simple rarity weighting: Common 5, Epic 2, Mythic 1.
        private int WeightedPick(List<InfusionDefinition> pool)
        {
            int total = 0;
            foreach (var item in pool) total += Weight(item.rarity);
            int roll = UnityEngine.Random.Range(0, total);
            for (int i = 0; i < pool.Count; i++)
            {
                roll -= Weight(pool[i].rarity);
                if (roll < 0) return i;
            }
            return pool.Count - 1;
        }

        private int Weight(InfusionRarity rarity) =>
            rarity == InfusionRarity.Bloodstained ? 5 :
            rarity == InfusionRarity.Bloodbound ? 2 : 1;

        private int HpCost(InfusionDefinition infusion) => infusion.Cost * hpPerCostUnit;

        private int DiceHpCost => DiceCostUnits * hpPerCostUnit;

        public void Buy(int slot)
        {
            if (diceRolling || !shopPanel.activeSelf || slot < 0 || slot >= offers.Count || offers[slot] == null) return;
            var infusion = offers[slot];
            if (!_wallet.TrySpend(HpCost(infusion))) return;
            AudioManager.Instance?.PlayPurchase();
            Grant(infusion);
            offers[slot] = null;
            RefreshButtons();
        }

        public void BuyDice()
        {
            if (diceRolling || !shopPanel.activeSelf) return;
            var mythics = AvailableInfusions().FindAll(x => x.rarity == InfusionRarity.Bloodwrought);
            if (mythics.Count == 0 || !_wallet.TrySpend(DiceHpCost)) return;
            AudioManager.Instance?.PlayPurchase();

            var reward = mythics[UnityEngine.Random.Range(0, mythics.Count)];
            diceRolling = true;
            diceHasRolled = false;
            diceRewardGranted = false;
            pendingDiceReward = reward;
            RefreshButtons();
            if (diceOverlay != null)
            {
                diceOverlay.SetActive(true);
                if (diceRoll != null) diceRoll.ShowDice();
                if (diceRollButton != null) diceRollButton.interactable = true;
                if (diceReturnButton != null) diceReturnButton.interactable = false;
            }
            else
            {
                // Safe fallback for scenes that have not yet added the optional overlay UI.
                RollPreparedDice();
            }
        }

        private void RollPreparedDice()
        {
            if (!diceRolling || diceHasRolled || pendingDiceReward == null) return;
            diceHasRolled = true;
            if (diceRollButton != null) diceRollButton.interactable = false;
            if (diceRoll == null) { CompleteDicePurchase(); return; }
            diceRoll.Roll(pendingDiceReward, CompleteDicePurchase, false);
        }

        private void CompleteDicePurchase()
        {
            if (pendingDiceReward == null || diceRewardGranted) return;
            diceRewardGranted = true;
            Grant(pendingDiceReward);
            for (int i = 0; i < offers.Count; i++)
                if (offers[i] == pendingDiceReward) offers[i] = null;
            if (diceOverlay != null && diceReturnButton != null) diceReturnButton.interactable = true;
            else ReturnFromDice();
        }

        private void ReturnFromDice()
        {
            if (!diceRewardGranted || pendingDiceReward == null) return;
            if (diceOverlay != null) diceOverlay.SetActive(false);
            if (diceRoll != null) diceRoll.HideDice();
            pendingDiceReward = null;
            diceRolling = false;
            diceHasRolled = false;
            diceRewardGranted = false;
            RefreshButtons();
        }

        private void Grant(InfusionDefinition infusion)
        {
            if (!infusion.Stackable) owned.Add(infusion);
            Purchased?.Invoke(infusion);
        }

        private void RefreshButtons()
        {
            for (int i = 0; i < cardButtons.Length; i++)
            {
                var item = i < offers.Count ? offers[i] : null;
                cardButtons[i].interactable = !diceRolling && item != null && _wallet.CanSpend(HpCost(item));
                if (cardViews != null && i < cardViews.Length && cardViews[i] != null)
                    cardViews[i].SetInfusion(item, item != null ? HpCost(item) : 0);
            }
            if (diceCostLabel != null) diceCostLabel.text = DiceHpCost + " HP";
            diceButton.interactable = !diceRolling && _wallet.CanSpend(DiceHpCost) &&
                AvailableInfusions().Exists(x => x.rarity == InfusionRarity.Bloodwrought);
            if (leaveButton != null) leaveButton.interactable = !diceRolling;
        }

        public InfusionDefinition GetOffer(int slot) =>
            slot >= 0 && slot < offers.Count ? offers[slot] : null;
    }
}
