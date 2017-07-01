using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Engine;

namespace SuperAdventure.UI
{
    /// <summary>
    /// Interaction logic for SuperAdventure.xaml
    /// </summary>
    public partial class SuperAdventure : Window
    {
        private Player _player;

        public SuperAdventure()
        {
            InitializeComponent();

            _player = new Player(10, 10, 20, 0, 1);

            lblHitPoints.Text = _player.CurrentHitPoints.ToString() + "/" + _player.MaximumHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();

        }

        private void btnNorth_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnWest_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEast_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSouth_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnUseWeapon_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnUsePotion_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
