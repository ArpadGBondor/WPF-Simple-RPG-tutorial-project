using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SuperAdventure_WinForms
{
    public partial class WorldMap : Form
    {
        readonly Assembly _EngineAssembly = Assembly.GetAssembly(typeof(Engine.Player));

        public WorldMap()
        {
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