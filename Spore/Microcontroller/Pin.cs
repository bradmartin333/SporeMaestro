using static Microcontroller.IProtocol;
using Serilog;

namespace Microcontroller
{
    internal class Pin
    {
        /// <summary>
        /// Standard Arduino pin types.
        /// For all other cases (Steppers, servos, I2C),
        /// additional development is needed.
        /// </summary>
        internal enum Configs
        {
            /// <summary>
            /// Pin not used on board
            /// </summary>
            None,
            /// <summary>
            /// Can read/set digital high or low (0 or 1)
            /// </summary>
            Digital,
            /// <summary>
            /// Can read/set digital high or low (0 or 1) and analog read (0 - 1023)
            /// </summary>
            Analog,
            /// <summary>
            /// Can read/set digital high or low (0 or 1) and analog write (0 - 255)
            /// </summary>
            PWM,
        }
        internal enum PinModes
        {
            AnalogInput,
            AnalogOutput,
            DigitalInput,
            DigitalOutput,
            None,
        }


        internal int Number { get; set; }
        internal int Value { get; set; } = 0;
        internal bool ActiveLow { get; set; } = false;
        internal bool IsValid => Number != -1 && Config != Configs.None && _Mode != PinModes.None;

        private IProtocol? Protocol;
        private readonly Configs Config;

        private PinModes _Mode;
        internal PinModes Mode
        {
            get => _Mode;
            set
            {
                if ((value == PinModes.AnalogInput && Config != Configs.Analog) ||
                    (value == PinModes.AnalogOutput && Config != Configs.PWM))
                {
                    _Mode = PinModes.None;
                    Log.Fatal($"Cannot set pin {Number} to {value}");
                }
                else
                    _Mode = value;
            }
        }

        internal Pin(int number = -1,
                     Configs config = Configs.None,
                     PinModes mode = PinModes.None,
                     int value = 0,
                     bool activeLow = false)
        {
            Number = number;
            Config = config;
            Mode = mode;
            Value = value;
            ActiveLow = activeLow;
        }

        internal int Update()
        {
            switch (Mode)
            {
                case PinModes.AnalogOutput:
                    Protocol?.Set(SubCommand.Analog, Number, Value);
                    break;
                case PinModes.DigitalOutput:
                    Protocol?.Set(SubCommand.Digital, Number, Math.Abs(Value - Convert.ToInt32(ActiveLow)));
                    break;
                default:
                    break;
            }
            return Value;
        }

        internal int Initialize(IProtocol protocol)
        {
            Value = 0;
            Protocol = protocol;
            return Update();
        }

        public override string ToString() => $"{Number} {Mode} {(ActiveLow ? "[Active Low]" : "")} {Value}";
    }
}
