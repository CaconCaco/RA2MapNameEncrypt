using RA2MapNameEncrypt.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA2MapNameEncrypt
{
    class Program
    {
        public static void Main()
        {
            Console.WriteLine("Map File: ");
            var i_file = Console.ReadLine();
            if (!new FileInfo(i_file).Exists)
            {
                ConsolePause("[IO ERROR]");
                return;
            }
            Console.WriteLine("Encrypt Mode (CRC32 or BarCode):");
            var i_mode = Console.ReadLine();
            switch (i_mode)
            {
                case "CRC32":
                    _ = MapEncryptCRC32(i_file);
                    break;
                case "BarCode":
                    _ = MapEncryptBC(i_file);
                    break;
                default:
                    ConsolePause("[MODE ERROR]");
                    return;
            }
            Console.ReadKey(true);
            return;
        }

        public static void ConsolePause(string msg = "Press any key to quit.")
        {
            Console.WriteLine(msg);
            Console.ReadKey(true);
        }

        public static async Task MapEncryptCRC32(string p)
        {
            var map = new RA2Map(p);
            await map.ParseSection();
            Console.WriteLine("Section Parsing: Done");
            await map.CRC32Encrypt();
            Console.WriteLine("Encrypt Process: Done");

            Console.WriteLine("Press any key to quit.");
            return;
        }

        public static async Task MapEncryptBC(string p)
        {
            var map = new RA2Map(p);
            await map.ParseSection();
            Console.WriteLine("Section Parsing: Done");
            await map.BarCodeEncrypt();
            Console.WriteLine("Encrypt Process: Done");

            Console.WriteLine("Press any key to quit.");
            return;
        }
    }
}
