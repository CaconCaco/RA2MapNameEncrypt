using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RA2MapNameEncrypt.Data;
using RA2MapNameEncrypt.Utils;

using Shimakaze.Struct.Ini;
using Shimakaze.Struct.Ini.Utils;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        const string URL = "https://api.github.com/repos/CaconCaco/RA2MapNameEncrypt/releases/latest";
        const string VERSION = "v1.2.2";

        static string[] RA2MapExt = { ".map", ".mpr", ".yrm" };

        public static List<RA2Map> MapFiles;
        public static async Task Main(string[] args)
        {
            Console.WriteLine($"{nameof(RA2MapNameEncrypt)} {VERSION}");
            var t_check_update = UpdateCheck();
            try
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

                await Task.WhenAll(MapFiles.Select(async map => await DoEncrypt(map, type, args.Contains("-o"))));

                //like system("Pause") in C.
                Console.WriteLine("Done!");
            }
            finally
            {
                await t_check_update;
            }
        }

        public static async Task DoEncrypt(RA2Map map, EncodeMode mode, bool copygen)
        {
            if (copygen)
            {
                var bak = new FileInfo(Path.Combine(map.DirectoryName, map.Name + ".bak"));
                if (bak.Exists)
                    Console_WriteColorLine($"[WARNING] Existing \"{bak.Name}\" overwritten!", ConsoleColor.DarkYellow);
                map.CopyTo(bak.FullName, true);
            }

            IIniDocument doc;
            using (var fs = map.file.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                doc = await IniDocumentUtils.ParseAsync(fs);

            await Task.WhenAll(
                Task.Run(() => SectionEncrypt(doc["Triggers"], 2, mode)),
                Task.Run(() => SectionEncrypt(doc["Tags"], 1, mode)),
                Task.Run(() => SectionEncrypt(doc["VariableNames"], 0, mode)),
                Task.Run(() => SectionEncrypt(doc["AITriggerTypes"], 0, mode)), //应该没人用这破玩意吧= =
                Task.Run(() => SectionEncrypt(map.AIElementRegs.Select(i => doc[i]), "Name", mode))
            );

            using (var fs = map.file.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
                await doc.DeparseAsync(fs);
            Console.WriteLine($"[Info] Process of \"{map.Name}\" is successful.");
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
                case EncodeMode.GUID: return Guid.NewGuid().ToString();
                default: return input;
            }
        }

        public static List<RA2Map> BatchMapCollect(DirectoryInfo dir)
        {
            if (!dir.Exists) return new List<RA2Map>();
            else return dir.GetFiles("*.*").Where(i => RA2MapExt.Contains(i.Extension)).Select(i => new RA2Map(i)).ToList();
        }

        public static List<RA2Map> MapCollect(string[] files) 
            => files.Select(i => new RA2Map(i)).Where(i => i.Exists).ToList();

        private static void Console_WriteColorLine(string str, ConsoleColor color)
        {
            ConsoleColor currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.ForegroundColor = currentForeColor;
        }


        static async Task UpdateCheck()
        {
            try
            {
                using (HttpClient http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Add("User-Agent", "Dotnet Framework 4.8 App");
                    var res = await http.GetAsync(URL);
                    var raw = await res.Content.ReadAsStringAsync();
                    var json = JObject.Parse(raw);
                    var new_version = json.Value<string>("tag_name");
                    if (new_version == VERSION)
                        return;

                    var download_url = json.SelectToken("assets[0]").Value<string>("browser_download_url");
                    Console.WriteLine($"Find New Version: {new_version}");
                    Console.WriteLine($"\tYou can Download from {download_url}");
                }
            }
            /*catch (HttpRequestException) 
            { 
                Console_WriteColorLine("[Warning] Couldn't access the update server.", ConsoleColor.DarkYellow);
            }*/
            catch { } //not much, just pass the checking process.
        }
    }
}
