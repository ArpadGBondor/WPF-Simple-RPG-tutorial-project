using Engine;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.IO;


namespace SuperAdventure_WPF.UI
{
    /// <summary>
    /// Interaction logic for SuperAdventure.xaml
    /// </summary>
    public partial class SuperAdventure : Window
    {
        private Player _player;
        private Monster _currentMonster;

        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        public struct InventoryListElement
        {
            public string Name { get; set; }
            public int Quantity { get; set; }
        }

        public struct QuestListElement
        {
            public string Name { get; set; }
            public bool IsDone { get; set; }
        }

        public SuperAdventure()
        {
            if (File.Exists(PLAYER_DATA_FILE_NAME))
            {
                _player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
            }
            else
            {
                _player = Player.CreateDefaultPlayer();
            }

            this.DataContext = _player;

            InitializeComponent();

            MoveTo(_player.CurrentLocation);
        }

        //private void UpdatePlayerInfoUI()
        //{
        //    lblHitPoints.Text = _player.CurrentHitPoints.ToString() + "/" + _player.MaximumHitPoints.ToString();
        //    lblGold.Text = _player.Gold.ToString();
        //    lblExperience.Text = _player.ExperiencePoints.ToString();
        //    lblLevel.Text = _player.Level.ToString();
        //}

        private void btnNorth_Click(object sender, RoutedEventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnWest_Click(object sender, RoutedEventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void btnEast_Click(object sender, RoutedEventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnSouth_Click(object sender, RoutedEventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void LogLocation(string name, string description)
        {
            rtbLocation.Document.Blocks.Clear();
            rtbLocation.Document.Blocks.Add(new Paragraph(new Run(name)));
            rtbLocation.Document.Blocks.Add(new Paragraph(new Run(description)));
        }

        private void LogMessage(string message)
        {
            rtbMessages.Document.Blocks.Add(new Paragraph(new Run(message)));
            rtbMessages.ScrollToEnd();
        }

        private void MoveTo(Location newLocation)
        {
            //Does the location have any required items
            if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                LogMessage("You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location."/* + Environment.NewLine*/);
                return;
            }

            // Update the player's current location
            _player.CurrentLocation = newLocation;

            // Show/hide available movement buttons
            btnNorth.Visibility = (newLocation.LocationToNorth != null ? Visibility.Visible : Visibility.Hidden);
            btnEast.Visibility = (newLocation.LocationToEast != null ? Visibility.Visible : Visibility.Hidden);
            btnSouth.Visibility = (newLocation.LocationToSouth != null ? Visibility.Visible : Visibility.Hidden);
            btnWest.Visibility = (newLocation.LocationToWest != null ? Visibility.Visible : Visibility.Hidden);

            // Display current location name and description
            LogLocation(newLocation.Name, newLocation.Description);

            // Completely heal the player
            _player.CurrentHitPoints = _player.MaximumHitPoints;

            // Update Hit Points in UI
            //UpdatePlayerInfoUI();

            // Does the location have a quest?
            if (newLocation.QuestAvailableHere != null)
            {
                // See if the player already has the quest, and if they've completed it
                bool playerAlreadyHasQuest = _player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = _player.CompletedThisQuest(newLocation.QuestAvailableHere);

                // See if the player already has the quest
                if (playerAlreadyHasQuest)
                {
                    // If the player has not completed the quest yet
                    if (!playerAlreadyCompletedQuest)
                    {
                        // See if the player has all the items needed to complete the quest
                        bool playerHasAllItemsToCompleteQuest = _player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

                        // The player has all items required to complete the quest
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            // Prepare message
                            string message = "You complete the '" + newLocation.QuestAvailableHere.Name + "' quest." + Environment.NewLine;

                            // Remove quest items from inventory
                            _player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);

                            // Give quest rewards
                            message += "You receive: " + Environment.NewLine +
                                newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine +
                                newLocation.QuestAvailableHere.RewardGold.ToString() + " gold" + Environment.NewLine +
                                newLocation.QuestAvailableHere.RewardItem.Name/* + Environment.NewLine*/;

                            // Display message
                            LogMessage(message);

                            _player.AddExperiencePoints(newLocation.QuestAvailableHere.RewardExperiencePoints);
                            _player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            // Add the reward item to the player's inventory
                            _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                            // Mark the quest as completed
                            _player.MarkQuestCompleted(newLocation.QuestAvailableHere);
                        }
                    }
                }
                else
                {
                    // The player does not already have the quest

                    // Display the messages
                    string message = "You receive the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine +
                        newLocation.QuestAvailableHere.Description + Environment.NewLine +
                        "To complete it, return with:" + Environment.NewLine;
                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        message += qci.Quantity.ToString() + " "
                            + (qci.Quantity == 1 ? qci.Details.Name : qci.Details.NamePlural)
                            /*+ Environment.NewLine*/;
                    }

                    // Display the messages
                    LogMessage(message);

                    // Add the quest to the player's quest list
                    _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }

            // Does the location have a monster?
            if (newLocation.MonsterLivingHere != null)
            {
                LogMessage("You see a " + newLocation.MonsterLivingHere.Name/* + Environment.NewLine*/);

                // Make a new monster, using the values from the standard monster in the World.Monster list
                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage,
                    standardMonster.RewardExperiencePoints, standardMonster.RewardGold, standardMonster.CurrentHitPoints, standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visibility = Visibility.Visible;
                cboPotions.Visibility = Visibility.Visible;
                btnUseWeapon.Visibility = Visibility.Visible;
                btnUsePotion.Visibility = Visibility.Visible;
            }
            else
            {
                _currentMonster = null;

                cboWeapons.Visibility = Visibility.Hidden;
                cboPotions.Visibility = Visibility.Hidden;
                btnUseWeapon.Visibility = Visibility.Hidden;
                btnUsePotion.Visibility = Visibility.Hidden;
            }


            // Refresh player's inventory list
            UpdateInventoryListInUI();

            // Refresh player's quest list
            UpdateQuestListInUI();

            // Refresh player's weapons combobox
            UpdateWeaponListInUI();

            // Refresh player's potions combobox
            UpdatePotionListInUI();
        }


        private void UpdateInventoryListInUI()
        {
            var inventoryList = new List<InventoryListElement>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                {
                    inventoryList.Add(
                        new InventoryListElement()
                        {
                            Name = inventoryItem.Details.Name,
                            Quantity = inventoryItem.Quantity
                        });
                }
            }

            dgvInventory.ItemsSource = inventoryList;
        }

        private void UpdateQuestListInUI()
        {
            var questList = new List<QuestListElement>();

            foreach (PlayerQuest playerQuest in _player.Quests)
            {
                questList.Add(
                    new QuestListElement()
                    {
                        Name = playerQuest.Details.Name,
                        IsDone = playerQuest.IsCompleted
                    });
            }

            dgvQuests.ItemsSource = questList;
        }

        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is Weapon)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }

            if (weapons.Count == 0)
            {
                // The player doesn't have any weapons, so hide the weapon combobox and "Use" button
                cboWeapons.Visibility = Visibility.Hidden;
                btnUseWeapon.Visibility = Visibility.Hidden;
            }
            else
            {
                cboWeapons.ItemsSource = weapons;
                if (_player.CurrentWeapon != null)
                {
                    cboWeapons.SelectedItem = _player.CurrentWeapon;
                }
                else
                {
                    cboWeapons.SelectedIndex = 0;
                }
            }
        }

        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is HealingPotion)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)inventoryItem.Details);
                    }
                }
            }

            if (healingPotions.Count == 0)
            {
                // The player doesn't have any potions, so hide the potion combobox and "Use" button
                cboPotions.Visibility = Visibility.Hidden;
                btnUsePotion.Visibility = Visibility.Hidden;
            }
            else
            {
                cboPotions.ItemsSource = healingPotions;
                cboPotions.SelectedIndex = 0;
            }
        }

        private void btnUseWeapon_Click(object sender, RoutedEventArgs e)
        {
            // Prepare message
            string message = "";

            // Get the currently selected weapon from the cboWeapons ComboBox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            // Determine the amount of damage to do to the monster
            int damageToMonster = (_player.Level/10) + RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            // Apply the damage to the monster's CurrentHitPoints
            _currentMonster.CurrentHitPoints -= damageToMonster;

            // Set message
            message += "You hit the " + _currentMonster.Name + " for " + damageToMonster.ToString() + " points.";

            // Check if the monster is dead
            if (_currentMonster.CurrentHitPoints <= 0)
            {
                // Monster is dead
                message += Environment.NewLine;
                message += Environment.NewLine;
                message += "You defeated the " + _currentMonster.Name;

                // Give player experience points for killing the monster
                _player.AddExperiencePoints(_currentMonster.RewardExperiencePoints);
                message += Environment.NewLine;
                message += "You receive " + _currentMonster.RewardExperiencePoints.ToString() + " experience points";

                // Give player gold for killing the monster 
                _player.Gold += _currentMonster.RewardGold;
                message += Environment.NewLine;
                message += "You receive " + _currentMonster.RewardGold.ToString() + " gold";

                // Get random loot items from the monster
                List<InventoryItem> lootedItems = new List<InventoryItem>();

                // Add items to the lootedItems list, comparing a random number to the drop percentage
                foreach (LootItem lootItem in _currentMonster.LootTable)
                {
                    if (RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }

                // If no items were randomly selected, then add the default loot item(s).
                if (lootedItems.Count == 0)
                {
                    foreach (LootItem lootItem in _currentMonster.LootTable)
                    {
                        if (lootItem.IsDefaultItem)
                        {
                            lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                        }
                    }
                }

                // Add the looted items to the player's inventory
                foreach (InventoryItem inventoryItem in lootedItems)
                {
                    _player.AddItemToInventory(inventoryItem.Details);

                    message += Environment.NewLine;
                    message += "You loot " + inventoryItem.Quantity.ToString() + " "
                        + (inventoryItem.Quantity == 1 ? inventoryItem.Details.Name : inventoryItem.Details.NamePlural);
                }

                // Refresh player information and inventory controls
                //UpdatePlayerInfoUI();
                UpdateInventoryListInUI();
                UpdateWeaponListInUI();
                UpdatePotionListInUI();


                // Move player to current location (to heal player and create a new monster to fight)
                MoveTo(_player.CurrentLocation);
            }
            else
            {
                // Monster is still alive

                // Determine the amount of damage the monster does to the player
                int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

                // Set message
                message += Environment.NewLine;
                message += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage.";

                // Subtract damage from player
                _player.CurrentHitPoints -= damageToPlayer;

                // Refresh player data in UI
                //UpdatePlayerInfoUI();

                if (_player.CurrentHitPoints <= 0)
                {
                    // Set message
                    message += Environment.NewLine;
                    message += "The " + _currentMonster.Name + " killed you.";

                    // Move player to "Home"
                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
                }
            }

            // Display the messages
            LogMessage(message);
        }

        private void btnUsePotion_Click(object sender, RoutedEventArgs e)
        {
            // Prepare message
            string message = "";

            // Get the currently selected potion from the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            // Add healing amount to the player's current hit points
            _player.CurrentHitPoints =
                Math.Min(_player.CurrentHitPoints + potion.AmountToHeal,
                    _player.MaximumHitPoints);

            // Remove the potion from the player's inventory
            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Details.ID == potion.ID)
                {
                    ii.Quantity--;
                    break;
                }
            }

            // Display message
            message += "You drink a " + potion.Name;

            // Monster gets their turn to attack

            // Determine the amount of damage the monster does to the player
            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

            // Set message
            message += Environment.NewLine;
            message += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage.";

            // Subtract damage from player
            _player.CurrentHitPoints -= damageToPlayer;

            if (_player.CurrentHitPoints <= 0)
            {
                // Set message
                message += Environment.NewLine;
                message += "The " + _currentMonster.Name + " killed you.";

                // Move player to "Home"
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            // Refresh player data in UI
            //UpdatePlayerInfoUI();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();

            // Display the messages
            LogMessage(message);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXmlString());
        }

        private void cboWeapons_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }
    }
}
