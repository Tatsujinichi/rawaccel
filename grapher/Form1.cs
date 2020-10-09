using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using grapher.Models.Calculations;
using grapher.Models.Options;
using grapher.Models.Serialized;
using grapher.Models;

namespace grapher
{
    public partial class RawAcceleration : Form
    {

        #region Constructor


        public RawAcceleration()
        {
            InitializeComponent();

            ManagedAccel activeAccel = null;

            try
            {
                activeAccel = ManagedAccel.GetActiveAccel();
            }
            catch (DriverNotInstalledException ex)
            {
                MessageBox.Show($"Driver not installed.\n\n {ex.ToString()}");
                throw;
            }

            AccelGUI = AccelGUIFactory.Construct(
                this,
                activeAccel,
                AccelerationChart,
                AccelerationChartY,
                VelocityChart,
                VelocityChartY,
                GainChart,
                GainChartY,
                accelTypeDropX,
                accelTypeDropY,
                writeButton,
                toggleButton,
                showVelocityGainToolStripMenuItem,
                showLastMouseMoveToolStripMenuItem,
                wholeVectorToolStripMenuItem,
                byVectorComponentToolStripMenuItem,
                gainCapToolStripMenuItem,
                legacyCapToolStripMenuItem,
                gainOffsetToolStripMenuItem,
                legacyOffsetToolStripMenuItem,
                ScaleMenuItem,
                DPITextBox,
                PollRateTextBox,
                sensitivityBoxX,
                sensitivityBoxY,
                rotationBox,
                motivityBoxX,
                motivityBoxY,
                syncSpeedBoxX,
                syncSpeedBoxY,
                gammaBoxX,
                gammaBoxY,

                sensXYLock,
                ByComponentXYLock,
                LockXYLabel,
                sensitivityLabel,
                rotationLabel,
                motivityLabelX,
                motivityLabelY,
                syncSpeedLabelX,
                syncSpeedLabelY,
                gammaLabelX,
                gammaLabelY,

                ActiveValueTitle,
                ActiveValueTitleY,
                SensitivityActiveXLabel,
                SensitivityActiveYLabel,
                RotationActiveLabel,
                motivityActiveLabelX,
                motivityActiveLabelY,
                syncSpeedActiveLabelX,
                syncSpeedActiveLabelY,
                gammaActiveLabelX,
                gammaActiveLabelY,

                AccelTypeActiveLabelX,
                AccelTypeActiveLabelY,
                OptionSetXTitle,
                OptionSetYTitle,
                MouseLabel);
        }

        #endregion Constructor

        #region Properties

        public AccelGUI AccelGUI { get; }

        #endregion Properties

        #region Methods

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x00ff)
            {
                AccelGUI.MouseWatcher.ReadMouseMove(m);
            }

            base.WndProc(ref m);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void ResetAutoScroll()
        {
            chartsPanel.AutoScrollPosition = Constants.Origin;
        }

        public void DoResize()
        {
            ResetAutoScroll();

            var workingArea = Screen.PrimaryScreen.WorkingArea;
            var chartsPreferredSize = chartsPanel.GetPreferredSize(Constants.MaxSize);

            Size = new Size
            {
                Width = Math.Min(workingArea.Width - Location.X, optionsPanel.Size.Width + chartsPreferredSize.Width),
                Height = Math.Min(workingArea.Height - Location.Y, chartsPreferredSize.Height + 48)
            };
        }

        private void RawAcceleration_Paint(object sender, PaintEventArgs e)
        {
            //AccelGUI.AccelCharts.DrawLastMovement();
        }

        #endregion Method

        private void optionsPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
