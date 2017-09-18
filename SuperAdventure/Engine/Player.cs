using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;

namespace Engine
{
    public class Player : LivingCreature
    {
        private int _gold;
        private int _experiencePoints;
        private Location _currentLocation;
        private Weapon _currentWeapon;
        private Monster _currentMonster;

        public event EventHandler<MessageEventArgs> OnMessage;

        public int Gold
        {
            get { return _gold; }
            set
            {
                _gold = value;
                OnPropertyChanged("Gold");
            }
        }

        public int ExperiencePoints
        {
            get { return _experiencePoints; }
            private set
            {
                _experiencePoints = value;
                OnPropertyChanged("ExperiencePoints");
                OnPropertyChanged("Level");
            }
        }
        public int Level { get { return (ExperiencePoints / 100 + 1); } }

        public Location CurrentLocation
        {
            get { return _currentLocation; }
            set
            {
                _currentLocation = value;
                OnPropertyChanged("CurrentLocation");
            }
        }

        public Weapon CurrentWeapon
        {
            get { return _currentWeapon; }
            set
            {
                _currentWeapon = value;
                OnPropertyChanged("CurrentWeapon");
            }
        }

        public BindingList<InventoryItem> Inventory { get; set; }
        public List<Weapon> Weapons
        {
            get { return Inventory.Where(x => x.Details is Weapon).Select(x => x.Details as Weapon).ToList(); }
        }
        public List<HealingPotion> Potions
        {
            get { return Inventory.Where(x => x.Details is HealingPotion).Select(x => x.Details as HealingPotion).ToList(); }
        }
        public BindingList<PlayerQuest> Quests { get; set; }

        private Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints) : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;

            Inventory = new BindingList<InventoryItem>();
            Quests = new BindingList<PlayerQuest>();
        }

        public static Player CreateDefaultPlayer()
        {
            Player player = new Player(10, 10, 20, 0);
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            player.MoveHome();

            return player;
        }

        public static Player CreatePlayerFromXmlString(string xmlPlayerData)
        {
            try
            {
                XmlDocument playerData = new XmlDocument();

                playerData.LoadXml(xmlPlayerData);

                int currentHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
                int maximumHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
                int gold = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
                int experiencePoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);

                Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints);

                int currentLocationID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
                player.CurrentLocation = World.LocationByID(currentLocationID);

                if (playerData.SelectSingleNode("/Player/Stats/CurrentWeapon") != null)
                {
                    int currentWeaponID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon").InnerText);
                    player.CurrentWeapon = (Weapon)World.ItemByID(currentWeaponID);
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/InventoryItems/InventoryItem"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

                    for (int i = 0; i < quantity; i++)
                    {
                        player.AddItemToInventory(World.ItemByID(id));
                    }
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);

                    PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(id));
                    playerQuest.IsCompleted = isCompleted;

                    player.Quests.Add(playerQuest);
                }

                return player;
            }
            catch
            {
                // If there was an error with the XML data, return a default player object
                return CreateDefaultPlayer();
            }
        }

        public static Player CreatePlayerFromDatabase(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints, int currentLocationID, int currentWeaponID)
        {
            Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints);

            player.CurrentWeapon = (Weapon)World.ItemByID(currentWeaponID);

            player.MoveTo(World.LocationByID(currentLocationID));

            return player;
        }

        public string ToXmlString()
        {
            XmlDocument playerData = new XmlDocument();

            // Create the top-level XML node
            XmlNode player = playerData.CreateElement("Player");
            playerData.AppendChild(player);

            // Create the "Stats" child node to hold the other player statistics nodes
            XmlNode stats = playerData.CreateElement("Stats");
            player.AppendChild(stats);

            // Create the child nodes for the "Stats" node
            CreateNewChildXmlNode(playerData, stats, "CurrentHitPoints", CurrentHitPoints);
            CreateNewChildXmlNode(playerData, stats, "MaximumHitPoints", MaximumHitPoints);
            CreateNewChildXmlNode(playerData, stats, "Gold", Gold);
            CreateNewChildXmlNode(playerData, stats, "ExperiencePoints", ExperiencePoints);
            CreateNewChildXmlNode(playerData, stats, "CurrentLocation", CurrentLocation.ID);

            if (CurrentWeapon != null)
            {
                CreateNewChildXmlNode(playerData, stats, "CurrentWeapon", CurrentWeapon.ID);
            }

            // Create the "InventoryItems" child node to hold each InventoryItem node
            XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
            player.AppendChild(inventoryItems);

            // Create an "InventoryItem" node for each item in the player's inventory
            foreach (InventoryItem item in Inventory)
            {
                XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

                AddXmlAttributeToNode(playerData, inventoryItem, "ID", item.Details.ID);
                AddXmlAttributeToNode(playerData, inventoryItem, "Quantity", item.Quantity);

                inventoryItems.AppendChild(inventoryItem);
            }

            // Create the "PlayerQuests" child node to hold each PlayerQuest node
            XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
            player.AppendChild(playerQuests);

            // Create a "PlayerQuest" node for each quest the player has acquired
            foreach (PlayerQuest quest in Quests)
            {
                XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

                AddXmlAttributeToNode(playerData, playerQuest, "ID", quest.Details.ID);
                AddXmlAttributeToNode(playerData, playerQuest, "IsCompleted", quest.IsCompleted);

                playerQuests.AppendChild(playerQuest);
            }

            return playerData.InnerXml; // The XML document, as a string, so we can save the data to disk
        }


        public void AddExperiencePoints(int experiencePointsToAdd)
        {
            ExperiencePoints += experiencePointsToAdd;
            MaximumHitPoints = (Level * 10);
        }

        public bool PlayerHasRequiredItemToEnterThisLocation(Location location)
        {
            if (location.ItemRequiredToEnter == null)
            {
                // There is no required item for this location, so return "true"
                return true;
            }

            // See if the player has the required item in their inventory
            return Inventory.Any(ii => ii.Details.ID == location.ItemRequiredToEnter.ID);
        }

        public bool PlayerHasThisQuest(Quest quest)
        {
            return Quests.Any(pq => pq.Details.ID == quest.ID);
        }

        public bool PlayerNotCompletedThisQuest(Quest quest)
        {
            return Quests.Any(pq => pq.Details.ID == quest.ID && !pq.IsCompleted);
        }

        public bool PlayerHasAllQuestCompletionItems(Quest quest)
        {
            // See if the player has all the items needed to complete the quest here
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                if (!Inventory.Any(ii => ii.Details.ID == qci.Details.ID && ii.Quantity >= qci.Quantity))
                {
                    return false;
                }
            }
            // If we got here, then the player must have all the required items, and enough of them, to complete the quest.
            return true;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                // Subtract the quantity from the player's inventory that was needed to complete the quest
                InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == qci.Details.ID);

                if (item != null)
                {
                    RemoveItemFromInventory(item.Details, qci.Quantity);
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd, int quantity = 1)
        {
            InventoryItem existingItemInInventory = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

            if (existingItemInInventory == null)
            {
                // They didn't have the item, so add it to their inventory
                Inventory.Add(new InventoryItem(itemToAdd, quantity));
            }
            else
            {
                // They have the item in their inventory, so increase the quantity
                existingItemInInventory.Quantity += quantity;
            }

            RaiseInventoryChangedEvent(itemToAdd);
        }

        public void RemoveItemFromInventory(Item itemToRemove, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToRemove.ID);

            if (item == null)
            {
                // The item is not in the player's inventory, so ignore it.
                // We might want to raise an error for this situation
            }
            else
            {
                // They have the item in their inventory, so decrease the quantity
                item.Quantity -= quantity;

                // If the quantity is zero, remove the item from the list
                if (item.Quantity <= 0)
                {
                    Inventory.Remove(item);
                }

                // Notify the UI that the inventory has changed
                RaiseInventoryChangedEvent(itemToRemove);
            }
        }

        private void RaiseInventoryChangedEvent(Item item)
        {
            if (item is Weapon)
            {
                OnPropertyChanged("Weapons");
            }

            if (item is HealingPotion)
            {
                OnPropertyChanged("Potions");
            }
        }

        public void MarkQuestCompleted(Quest quest)
        {
            // Find the quest in the player's quest list
            PlayerQuest playerQuest = Quests.SingleOrDefault(pq => pq.Details.ID == quest.ID);

            if (playerQuest != null)
            {
                // Mark it as completed
                playerQuest.IsCompleted = true;
            }
        }

        public void MoveTo(Location location)
        {
            //Does the location have any required items
            if (!PlayerHasRequiredItemToEnterThisLocation(location))
            {
                RaiseMessage("You must have a " + location.ItemRequiredToEnter.Name + " to enter this location.", true);
                return;
            }

            // Update the player's current location
            CurrentLocation = location;

            // Completely heal the player
            CompletelyHeal();

            // Does the location have a quest?
            if (location.IsQuestAvailableHere)
            {
                // See if the player already has the quest
                if (PlayerHasThisQuest(location.QuestAvailableHere))
                {
                    // If the player has not completed the quest yet, and
                    // The player has all items required to complete the quest
                    if (PlayerNotCompletedThisQuest(location.QuestAvailableHere) &&
                        PlayerHasAllQuestCompletionItems(location.QuestAvailableHere))
                    {
                        GivePlayerQuestRewards(location.QuestAvailableHere);
                    }
                }
                else
                {
                    GiveQuestToPlayer(location.QuestAvailableHere);
                }
            }

            SetTheCurrentMonsterForTheCurrentLocation(location);
        }

        private void SetTheCurrentMonsterForTheCurrentLocation(Location location)
        {
            // Populate the current monster with this location's monster (or null, if there is no monster here)
            _currentMonster = location.NewInstanceOfMonsterLivingHere();

            if (_currentMonster != null)
            {
                RaiseMessage("You see a " + _currentMonster.Name);
            }
        }

        private void GiveQuestToPlayer(Quest quest)
        {
            RaiseMessage("You receive the " + quest.Name + " quest.", false);
            RaiseMessage(quest.Description, false);
            RaiseMessage("To complete it, return with:", false);

            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                RaiseMessage(string.Format("{0} {1}", qci.Quantity,
                    qci.Quantity == 1 ? qci.Details.Name : qci.Details.NamePlural));
            }
            RaiseMessage("", false);

            Quests.Add(new PlayerQuest(quest));
        }

        private void GivePlayerQuestRewards(Quest quest)
        {
            RaiseMessage("You complete the '" + quest.Name + "' quest.", false);
            RaiseMessage("You receive: ", false);
            RaiseMessage(quest.RewardExperiencePoints.ToString() + " experience points", false);
            RaiseMessage(quest.RewardGold.ToString() + " gold", false);
            RaiseMessage(quest.RewardItem.Name, true);

            AddExperiencePoints(quest.RewardExperiencePoints);
            Gold += quest.RewardGold;

            RemoveQuestCompletionItems(quest);
            AddItemToInventory(quest.RewardItem);

            MarkQuestCompleted(quest);
        }

        public void MoveNorth()
        {
            if (CurrentLocation.HasPathToNorth)
            {
                MoveTo(CurrentLocation.LocationToNorth);
            }
        }

        public void MoveEast()
        {
            if (CurrentLocation.HasPathToEast)
            {
                MoveTo(CurrentLocation.LocationToEast);
            }
        }

        public void MoveSouth()
        {
            if (CurrentLocation.HasPathToSouth)
            {
                MoveTo(CurrentLocation.LocationToSouth);
            }
        }

        public void MoveWest()
        {
            if (CurrentLocation.HasPathToWest)
            {
                MoveTo(CurrentLocation.LocationToWest);
            }
        }

        public void UseWeapon(Weapon currentWeapon)
        {
            // Determine the amount of damage to do to the monster
            int damageToMonster = (Level / 10) + RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            if (damageToMonster == 0)
            {
                RaiseMessage("You missed the " + _currentMonster.Name);
            }
            else
            {
                _currentMonster.CurrentHitPoints -= damageToMonster;
                RaiseMessage("You hit the " + _currentMonster.Name + " for " + damageToMonster + " points.");
            }

            // Check if the monster is dead
            if (_currentMonster.IsDead)
            {
                // Monster is dead
                GivePlayerMonsterKillingRewards();

                // Move player to current location (to heal player and create a new monster to fight)
                MoveTo(CurrentLocation);
            }
            else
            {
                // Monster is still alive
                MonsterAttacksThePlayer();
            }
        }

        private void GivePlayerMonsterKillingRewards()
        {
            RaiseMessage("");
            RaiseMessage("You defeated the " + _currentMonster.Name);
            RaiseMessage("You receive " + _currentMonster.RewardExperiencePoints + " experience points");
            RaiseMessage("You receive " + _currentMonster.RewardGold + " gold");

            AddExperiencePoints(_currentMonster.RewardExperiencePoints);
            Gold += _currentMonster.RewardGold;

            // Give monster's loot items to the player
            foreach (InventoryItem inventoryItem in _currentMonster.LootItems)
            {
                AddItemToInventory(inventoryItem.Details, inventoryItem.Quantity);

                RaiseMessage(string.Format("You loot {0} {1}", inventoryItem.Quantity, inventoryItem.Description));
            }

            RaiseMessage("");
        }

        public void UsePotion(HealingPotion potion)
        {
            // Display message
            RaiseMessage("You drink a " + potion.Name, false);

            // Add healing amount to the player's current hit points
            Heal(potion.AmountToHeal);

            // Remove the potion from the player's inventory
            RemoveItemFromInventory(potion, 1);

            // Monster gets their turn to attack
            MonsterAttacksThePlayer();
        }

        private void MonsterAttacksThePlayer()
        {
            // Determine the amount of damage the monster does to the player
            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

            // Display message
            RaiseMessage("The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage.", false);

            // Subtract damage from player
            CurrentHitPoints -= damageToPlayer;

            if (IsDead)
            {
                // Display message
                RaiseMessage("The " + _currentMonster.Name + " killed you.", true);

                // Move player to "Home"
                MoveHome();
            }

            RaiseMessage("", false);
        }

        private void RaiseMessage(string message, bool addExtraNewLine = false)
        {
            OnMessage?.Invoke(this, new MessageEventArgs(message, addExtraNewLine));
        }

        private void MoveHome()
        {
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
        }

        private void CreateNewChildXmlNode(XmlDocument document, XmlNode parentNode, string elementName, object value)
        {
            XmlNode node = document.CreateElement(elementName);
            node.AppendChild(document.CreateTextNode(value.ToString()));
            parentNode.AppendChild(node);
        }

        private void AddXmlAttributeToNode(XmlDocument document, XmlNode node, string attributeName, object value)
        {
            XmlAttribute attribute = document.CreateAttribute(attributeName);
            attribute.Value = value.ToString();
            node.Attributes.Append(attribute);
        }
    }
}