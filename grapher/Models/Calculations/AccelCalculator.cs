﻿using grapher.Models.Serialized;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace grapher.Models.Calculations
{
    public class AccelCalculator
    {
        #region Structs

        public struct SimulatedMouseInput
        {
            public double velocity;
            public double time;
            public double angle;
            public int x;
            public int y;
        }

        #endregion Structs

        #region Static

        public static double[] SlowMovements =
        {
            0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0, 1.2, 1.4, 1.6, 1.8, 2.0, 2.2, 2.4, 2.6, 2.8, 3.0, 3.333, 3.666, 4.0, 4.333, 4.666, 
        };

        public IEnumerable<double> Angles = GetAngles();

        #endregion static

        #region Constructors

        public AccelCalculator(Field dpi, Field pollRate)
        {
            DPI = dpi;
            PollRate = pollRate;
        }

        #endregion Constructors

        #region Properties

        public ReadOnlyCollection<SimulatedMouseInput> SimulatedInputCombined { get; private set; }

        public ReadOnlyCollection<SimulatedMouseInput> SimulatedInputX { get; private set; }

        public ReadOnlyCollection<SimulatedMouseInput> SimulatedInputY { get; private set; }

        public IReadOnlyCollection<IReadOnlyCollection<SimulatedMouseInput>> SimulatedDirectionalInput { get; private set; }

        public Field DPI { get; private set; }

        public Field PollRate { get; private set; }

        private double MaxVelocity { get; set; }

        private double Increment { get; set; }
        
        private double MeasurementTime { get; set; }

        private (double, double) RotationVector { get; set; } 

        private (double, double) Sensitivity { get; set; }

        #endregion Fields

        #region Methods

        public static IEnumerable<double> GetAngles()
        {
            for(double i=0; i < (Constants.AngleDivisions); i++)
            {
                yield return (i / (Constants.AngleDivisions-1.0)) * (Math.PI / 2);
            }
        }

        public static int NearestAngleDivision(double angle)
        {
            var angleTransformed = angle * 2 / Math.PI * (Constants.AngleDivisions-1);
            return (int)Math.Round(angleTransformed);
        }

        public void Calculate(AccelChartData data, ManagedAccel accel, double starter, ICollection<SimulatedMouseInput> simulatedInputData)
        {
            double lastInputMagnitude = 0;
            double lastOutputMagnitude = 0;
            SimulatedMouseInput lastInput;
            double lastSlope = 0;

            double maxRatio = 0.0;
            double minRatio = Double.MaxValue;
            double maxSlope = 0.0;
            double minSlope = Double.MaxValue;

            double log = -2;
            int index = 0;
            int logIndex = 0;

            foreach (var simulatedInputDatum in simulatedInputData)
            {
                if (simulatedInputDatum.velocity <= 0)
                {
                    continue;
                }

                var output = accel.Accelerate(simulatedInputDatum.x, simulatedInputDatum.y, simulatedInputDatum.time);
                var outMagnitude = Velocity(output.Item1, output.Item2, simulatedInputDatum.time);
                var inDiff = Math.Round(simulatedInputDatum.velocity - lastInputMagnitude, 5);
                var outDiff = Math.Round(outMagnitude - lastOutputMagnitude, 5);

                if (inDiff == 0)
                {
                    continue;
                }

                if (!data.VelocityPoints.ContainsKey(simulatedInputDatum.velocity))
                {
                    data.VelocityPoints.Add(simulatedInputDatum.velocity, outMagnitude);
                }
                else
                {
                    continue;
                }

                while (Math.Pow(10,log) < outMagnitude && logIndex < data.LogToIndex.Length)
                {
                    data.LogToIndex[logIndex] = index;
                    log += 0.01;
                    logIndex++;
                }

                var ratio = outMagnitude / simulatedInputDatum.velocity;
                var slope = inDiff > 0 ? outDiff / inDiff : starter;

                if (slope < lastSlope)
                {
                    Console.WriteLine();
                }

                if (ratio > maxRatio)
                {
                    maxRatio = ratio;
                }

                if (ratio < minRatio)
                {
                    minRatio = ratio;
                }

                if (slope > maxSlope)
                {
                    maxSlope = slope;
                }

                if (slope < minSlope)
                {
                    minSlope = slope;
                }

                if (!data.AccelPoints.ContainsKey(simulatedInputDatum.velocity))
                {
                    data.AccelPoints.Add(simulatedInputDatum.velocity, ratio);
                }

                if (!data.GainPoints.ContainsKey(simulatedInputDatum.velocity))
                {
                    data.GainPoints.Add(simulatedInputDatum.velocity, slope);
                }

                lastInputMagnitude = simulatedInputDatum.velocity;
                lastOutputMagnitude = outMagnitude;
                index += 1;
                lastInput = simulatedInputDatum;
                lastSlope = slope;
            }

            index--;

            while (log <= 5.0)
            {
                data.LogToIndex[logIndex] = index;
                log += 0.01;
                logIndex++;
            }

            data.MaxAccel = maxRatio;
            data.MinAccel = minRatio;
            data.MaxGain = maxSlope;
            data.MinGain = minSlope;
        }

        public void CalculateDirectional(AccelChartData[] dataByAngle, ManagedAccel accel, DriverSettings settings, IReadOnlyCollection<IReadOnlyCollection<SimulatedMouseInput>> simulatedInputData)
        {
            double maxRatio = 0.0;
            double minRatio = Double.MaxValue;
            double maxSlope = 0.0;
            double minSlope = Double.MaxValue;


            Sensitivity = GetSens(ref settings);

            int angleIndex = 0;

            foreach (var simulatedInputDataAngle in simulatedInputData)
            {
                double log = -2;
                int index = 0;
                int logIndex = 0;
                double lastInputMagnitude = 0;
                double lastOutputMagnitude = 0;

                var data = dataByAngle[angleIndex];

                foreach (var simulatedInputDatum in simulatedInputDataAngle)
                {
                    if (simulatedInputDatum.velocity <= 0)
                    {
                        continue;
                    }

                    var output = accel.Accelerate(simulatedInputDatum.x, simulatedInputDatum.y, simulatedInputDatum.time);
                    var magnitude = Velocity(output.Item1, output.Item2, simulatedInputDatum.time);
                    var inDiff = Math.Round(simulatedInputDatum.velocity - lastInputMagnitude, 5);
                    var outDiff = Math.Round(magnitude - lastOutputMagnitude, 5);

                    if (inDiff == 0)
                    {
                        continue;
                    }

                    if (!data.VelocityPoints.ContainsKey(simulatedInputDatum.velocity))
                    {
                        data.VelocityPoints.Add(simulatedInputDatum.velocity, magnitude);
                    }
                    else
                    {
                        continue;
                    }

                    while (Math.Pow(10, log) < magnitude && logIndex < data.LogToIndex.Length)
                    {
                        data.LogToIndex[logIndex] = index;
                        log += 0.01;
                        logIndex++;
                    }

                    var ratio = magnitude / simulatedInputDatum.velocity;
                    var slope = inDiff > 0 ? outDiff / inDiff : settings.sensitivity.x;

                    bool indexToMeasureExtrema = (angleIndex == 0) || (angleIndex == (Constants.AngleDivisions - 1));

                    if (indexToMeasureExtrema && (ratio > maxRatio))
                    {
                        maxRatio = ratio;
                    }

                    if (indexToMeasureExtrema && (ratio < minRatio))
                    {
                        minRatio = ratio;
                    }

                    if (indexToMeasureExtrema && (slope > maxSlope))
                    {
                        maxSlope = slope;
                    }

                    if (indexToMeasureExtrema && (slope < minSlope))
                    {
                        minSlope = slope;
                    }

                    if (!data.AccelPoints.ContainsKey(simulatedInputDatum.velocity))
                    {
                        data.AccelPoints.Add(simulatedInputDatum.velocity, ratio);
                    }

                    if (!data.GainPoints.ContainsKey(simulatedInputDatum.velocity))
                    {
                        data.GainPoints.Add(simulatedInputDatum.velocity, slope);
                    }

                    lastInputMagnitude = simulatedInputDatum.velocity;
                    lastOutputMagnitude = magnitude;
                    index += 1;
                }

                index--;

                while (log <= 5.0)
                {
                    data.LogToIndex[logIndex] = index;
                    log += 0.01;
                    logIndex++;
                }

                angleIndex++;
            }

            dataByAngle[0].MaxAccel = maxRatio;
            dataByAngle[0].MinAccel = minRatio;
            dataByAngle[0].MaxGain = maxSlope;
            dataByAngle[0].MinGain = minSlope;
        }

        public ReadOnlyCollection<SimulatedMouseInput> GetSimulatedInput()
        {
            var magnitudes = new List<SimulatedMouseInput>();

            foreach (var slowMoveX in SlowMovements)
            {
                var slowMoveY = 0.0;
                var ceilX = (int)Math.Round(slowMoveX*50);
                var ceilY = (int)Math.Round(slowMoveY*50);
                var ceilMagnitude = Magnitude(ceilX, ceilY);
                var timeFactor = ceilMagnitude / Magnitude(slowMoveX, slowMoveY);

                SimulatedMouseInput mouseInputData;
                mouseInputData.x = ceilX;
                mouseInputData.y = ceilY;
                mouseInputData.time = MeasurementTime*timeFactor;
                mouseInputData.velocity = Velocity(ceilX, ceilY, mouseInputData.time);
                mouseInputData.angle = Math.Atan2(ceilY, ceilX);
                magnitudes.Add(mouseInputData);

            }

            for (double i = 5; i < MaxVelocity; i+=Increment)
            {
                SimulatedMouseInput mouseInputData;
                var ceil = (int)Math.Ceiling(i);
                var timeFactor = ceil / i;
                mouseInputData.x = ceil;
                mouseInputData.y = 0;
                mouseInputData.time = MeasurementTime * timeFactor;
                mouseInputData.velocity = Velocity(ceil, 0, mouseInputData.time);
                mouseInputData.angle = Math.Atan2(ceil, 0);
                magnitudes.Add(mouseInputData);
            }

            magnitudes.Sort((m1, m2) => m1.velocity.CompareTo(m2.velocity));

            return magnitudes.AsReadOnly();
        }

        public ReadOnlyCollection<SimulatedMouseInput> GetSimulatInputX()
        {
            var magnitudes = new List<SimulatedMouseInput>();

            foreach (var slowMovement in SlowMovements)
            {
                var ceil = (int)Math.Ceiling(slowMovement);
                var timeFactor = ceil / slowMovement;
                SimulatedMouseInput mouseInputData;
                mouseInputData.x = ceil;
                mouseInputData.y = 0;
                mouseInputData.time = MeasurementTime*timeFactor;
                mouseInputData.velocity = Velocity(ceil, 0, mouseInputData.time);
                mouseInputData.angle = 0;
                magnitudes.Add(mouseInputData);
            }

            for (double i = 5; i < MaxVelocity; i+=Increment)
            {
                SimulatedMouseInput mouseInputData;
                var ceil = (int)Math.Ceiling(i);
                var timeFactor = ceil / i;
                mouseInputData.x = ceil;
                mouseInputData.y = 0;
                mouseInputData.time = MeasurementTime*timeFactor;
                mouseInputData.velocity = Velocity(ceil, 0, mouseInputData.time);
                mouseInputData.angle = 0;
                magnitudes.Add(mouseInputData);
            }

            return magnitudes.AsReadOnly();
        }

        public ReadOnlyCollection<SimulatedMouseInput> GetSimulatedInputY()
        {
            var magnitudes = new List<SimulatedMouseInput>();

            foreach (var slowMovement in SlowMovements)
            {
                var ceil = (int)Math.Ceiling(slowMovement);
                var timeFactor = ceil / slowMovement;
                SimulatedMouseInput mouseInputData;
                mouseInputData.x = 0;
                mouseInputData.y = ceil;
                mouseInputData.time = MeasurementTime*timeFactor;
                mouseInputData.velocity = Velocity(0, ceil, mouseInputData.time);
                mouseInputData.angle = 0;
                magnitudes.Add(mouseInputData);
            }

            for (double i = 5; i < MaxVelocity; i+=Increment)
            {
                SimulatedMouseInput mouseInputData;
                var ceil = (int)Math.Ceiling(i);
                var timeFactor = ceil / i;
                mouseInputData.x = 0;
                mouseInputData.y = ceil;
                mouseInputData.time = MeasurementTime*timeFactor;
                mouseInputData.velocity = Velocity(0, ceil, mouseInputData.time);
                mouseInputData.angle = Math.PI / 2;
                magnitudes.Add(mouseInputData);
            }

            return magnitudes.AsReadOnly();
        }

        public IReadOnlyCollection<IReadOnlyCollection<SimulatedMouseInput>> GetSimulatedDirectionalInput()
        {
            var magnitudesByAngle = new List<IReadOnlyCollection<SimulatedMouseInput>>();

            foreach (var angle in Angles)
            {
                var magnitudes = new List<SimulatedMouseInput>();

                foreach (var slowMoveMagnitude in SlowMovements)
                {
                        var slowMoveX = Math.Round(slowMoveMagnitude * Math.Cos(angle), 4);
                        var slowMoveY = Math.Round(slowMoveMagnitude * Math.Sin(angle), 4);
                        var ceilX = (int)Math.Round(slowMoveX*90);
                        var ceilY = (int)Math.Round(slowMoveY*90);
                        var ceilMagnitude = Magnitude(ceilX, ceilY);
                        var timeFactor = ceilMagnitude / slowMoveMagnitude;

                        SimulatedMouseInput mouseInputData;
                        mouseInputData.x = ceilX;
                        mouseInputData.y = ceilY;
                        mouseInputData.time = timeFactor;
                        mouseInputData.velocity = Velocity(ceilX, ceilY, timeFactor);
                        mouseInputData.angle = angle;
                        magnitudes.Add(mouseInputData);
                }

                for (double magnitude = 5; magnitude < MaxVelocity; magnitude+=Increment)
                {
                        var slowMoveX = Math.Round(magnitude * Math.Cos(angle), 4);
                        var slowMoveY = Math.Round(magnitude * Math.Sin(angle), 4);
                        var ratio = slowMoveX > 0.0 ? slowMoveY / slowMoveX : 90;
                        var ceilX = (int)Math.Round(slowMoveX*90);
                        var ceilY = (int)Math.Round(slowMoveY*ratio);
                        var ceilMagnitude = Magnitude(ceilX, ceilY);
                        var timeFactor = ceilMagnitude / magnitude;

                        SimulatedMouseInput mouseInputData;
                        mouseInputData.x = ceilX;
                        mouseInputData.y = ceilY;
                        mouseInputData.time = timeFactor;
                        mouseInputData.velocity = Velocity(ceilX, ceilY, mouseInputData.time);
                        mouseInputData.angle = angle;
                        magnitudes.Add(mouseInputData);
                }

                magnitudesByAngle.Add(magnitudes.AsReadOnly());
            }

            return magnitudesByAngle.AsReadOnly();
        }

        public static double Magnitude(int x, int y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        public static double Magnitude(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        public static double Velocity(int x, int y, double time)
        {
            return Magnitude(x, y) / time;
        }

        public static double Velocity(double x, double y, double time)
        {
            return Magnitude(x, y) / time;
        }

        public static bool ShouldStripSens(ref DriverSettings settings) =>
            settings.sensitivity.x != settings.sensitivity.y;

        public static bool ShouldStripRot(ref DriverSettings settings) =>
            settings.rotation > 0;

        public static (double, double) GetSens(ref DriverSettings settings) =>
            (settings.sensitivity.x, settings.sensitivity.y);

        public static (double, double) GetRotVector(ref DriverSettings settings) =>
            (Math.Cos(settings.rotation), Math.Sin(settings.rotation));

        public static (double, double) StripSens(double outputX, double outputY, double sensitivityX, double sensitivityY) =>
            (outputX / sensitivityX, outputY / sensitivityY);

        public (double, double) StripRot(double outputX, double outputY, double rotX, double rotY) =>
            (outputX * rotX + outputY * rotY, outputX * rotY - outputY * rotX);

        public (double, double) StripThisSens(double outputX, double outputY) =>
            StripSens(outputX, outputY, Sensitivity.Item1, Sensitivity.Item2);

        public (double, double) StripThisRot(double outputX, double outputY) =>
            StripRot(outputX, outputY, RotationVector.Item1, RotationVector.Item2);

        public void ScaleByMouseSettings()
        {
            MaxVelocity = DPI.Data * Constants.MaxMultiplier;
            var ratio = MaxVelocity / Constants.Resolution;
            Increment = ratio;
            MeasurementTime = 1;
            SimulatedInputCombined = GetSimulatedInput();
            SimulatedInputX = GetSimulatInputX();
            SimulatedInputY = GetSimulatedInputY();
            SimulatedDirectionalInput = GetSimulatedDirectionalInput();
        }

        #endregion Methods
    }
}
