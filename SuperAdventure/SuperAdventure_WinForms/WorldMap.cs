using Engine;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SuperAdventure_WinForms
{
    public partial class WorldMap : Form
    {
        readonly Assembly _EngineAssembly = Assembly.GetAssembly(typeof(Engine.Player));

        private Player _currentPlayer;
            
        public WorldMap(Player player)
        {
            _currentPlayer = player;

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

        private void SetImage(PictureBox pictureBox, string imageName)
        {
            using (Stream resourceStream =
                _EngineAssembly.GetManifestResourceStream(
                    "Engine.Images." + imageName + ".png"))
            {
                if (resourceStream != null)
                {
                    pictureBox.Image = new Bitmap(resourceStream);
                }
            }
        }
    }
}