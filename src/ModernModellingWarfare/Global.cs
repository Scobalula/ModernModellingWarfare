using PhilLibX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernModellingWarfare
{
    /// <summary>
    /// Basic Settings Class
    /// </summary>
    internal static class Global
    {
        /// <summary>
        /// Gets or Sets the Values
        /// </summary>
        public static Dictionary<string, string> Values { get; set; }

        /// <summary>
        /// Gets if Verbose is enabled
        /// </summary>
        public static bool Verbose => Values.TryGetValue("VerbosePrint", out var v) && v.Equals("yes", StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        /// Gets if Debug is enabled
        /// </summary>
        public static bool Debug => Values.TryGetValue("DebugPrint", out var v) && v.Equals("yes", StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        /// Gets the Image Extension
        /// </summary>
        public static string ImageExtension => Values.TryGetValue("ImageExtension", out var v) ? v : ".png";

        /// <summary>
        /// Gets the current Working Directory
        /// </summary>
        public static string WorkingDirectory => Path.GetDirectoryName(Environment.ProcessPath);

        /// <summary>
        /// Initializes Settings
        /// </summary>
        static Global()
        {
            Values = new Dictionary<string, string>()
            {
                { "VerbosePrint", "no" },
                { "DebugPrint", "no" }
            };
        }

        /// <summary>
        /// Prints a message if verbose is enabled
        /// </summary>
        /// <param name="value">Value to print</param>
        public static void VerbosePrint(string value)
        {
            if(Verbose) Printer.WriteLine("VERBOSE", value);
        }

        /// <summary>
        /// Prints a message if debug is enabled
        /// </summary>
        /// <param name="value">Value to print</param>
        public static void DebugPrint(string value)
        {
            if (Debug) Printer.WriteLine("DEBUG", value, ConsoleColor.DarkGreen);
        }
    }
}
