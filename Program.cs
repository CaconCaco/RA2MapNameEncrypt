using RA2MapNameEncrypt.Data;
using RA2MapNameEncrypt.Utils;
using Shimakaze.Struct.Ini;
using Shimakaze.Struct.Ini.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RA2MapNameEncrypt
{
    static class Program
    {
        public static int EncryptMode;
        public static List<RA2Map> MapFiles = new List<RA2Map>();
        public static List<Task> FilesToDo = new List<Task>();
        public static async Task Main(string[] args)
        {
            if (!args.Contains("-i") || !args.Contains("-m"))
            {
                return;
            }
            int dInput = args.ToList().IndexOf("-i");
            int dMode = args.ToList().IndexOf("-m");
            switch (args[dMode + 1])
            {
                case "CRC32":
                    EncryptMode = 1;
                    Console.WriteLine("[Info] Encrypt Mode: CRC32");
                    break;
                case "BarCode":
                    EncryptMode = 2;
                    Console.WriteLine("[Info] Encrypt Mode: Bar Code");
                    break;
                default:
                    EncryptMode = 0;
                    return;
            }
            foreach (var i in args[dInput + 1].Split(','))
            {
                if (!new FileInfo(i).Exists)
                {
                    continue;
                }
                else
                {
                    var map = new RA2Map(i);
                    Console.WriteLine("[Info] File detected: {0}", i);
                    MapFiles.Add(map);
                }
            }
            if (MapFiles.Count == 0)
            {
                return;
            }
            foreach (var i in MapFiles)
            {
                FilesToDo.Add(i.DoEncrypt(EncryptMode));
            }
            while (FilesToDo.Count > 0)
            {
                Task done = await Task.WhenAny(FilesToDo);
                Console.WriteLine("[Info] Successfully encrypted 1 file.");
                FilesToDo.Remove(done);
            }
            //like system("Pause") in C.
            Console.WriteLine("Press any key to quit.");
            Console.ReadKey(true);

            return;
        }

        public static async Task DoEncrypt(this RA2Map map, int e_mode)
        {
            map.file.Refresh();
            IIniDocument doc;
            using (var fs = map.file.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                doc = await IniDocumentUtils.ParseAsync(fs);

            string encode(string input)
            {
                switch (e_mode)
                {
                    case 1:
                        var crc = new CRC32();
                        return crc.Encrypt(input).ToString("x");
                    case 2:
                        var bc = new BarCode();
                        return new string(bc.Generate());
                    case 0:
                    default:
                        return input;
                }
            }

            foreach (var kv in doc["Triggers"])
            {
                var origin = ((string)kv.Value).Split(',');
                origin[2] = encode(origin[2]);
                doc["Triggers", kv.Key] = string.Join(",", origin);
            }
            foreach (var kv in doc["Tags"])
            {
                var origin = ((string)kv.Value).Split(',');
                origin[1] = encode(origin[1]);
                doc["Tags", kv.Key] = string.Join(",", origin);
            }
            foreach (var kv in doc["VariableNames"])
            {
                var origin = ((string)kv.Value).Split(',');
                origin[0] = encode(origin[0]);
                doc["VariableNames", kv.Key] = string.Join(",", origin);
            }
            foreach (var i in map.AIElementRegs)
            {
                var origin = (string)doc[i, "Name"];
                doc[i, "Name"] = encode(origin);
            }
            using (var fs = map.file.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
                await doc.DeparseAsync(fs);
            return;
        }
    }
}
