using grapher.Models.Serialized;
using Newtonsoft.Json.Serialization;

namespace grapher.Layouts
{
    public class TanhLayout : LayoutBase
    {
        public TanhLayout()
            : base()
        {
            Name = "Tanh";
            Index = (int)GainMode.tanh;
            LogarithmicCharts = false;

            MotivityLayout = new OptionLayout(true, Motivity);
            SynchronousSpeedLayout = new OptionLayout(true, SynchronousSpeed);
            GammaLayout = new OptionLayout(true, Gamma);

        }
    }
}
