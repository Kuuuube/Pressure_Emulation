using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;
using System;
using System.Numerics;

namespace Pressure_Emulation
{
    [PluginName("Pressure Emulation Line")]
    public class Pressure_Emulation_Line : IPositionedPipelineElement<IDeviceReport>
    {
        private Vector2 start_position;
        private Vector2 output_position;
        private double output_pressure_double;
        private uint last_real_pressure;
        private uint calc_pressure_change(uint input_pressure) {
            if (pressure_resolution == 0) {
                pressure_resolution = 1;
            }
            if (line_length == 0) {
                line_length = 1;
            }

            double pressure_per_px = ((double)max_pressure_resolution / (double)line_length);
            output_pressure_double += pressure_per_px;

            uint pressure_divisor = (max_pressure_resolution / pressure_resolution);
            if (pressure_divisor <= 0) {
                    pressure_divisor = 1;
            }

            uint output_pressure_uint = (uint)output_pressure_double / pressure_divisor * pressure_divisor;
            return output_pressure_uint;
        }
        public IDeviceReport Resolution(IDeviceReport input)
        {
            if (input is ITabletReport tabletReport)
            {
                if (tabletReport.Pressure == 0) {
                    last_real_pressure = 0;
                    output_pressure_double = 0;
                    return input;
                }

                if (tabletReport.Pressure > 0 && last_real_pressure == 0) {
                    start_position = tabletReport.Position;
                    output_position = tabletReport.Position;
                    last_real_pressure = tabletReport.Pressure;
                }

                if (Math.Abs(start_position.X - output_position.X) > line_length + line_offset) {
                    tabletReport.Pressure = 0;
                    tabletReport.Position = output_position;
                    return input;
                }

                tabletReport.Pressure = calc_pressure_change(tabletReport.Pressure);
                output_position = new Vector2(output_position.X + 1, output_position.Y); 
                tabletReport.Position = output_position;
            }
            return input;
        }

        public event Action<IDeviceReport> Emit;

        public void Consume(IDeviceReport value)
        {
            if (value is ITabletReport report)
            {
                report = (ITabletReport)Filter(report);
                value = report;
            }

            Emit?.Invoke(value);
        }

        public IDeviceReport Filter(IDeviceReport input) => Resolution(input);

        public PipelinePosition Position => PipelinePosition.PostTransform;

        [Property("Pressure Resolution"), ToolTip
            ("Pressure Emulation Line:\n\n" +
            "Pressure Resolution: The pressure resolution to emulate (must be lower than the tablet's max).")]
        public uint pressure_resolution { set; get; }

        [Property("Line Length"), Unit("px"), ToolTip
            ("Pressure Emulation Line:\n\n" +
            "Line Length: The length in pixels to draw the line.")]
        public uint line_length { set; get; }

        [Property("Line Offset"), Unit("px"), ToolTip
            ("Pressure Emulation Line:\n\n" +
            "Line Offset: The length in pixels to continue drawing after max pressure is reached.")]
        public uint line_offset { set; get; }

        protected uint max_pressure_resolution;

        [TabletReference]
        public TabletReference TabletReference { set => HandleTabletReferenceInternal(value); }
        private void HandleTabletReferenceInternal(TabletReference tabletReference)
        {
            max_pressure_resolution = tabletReference.Properties.Specifications.Pen.MaxPressure;
        }
    }
}