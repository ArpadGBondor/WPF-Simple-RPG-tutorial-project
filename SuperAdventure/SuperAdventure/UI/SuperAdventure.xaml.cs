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

            _player = 
                new Player()
                {
                    CurrentHitPoints    = 10,
                    MaximumHitPoints    = 10,
                    Gold                = 20,
                    ExperiencePoints    = 0,
                    Level               = 1
                };

            lblHitPoints.Text = _player.CurrentHitPoints.ToString() + "/" + _player.MaximumHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();

        }
    }
}
