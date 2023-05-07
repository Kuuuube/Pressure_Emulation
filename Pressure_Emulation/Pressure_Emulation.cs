using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;
using System;

namespace Pressure_Emulation
{
    [PluginName("Pressure Emulation")]
    public class Pressure_Emulation : IPositionedPipelineElement<IDeviceReport>
    {
        public IDeviceReport Resolution(IDeviceReport input)
        {
            if (input is ITabletReport tabletReport)
            {
                if (pressure_resolution == 0) {
                    pressure_resolution = 1;
                }

                uint pressure_divisor = (max_pressure_resolution / pressure_resolution);
                if (pressure_divisor <= 0) {
                    pressure_divisor = 1;
                }
                tabletReport.Pressure = tabletReport.Pressure / pressure_divisor * pressure_divisor;
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
            ("Pressure Emulation:\n\n" +
            "Pressure Resolution: The pressure resolution to emulate (must be lower than the tablet's max).")]
        public uint pressure_resolution { set; get; }
    
        protected uint max_pressure_resolution;

        [TabletReference]
        public TabletReference TabletReference { set => HandleTabletReferenceInternal(value); }
        private void HandleTabletReferenceInternal(TabletReference tabletReference)
        {
            max_pressure_resolution = tabletReference.Properties.Specifications.Pen.MaxPressure;
        }
    }
}