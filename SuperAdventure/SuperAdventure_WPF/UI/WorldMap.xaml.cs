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
using System.Reflection;
using System.IO;
using System.Drawing;

namespace SuperAdventure_WPF.UI
{
    /// <summary>
    /// Interaction logic for WorldMap.xaml
    /// </summary>
    public partial class WorldMap : Window
    {
        private Player _currentPlayer;

        readonly Assembly _EngineAssembly = Assembly.GetAssembly(typeof(Engine.Player));

        public WorldMap(Player player)
        {
            _currentPlayer = player;
            this.DataContext = _currentPlayer;

            InitializeComponent();

            SetImage(pic_0_2, player.LocationsVisited.Contains(World.LOCATION_ID_ALCHEMISTS_GARDEN) ? "HerbalistsGarden" : "FogLocation");
            SetImage(pic_1_2, player.LocationsVisited.Contains(World.LOCATION_ID_ALCHEMIST_HUT) ? "HerbalistsHut" : "FogLocation");
            SetImage(pic_2_0, player.LocationsVisited.Contains(World.LOCATION_ID_FARM_FIELD) ? "FarmFields" : "FogLocation");
            SetImage(pic_2_1, player.LocationsVisited.Contains(World.LOCATION_ID_FARMHOUSE) ? "Farmhouse" : "FogLocation");
            SetImage(pic_2_2, player.LocationsVisited.Contains(World.LOCATION_ID_TOWN_SQUARE) ? "TownSquare" : "FogLocation");
            SetImage(pic_2_3, player.LocationsVisited.Contains(World.LOCATION_ID_GUARD_POST) ? "TownGate" : "FogLocation");
            SetImage(pic_2_4, player.LocationsVisited.Contains(World.LOCATION_ID_BRIDGE) ? "Bridge" : "FogLocation");
            SetImage(pic_2_5, player.LocationsVisited.Contains(World.LOCATION_ID_SPIDER_FIELD) ? "SpiderForest" : "FogLocation");
            SetImage(pic_3_2, player.LocationsVisited.Contains(World.LOCATION_ID_HOME) ? "Home" : "FogLocation");
        }

        private void SetImage(Image pictureBox, string imageName)
        {
            Stream stream =
                _EngineAssembly.GetManifestResourceStream(
                    "Engine.Images." + imageName + ".png");

            if (stream != null)
            {
                PngBitmapDecoder bitmapDecoder = 
                    new PngBitmapDecoder(
                        stream,
                        BitmapCreateOptions.PreservePixelFormat, 
                        BitmapCacheOption.Default);

                pictureBox.Source = bitmapDecoder.Frames[0];
            }
        }
    }
}
