using Engine;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SuperAdventure_WPF.UI
{
    /// <summary>
    /// Interaction logic for TradingScreen.xaml
    /// </summary>
    public partial class TradingScreen : Window
    {

        private Player _currentPlayer;

        public TradingScreen(Player player)
        {
            _currentPlayer = player;
            this.DataContext = _currentPlayer;

            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void dgvMyItems_CellClick(object sender, RoutedEventArgs e)
        {
            // This gets the ID value of the item
            var itemID = ((InventoryItem)((Button)sender).DataContext).ItemID;

            // Get the Item object for the selected item row
            Item itemBeingSold = World.ItemByID(Convert.ToInt32(itemID));

            if (itemBeingSold.Price == World.UNSELLABLE_ITEM_PRICE)
            {
                MessageBox.Show("You cannot sell the " + itemBeingSold.Name);
            }
            else
            {
                // Remove one of these items from the player's inventory
                _currentPlayer.RemoveItemFromInventory(itemBeingSold);

                // Give the player the gold for the item being sold.
                _currentPlayer.Gold += itemBeingSold.Price;
            }
        }

        private void dgvVendorItems_CellClick(object sender, RoutedEventArgs e)
        {
            // This gets the ID value of the item
            var itemID = ((InventoryItem)((Button)sender).DataContext).ItemID;

            // Get the Item object for the selected item row
            Item itemBeingBought = World.ItemByID(Convert.ToInt32(itemID));

            // Check if the player has enough gold to buy the item
            if (_currentPlayer.Gold >= itemBeingBought.Price)
            {
                // Add one of the items to the player's inventory
                _currentPlayer.AddItemToInventory(itemBeingBought);

                // Remove the gold to pay for the item
                _currentPlayer.Gold -= itemBeingBought.Price;
            }
            else
            {
                MessageBox.Show("You do not have enough gold to buy the " + itemBeingBought.Name);
            }
        }
    }
}
