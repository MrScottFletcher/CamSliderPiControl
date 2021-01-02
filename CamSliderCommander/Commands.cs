using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamSliderCommander
{
    public class Commands
    {
        public const int INSTRUCTION_BYTES_SLIDER_PAN_TILT_SPEED = 4;
        public const string INSTRUCTION_STEP_MODE = "m";
        public const string INSTRUCTION_PAN_DEGREES = "p";
        public const string INSTRUCTION_TILT_DEGREES = "t";
        public const string INSTRUCTION_ENABLE = "e";
        public const string INSTRUCTION_SET_PAN_SPEED = "s";
        public const string INSTRUCTION_SET_TILT_SPEED = "S";
        public const string INSTRUCTION_INVERT_PAN = "i";
        public const string INSTRUCTION_INVERT_TILT = "I";
        public const string INSTRUCTION_SET_PAN_HALL_OFFSET = "o";
        public const string INSTRUCTION_SET_TILT_HALL_OFFSET = "O";
        public const string INSTRUCTION_SET_HOMING = "H";
        public const string INSTRUCTION_TRIGGER_SHUTTER = "c";
        public const string INSTRUCTION_AUTO_HOME = "A";
        public const string INSTRUCTION_DEBUG_STATUS = "R";
        public const string INSTRUCTION_EXECUTE_MOVES = ";";
        public const string INSTRUCTION_ADD_POSITION = "#";
        public const string INSTRUCTION_STEP_FORWARD = ">";
        public const string INSTRUCTION_STEP_BACKWARD = "<";
        public const string INSTRUCTION_JUMP_TO_START = "[";
        public const string INSTRUCTION_JUMP_TO_END = "]";
        public const string INSTRUCTION_EDIT_ARRAY = "E";
        public const string INSTRUCTION_ADD_DELAY = "d";
        public const string INSTRUCTION_EDIT_DELAY = "D";
        public const string INSTRUCTION_CLEAR_ARRAY = "C";
        public const string INSTRUCTION_SAVE_TO_EEPROM = "U";
        public const string INSTRUCTION_PANORAMICLAPSE = "L";
        public const string INSTRUCTION_ANGLE_BETWEEN_PICTURES = "b";
        public const string INSTRUCTION_DELAY_BETWEEN_PICTURES = "B";
        public const string INSTRUCTION_TIMELAPSE = "l";
        public const string INSTRUCTION_SLIDER_MILLIMETRES = "x";
        public const string INSTRUCTION_INVERT_SLIDER = "j";
        public const string INSTRUCTION_SET_SLIDER_SPEED = "X";
        public const string INSTRUCTION_ORIBIT_POINT = "@";
        public const string INSTRUCTION_CALCULATE_TARGET_POINT = "T";
        public const string INSTRUCTION_ACCEL_ENABLE = "a";
        public const string INSTRUCTION_PAN_ACCEL_INCREMENT_DELAY = "q";
        public const string INSTRUCTION_TILT_ACCEL_INCREMENT_DELAY = "Q";
        public const string INSTRUCTION_SLIDER_ACCEL_INCREMENT_DELAY = "w";
        public const string INSTRUCTION_SCALE_SPEED = "W";
    }
}
