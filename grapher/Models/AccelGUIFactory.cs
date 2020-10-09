using grapher.Models.Calculations;
using grapher.Models.Mouse;
using grapher.Models.Options;
using grapher.Models.Serialized;
using Newtonsoft.Json.Serialization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace grapher.Models
{
    public static class AccelGUIFactory
    {
        #region Methods

        public static AccelGUI Construct(
            RawAcceleration form,
            ManagedAccel activeAccel,
            Chart accelerationChart,
            Chart accelerationChartY,
            Chart velocityChart,
            Chart velocityChartY,
            Chart gainChart,
            Chart gainChartY,
            ComboBox accelTypeDropX,
            ComboBox accelTypeDropY,
            Button writeButton,
            ButtonBase toggleButton,
            ToolStripMenuItem showVelocityGainToolStripMenuItem,
            ToolStripMenuItem showLastMouseMoveMenuItem,
            ToolStripMenuItem wholeVectorToolStripMenuItem,
            ToolStripMenuItem byVectorComponentToolStripMenuItem,
            ToolStripMenuItem velocityGainCapToolStripMenuItem,
            ToolStripMenuItem legacyCapToolStripMenuItem,
            ToolStripMenuItem gainOffsetToolStripMenuItem,
            ToolStripMenuItem legacyOffsetToolStripMenuItem,
            ToolStripMenuItem scaleMenuItem,
            ToolStripTextBox dpiTextBox,
            ToolStripTextBox pollRateTextBox,
            TextBox sensitivityBoxX,
            TextBox sensitivityBoxY,
            TextBox rotationBox,

            TextBox motivityBoxX,
            TextBox motivityBoxY,
            TextBox syncSpeedBoxX,
            TextBox syncSpeedBoxY,
            TextBox gammaBoxX,
            TextBox gammaBoxY,

            CheckBox sensXYLock,
            CheckBox byComponentXYLock,
            Label lockXYLabel,
            Label sensitivityLabel,
            Label rotationLabel,

            Label motivityLabelX,
            Label motivityLabelY,
            Label syncSpeedLabelX,
            Label syncSpeedLabelY,
            Label gammaLabelX,
            Label gammaLabelY,

            Label activeValueTitleX,
            Label activeValueTitleY,
            Label sensitivityActiveXLabel,
            Label sensitivityActiveYLabel,
            Label rotationActiveLabel,

            Label motivityActiveLabelX,
            Label motivityActiveLabelY,
            Label syncSpeedActiveLabelX,
            Label syncSpeedActiveLabelY,
            Label gammaActiveLabelX,
            Label gammaActiveLabelY,

            Label accelTypeActiveLabelX,
            Label accelTypeActiveLabelY,
            Label optionSetXTitle,
            Label optionSetYTitle,
            Label mouseLabel)
        {
            var accelCalculator = new AccelCalculator(
                new Field(dpiTextBox.TextBox, form, Constants.DefaultDPI),
                new Field(pollRateTextBox.TextBox, form, Constants.DefaultPollRate));

            var accelCharts = new AccelCharts(
                                form,
                                new ChartXY(accelerationChart, accelerationChartY, Constants.SensitivityChartTitle),
                                new ChartXY(velocityChart, velocityChartY, Constants.VelocityChartTitle),
                                new ChartXY(gainChart, gainChartY, Constants.GainChartTitle),
                                showVelocityGainToolStripMenuItem,
                                showLastMouseMoveMenuItem,
                                writeButton,
                                accelCalculator);

            var sensitivity = new OptionXY(
                sensitivityBoxX,
                sensitivityBoxY,
                sensXYLock,
                form,
                1,
                sensitivityLabel,
                new ActiveValueLabelXY(
                    new ActiveValueLabel(sensitivityActiveXLabel, activeValueTitleX),
                    new ActiveValueLabel(sensitivityActiveYLabel, activeValueTitleX)),
                "Sens Multiplier");

            var rotation = new Option(
                rotationBox,
                form,
                0,
                rotationLabel,
                0,
                new ActiveValueLabel(rotationActiveLabel, activeValueTitleX),
                "Rotation");

            var optionSetYLeft = rotation.Left + rotation.Width;


            var defaults = DriverInterop.DefaultSettings.args.x;

            var motivityX = new Option(
                new Field(motivityBoxX, form, defaults.motivity),
                motivityLabelX,
                new ActiveValueLabel(motivityActiveLabelX, activeValueTitleX),
                0);

            var motivityY = new Option(
                new Field(motivityBoxY, form, defaults.motivity),
                motivityLabelY,
                new ActiveValueLabel(motivityActiveLabelY, activeValueTitleY),
                optionSetYLeft);

            var syncSpeedX = new Option(
                new Field(syncSpeedBoxX, form, defaults.synchronousSpeed),
                syncSpeedLabelX,
                new ActiveValueLabel(syncSpeedActiveLabelX, activeValueTitleX),
                0);

            var syncSpeedY = new Option(
                new Field(syncSpeedBoxY, form, defaults.synchronousSpeed),
                syncSpeedLabelY,
                new ActiveValueLabel(syncSpeedActiveLabelY, activeValueTitleY),
                optionSetYLeft);

            var gammaX = new Option(
                new Field(gammaBoxX, form, defaults.gamma),
                gammaLabelX,
                new ActiveValueLabel(gammaActiveLabelX, activeValueTitleX),
                0);

            var gammaY = new Option(
                new Field(gammaBoxY, form, defaults.gamma),
                gammaLabelY,
                new ActiveValueLabel(gammaActiveLabelY, activeValueTitleY),
                optionSetYLeft);

            var accelerationOptionsX = new AccelTypeOptions(
                accelTypeDropX,
                motivityX,
                syncSpeedX,
                gammaX,
                writeButton,
                new ActiveValueLabel(accelTypeActiveLabelX, activeValueTitleX));

            var accelerationOptionsY = new AccelTypeOptions(
                accelTypeDropY,
                motivityY,
                syncSpeedY,
                gammaY,
                writeButton,
                new ActiveValueLabel(accelTypeActiveLabelY, activeValueTitleY));

            var optionsSetX = new AccelOptionSet(
                optionSetXTitle,
                activeValueTitleX,
                rotationBox.Top + rotationBox.Height + Constants.OptionVerticalSeperation,
                accelerationOptionsX);

            var optionsSetY = new AccelOptionSet(
                optionSetYTitle,
                activeValueTitleY,
                rotationBox.Top + rotationBox.Height + Constants.OptionVerticalSeperation,
                accelerationOptionsY);

            var applyOptions = new ApplyOptions(
                wholeVectorToolStripMenuItem,
                byVectorComponentToolStripMenuItem,
                byComponentXYLock,
                optionsSetX,
                optionsSetY,
                sensitivity,
                rotation,
                lockXYLabel,
                accelCharts);

            var settings = new SettingsManager(
                activeAccel,
                accelCalculator.DPI,
                accelCalculator.PollRate,
                showLastMouseMoveMenuItem,
                showVelocityGainToolStripMenuItem);

            var mouseWatcher = new MouseWatcher(form, mouseLabel, accelCharts, accelCalculator.PollRate);

            return new AccelGUI(
                form,
                accelCalculator,
                accelCharts,
                settings,
                applyOptions,
                writeButton,
                toggleButton,
                mouseWatcher,
                scaleMenuItem);
        }

        #endregion Methods
    }
}
