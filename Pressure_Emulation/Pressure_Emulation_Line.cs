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
        private Vector2 offset_position;
        private Vector2 output_position;
        private double output_pressure_double;
        private uint last_real_pressure;
        private uint emulating_pressure_resolution;
        private bool hold_report = true;
        private uint calc_pressure_change(uint input_pressure) {
            if (emulating_pressure_resolution == 0) {
                emulating_pressure_resolution = 1;
            }
            if (line_length == 0) {
                line_length = 1;
            }

            double pressure_per_px = (double)max_pressure_resolution / (double)line_length;
            output_pressure_double += pressure_per_px;

            uint pressure_divisor = max_pressure_resolution / emulating_pressure_resolution;
            if (pressure_divisor <= 0) {
                    pressure_divisor = 1;
            }

            uint output_pressure_uint = (uint)output_pressure_double / pressure_divisor * pressure_divisor;
            if (output_pressure_uint == 0) {
                output_pressure_uint = (uint)(pressure_deadzone_percent / 100 * max_pressure_resolution) + 1;
            }
            return output_pressure_uint;
        }
        public IDeviceReport Resolution(IDeviceReport input)
        {
            if (input is ITabletReport tabletReport)
            {
                if (tabletReport.Pressure <= pressure_deadzone_percent / 100 * max_pressure_resolution) {
                    hold_report = true;
                    emulating_pressure_resolution = pressure_resolution;
                    last_real_pressure = 0;
                    output_pressure_double = 0;
                    return input;
                }

                if (tabletReport.Pressure > pressure_deadzone_percent / 100 * max_pressure_resolution && last_real_pressure == 0) {
                    start_position = tabletReport.Position;
                    output_position = tabletReport.Position;
                    offset_position = start_position;
                    last_real_pressure = tabletReport.Pressure;
                }

                if (Math.Abs(start_position.X - output_position.X) > line_length + line_x_offset) {
                    if (continuous_mode_pressure_divisor == 0) {
                            continuous_mode_pressure_divisor = 1;
                    }
                    if (!continuous_mode || minimum_pressure_resolution > (uint)(emulating_pressure_resolution / continuous_mode_pressure_divisor)) {
                        tabletReport.Pressure = 0;
                        tabletReport.Position = output_position;
                        return input;
                    } else {
                        if (hold_report) {
                            hold_report = false;
                            tabletReport.Position = output_position;
                            tabletReport.Pressure = 0;
                            return input;
                        }
                        hold_report = true;
                        offset_position = new Vector2(offset_position.X + continuous_mode_x_offset, offset_position.Y + continuous_mode_y_offset);
                        start_position.X += continuous_mode_x_offset;
                        output_position = offset_position;
                        output_pressure_double = 0;
                        emulating_pressure_resolution = (uint)(emulating_pressure_resolution / continuous_mode_pressure_divisor);
                    }
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
            ("Pressure Resolution: The pressure resolution to emulate (must be lower than the tablet's max).")]
        public uint pressure_resolution { set; get; }

        [Property("Line Length"), Unit("px"), ToolTip
            ("Line Length: The length in pixels to draw the line.")]
        public uint line_length { set; get; }

        [Property("Line X Offset"), Unit("px"), ToolTip
            ("Line X Offset: The length in pixels to continue drawing after max pressure is reached.")]
        public uint line_x_offset { set; get; }

        [Property("Pressure Deadzone"), Unit("%"), ToolTip
            ("Pressure Deadzone: Adds a pressure deadzone at the set pressure percent (match this value to your Tip Threshold in the Pen Settings tab).")]
        public float pressure_deadzone_percent { set; get; }

        [BooleanProperty("Continuous Mode", ""), ToolTip
            ("Continuous Mode: Repeats the line drawing after applying the specified offsets and divisors.")]
        public bool continuous_mode { set; get; }

        [Property("Continuous Mode X Offset"), Unit("px"), ToolTip
            ("Continuous Mode X Offset: The length in pixels to offset the line in the X axis every repeat.")]
        public int continuous_mode_x_offset { set; get; }

        [Property("Continuous Mode Y Offset"), Unit("px"), ToolTip
            ("Continuous Mode Y Offset: The length in pixels to offset the line in the Y axis every repeat.")]
        public int continuous_mode_y_offset { set; get; }

        [Property("Continuous Mode Pressure Divisor"), ToolTip
            ("Continuous Mode Pressure Divisor: The number to divide the pressure resolution by every repeat.")]
        public float continuous_mode_pressure_divisor { set; get; }

        [Property("Minimum Pressure Resolution"), ToolTip
            ("Minimum Pressure Resolution: The minimum pressure resolution to emulate.")]
        public uint minimum_pressure_resolution { set; get; }

        protected uint max_pressure_resolution;

        [TabletReference]
        public TabletReference TabletReference { set => HandleTabletReferenceInternal(value); }
        private void HandleTabletReferenceInternal(TabletReference tabletReference)
        {
            max_pressure_resolution = tabletReference.Properties.Specifications.Pen.MaxPressure;
        }
    }
}