using grapher.Models.Serialized;

namespace grapher.Layouts
{
    public class OffLayout : LayoutBase
    {
        public OffLayout()
            : base()
        {
            Name = "Off";
            Index = -1;
            ButtonEnabled = true;
            LogarithmicCharts = false;

            MotivityLayout = new OptionLayout(false, string.Empty);
            SynchronousSpeedLayout = new OptionLayout(false, string.Empty);
            GammaLayout = new OptionLayout(false, string.Empty);
        }
    }
}
