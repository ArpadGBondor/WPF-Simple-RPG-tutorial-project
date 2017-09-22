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

            SetImage(pic_0_2, "HerbalistsGarden");
            SetImage(pic_1_2, "HerbalistsHut");
            SetImage(pic_2_0, "FarmFields");
            SetImage(pic_2_1, "Farmhouse");
            SetImage(pic_2_2, "TownSquare");
            SetImage(pic_2_3, "TownGate");
            SetImage(pic_2_4, "Bridge");
            SetImage(pic_2_5, "SpiderForest");
            SetImage(pic_3_2, "Home");
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
