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
        public static bool? IsHash;
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
                    IsHash = true;
                    Console.WriteLine("[Info] Encrypt Mode: CRC32");
                    break;
                case "BarCode":
                    IsHash = false;
                    Console.WriteLine("[Info] Encrypt Mode: Bar Code");
                    break;
                default:
                    IsHash = null;
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
                FilesToDo.Add(i.DoEncrypt(IsHash ?? false));
            }
            while (FilesToDo.Count > 0)
            {
                Task done = await Task.WhenAny(FilesToDo);
                Console.WriteLine("[Info] Successfully encrypted 1 file.");
                FilesToDo.Remove(done);
            }
            //like system("Pause") in C.
            Console.WriteLine("[Info] Objective Complete.");
            Console.ReadKey(true);

            return;
        }

        public static async Task DoEncrypt(this RA2Map map, bool useorigin)
        {
            map.file.Refresh();
            var crc = new CRC32();
            var bc = new BarCode();
            //var parse_list = new List<Task>();
            IIniDocument doc;
            using (var fs = map.file.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                doc = await IniDocumentUtils.ParseAsync(fs);
            // TODO: how to make those "foreach" async?
            // Reject: no need to async here anymore. The main() would PROPERLY RUN!!
            /*Action<string, int> pSection = (section, idx) =>
            {*/
            foreach (var kv in doc["Triggers"])
            {
                var origin = ((string)kv.Value).Split(',');
                origin[2] = useorigin ? crc.Encrypt(origin[2]).ToString("x") : new string(bc.Generate());
                doc["Triggers", kv.Key] = string.Join(",", origin);
            }
            foreach (var kv in doc["Tags"])
            {
                var origin = ((string)kv.Value).Split(',');
                origin[1] = useorigin ? crc.Encrypt(origin[1]).ToString("x") : new string(bc.Generate());
                doc["Tags", kv.Key] = string.Join(",", origin);
            }
            foreach (var kv in doc["VariableNames"])
            {
                var origin = ((string)kv.Value).Split(',');
                origin[0] = useorigin ? crc.Encrypt(origin[0]).ToString("x") : new string(bc.Generate());
                doc["VariableNames", kv.Key] = string.Join(",", origin);
            }
            /*};
            Action pVal = () =>
            {*/
            foreach (var i in map.AIElementRegs)
            {
                var origin = (string)doc[i, "Name"];
                doc[i, "Name"] = useorigin ? crc.Encrypt(origin).ToString("x") : new string(bc.Generate());
            }
            /*};*/

            /*while (parse_list.Count > 0)
            {
                Task parse_done = await Task.WhenAny(parse_list);
                parse_list.Remove(parse_done);
            }*/
            using (var fs = map.file.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
                await doc.DeparseAsync(fs);
            return;
        }


    }
}
