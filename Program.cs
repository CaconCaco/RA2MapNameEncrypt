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
    enum EncodeMode
    {
        Default = 0,
        CRC32 = 1,
        BarCode = 2,
        GUID = 3
    }

    static class Program
    {
        public static List<RA2Map> MapFiles;
        public static async Task Main(string[] args)
        {
            int dInput = args.ToList().IndexOf("-i");
            int dMode = args.ToList().IndexOf("-m");
            if (dInput == -1 || dMode == -1)
                return;

            EncodeMode type;
            try
            {
                type = (EncodeMode)Enum.Parse(typeof(EncodeMode), args[dMode + 1]);
            }
            catch (IndexOutOfRangeException) { return; }
            catch (ArgumentException) { type = 0; }
            Console.WriteLine($"[Info] Encrypt Mode: {type}");

            MapFiles = args.Contains("-b") ?
                BatchMapCollect(new DirectoryInfo(args[dInput + 1])) :
                MapCollect(args[dInput + 1].Split(','));
            if (MapFiles.Count == 0) return;
            else Console.WriteLine($"[Info] {MapFiles.Count} file(s) parsed.");

            foreach (var i in MapFiles) await DoEncrypt(i, type, args.Contains("-o"));
            //await Task.WhenAll(MapFiles.Select(async map => await DoEncrypt(map, type, args.Contains("-o"))));

            //like system("Pause") in C.
            Console.WriteLine("Done!");
        }

        public static async Task DoEncrypt(RA2Map map, EncodeMode mode, bool copygen)
        {
            var filename = map.Name.Split('.');
            if (copygen) filename[0] += "-output";
            var output = copygen ? 
                new FileInfo(Path.Combine(map.DirectoryName, string.Join(".", filename))) : 
                map.file;

            IIniDocument doc;
            using (var fs = map.file.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                doc = await IniDocumentUtils.ParseAsync(fs);

            SectionEncrypt(doc["Triggers"], 2, mode);
            SectionEncrypt(doc["Tags"], 1, mode);
            SectionEncrypt(doc["VariableNames"], 0, mode);
            SectionEncrypt(doc["AITriggerTypes"], 0, mode); //应该没人用这破玩意吧= =
            SectionEncrypt(map.AIElementRegs.Select(i => doc[i]), "Name", mode);
            // await Task.WhenAll(
            //     Task.Run(() => SectionEncrypt(doc["Triggers"], 2, mode)),
            //     Task.Run(() => SectionEncrypt(doc["Tags"], 1, mode)),
            //     Task.Run(() => SectionEncrypt(doc["VariableNames"], 0, mode)),
            //     Task.Run(() => SectionEncrypt(doc["AITriggerTypes"], 0, mode)), //应该没人用这破玩意吧= =
            //     Task.Run(() => SectionEncrypt(map.AIElementRegs.Select(i => doc[i]), "Name", mode))
            // );

            using (var fs = output.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
                await doc.DeparseAsync(fs);
            Console.WriteLine($"[Info] Process of \"{output.Name}\" is successful.");
            return;
        }

        private static void SectionEncrypt(IIniSection section, int idx, EncodeMode enc)
        {
            foreach (var kv in section)
            {
                var origin = ((string)kv.Value).Split(',');
                origin[idx] = Encode(origin[idx], enc);
                section[kv.Key] = string.Join(",", origin);
            }
        }

        private static void SectionEncrypt(IEnumerable<IIniSection> sections, string key, EncodeMode enc)
        {
            foreach (var s in sections)
            {
                s[key] = Encode(s[key], enc);
            }
        }

        private static string Encode(string input, EncodeMode enc)
        {
            switch (enc)
            {
                case EncodeMode.CRC32: return CRC32.Encrypt(input).ToString("x");
                case EncodeMode.BarCode: return new string(new BarCode().Generate());
                case EncodeMode.GUID: return new Guid().ToString();
                default: return input;
            }
        }

        public static List<RA2Map> BatchMapCollect(DirectoryInfo dir)
        {
            if (!dir.Exists) return new List<RA2Map>();
            else
            {
                var ret = new List<RA2Map>(dir.GetFiles("*.map").Select(i => new RA2Map(i)));
                ret.AddRange(dir.GetFiles("*.mpr").Select(i => new RA2Map(i)));
                ret.AddRange(dir.GetFiles("*.yrm").Select(i => new RA2Map(i)));
                return ret;
            }
        }

        public static List<RA2Map> MapCollect(string[] files)
        {
            var f_array = files.Select(i => new RA2Map(i));
            return new List<RA2Map>(from map in f_array where map.Exists select map);
        }
    }
}
