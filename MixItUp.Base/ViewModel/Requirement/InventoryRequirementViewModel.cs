﻿using MixItUp.Base.ViewModel.User;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    [DataContract]
    public class InventoryRequirementViewModel : IEquatable<InventoryRequirementViewModel>
    {
        [DataMember]
        public Guid InventoryID { get; set; }

        [DataMember]
        public string ItemName { get; set; }

        [DataMember]
        public int Amount { get; set; }

        public InventoryRequirementViewModel() { }

        public InventoryRequirementViewModel(UserInventoryViewModel inventory, UserInventoryItemViewModel item, int amount)
        {
            this.InventoryID = inventory.ID;
            this.ItemName = item.Name;
            this.Amount = amount;
        }

        public UserInventoryViewModel GetInventory()
        {
            if (ChannelSession.Settings.Inventories.ContainsKey(this.InventoryID))
            {
                return ChannelSession.Settings.Inventories[this.InventoryID];
            }
            return null;
        }

        public bool TrySubtractAmount(UserDataViewModel userData) { return this.TrySubtractAmount(userData, this.Amount); }

        public bool TrySubtractMultiplierAmount(UserDataViewModel userData, int multiplier) { return this.TrySubtractAmount(userData, multiplier * this.Amount); }

        public bool TrySubtractAmount(UserDataViewModel userData, int amount)
        {
            if (this.DoesMeetRequirement(userData))
            {
                UserInventoryViewModel inventory = this.GetInventory();
                if (inventory == null)
                {
                    return false;
                }

                if (!userData.HasInventoryAmount(inventory, this.ItemName, amount))
                {
                    return false;
                }
                userData.SubtractInventoryAmount(inventory, this.ItemName, amount);
                return true;
            }
            return false;
        }

        public bool DoesMeetRequirement(UserDataViewModel userData)
        {
            if (userData.IsCurrencyRankExempt)
            {
                return true;
            }

            UserInventoryViewModel inventory = this.GetInventory();
            if (inventory == null)
            {
                return false;
            }

            if (!inventory.Items.ContainsKey(this.ItemName))
            {
                return false;
            }

            return userData.GetInventoryAmount(inventory, this.ItemName) >= this.Amount;
        }

        public async Task SendNotMetWhisper(UserViewModel user)
        {
            if (ChannelSession.Chat != null && ChannelSession.Settings.Inventories.ContainsKey(this.InventoryID))
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You do not have the required {0} {1} to do this", this.Amount, this.ItemName));
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is InventoryRequirementViewModel)
            {
                return this.Equals((InventoryRequirementViewModel)obj);
            }
            return false;
        }

        public bool Equals(InventoryRequirementViewModel other) { return this.InventoryID.Equals(other.InventoryID); }

        public override int GetHashCode() { return this.InventoryID.GetHashCode(); }
    }
}
