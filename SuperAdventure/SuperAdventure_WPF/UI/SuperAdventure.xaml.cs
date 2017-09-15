using Engine;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace SuperAdventure_WPF.UI
{
    /// <summary>
    /// Interaction logic for SuperAdventure.xaml
    /// </summary>
    public partial class SuperAdventure : Window
    {
        private Player _player;

        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        public SuperAdventure()
        {
            _player = PlayerDataMapper.CreateFromDatabase();

            if (_player == null)
            {
                if (File.Exists(PLAYER_DATA_FILE_NAME))
                {
                    _player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
                }
                else
                {
                    _player = Player.CreateDefaultPlayer();
                }
            }

            // Update the combobox data when the player’s inventory changes.
            // Displays location information, and handles the visibility of buttons and comboboxes.
            _player.PropertyChanged += PlayerOnPropertyChanged;

            // We add this DisplayMessage function to run when the OnMessage event is raised, and we want to add the new message to the UI:
            _player.OnMessage += DisplayMessage;

            this.DataContext = _player;

            InitializeComponent();

            // Set selected item in the combo boxes
            if (_player.CurrentWeapon != null)
            {
                cboWeapons.SelectedItem = _player.CurrentWeapon;
            }
            else
            {
                cboWeapons.SelectedIndex = 0;
            }
            cboPotions.SelectedIndex = 0;

            _player.MoveTo(_player.CurrentLocation);
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            _player.MoveNorth();
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            _player.MoveEast();
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            _player.MoveSouth();
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            _player.MoveWest();
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

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            // Get the currently selected weapon from the cboWeapons ComboBox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            _player.UseWeapon(currentWeapon);
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            // Get the currently selected potion from the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            _player.UsePotion(potion);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXmlString());

            PlayerDataMapper.SaveToDatabase(_player);
        }

        private void cboWeapons_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Weapons")
            {
                Weapon previouslySelectedWeapon = _player.CurrentWeapon;

                cboWeapons.ItemsSource = _player.Weapons;

                // Handle the possibility that the player does not have any weapons, or sold their weapon.
                if (previouslySelectedWeapon != null &&
                _player.Weapons.Exists(w => w.ID == previouslySelectedWeapon.ID))
                {
                    cboWeapons.SelectedItem = previouslySelectedWeapon;
                }

                if (!_player.Weapons.Any())
                {
                    cboWeapons.Visibility = Visibility.Hidden;
                    btnUseWeapon.Visibility = Visibility.Hidden;
                }
            }

            if (propertyChangedEventArgs.PropertyName == "Potions")
            {
                cboPotions.ItemsSource = _player.Potions;

                if (!_player.Potions.Any())
                {
                    cboPotions.Visibility = Visibility.Hidden;
                    btnUsePotion.Visibility = Visibility.Hidden;
                }
            }

            if (propertyChangedEventArgs.PropertyName == "CurrentLocation")
            {
                // Show/hide available movement buttons
                btnNorth.Visibility = (_player.CurrentLocation.HasPathToNorth ? Visibility.Visible : Visibility.Hidden);
                btnEast.Visibility = (_player.CurrentLocation.HasPathToEast ? Visibility.Visible : Visibility.Hidden);
                btnSouth.Visibility = (_player.CurrentLocation.HasPathToSouth ? Visibility.Visible : Visibility.Hidden);
                btnWest.Visibility = (_player.CurrentLocation.HasPathToWest ? Visibility.Visible : Visibility.Hidden);

                // Display current location name and description
                LogLocation(_player.CurrentLocation.Name, _player.CurrentLocation.Description);

                if (_player.CurrentLocation.IsMonsterLivingHere)
                {
                    cboWeapons.Visibility = (_player.Weapons.Any() ? Visibility.Visible : Visibility.Hidden);
                    cboPotions.Visibility = (_player.Potions.Any() ? Visibility.Visible : Visibility.Hidden);
                    btnUseWeapon.Visibility = (_player.Weapons.Any() ? Visibility.Visible : Visibility.Hidden);
                    btnUsePotion.Visibility = (_player.Potions.Any() ? Visibility.Visible : Visibility.Hidden);
                }
                else
                {
                    cboWeapons.Visibility = Visibility.Hidden;
                    cboPotions.Visibility = Visibility.Hidden;
                    btnUseWeapon.Visibility = Visibility.Hidden;
                    btnUsePotion.Visibility = Visibility.Hidden;
                }

                btnTrade.Visibility = (_player.CurrentLocation.IsVendorWorkingHere ? Visibility.Visible : Visibility.Hidden);
            }
        }
        private void DisplayMessage(object sender, MessageEventArgs messageEventArgs)
        {
            LogMessage(messageEventArgs.Message);
            if (messageEventArgs.AddExtraNewLine)
            {
                LogMessage("");
            }
        }

        private void btnTrade_Click(object sender, RoutedEventArgs e)
        {
            TradingScreen tradingScreen = new TradingScreen(_player);
            tradingScreen.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            tradingScreen.Owner = this;
            tradingScreen.ShowDialog();
        }
    }
}
