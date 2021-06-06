using RA2MapNameEncrypt.Utils;
using Shimakaze.Struct.Ini;
using Shimakaze.Struct.Ini.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA2MapNameEncrypt.Data
{
    class RA2Map
    {
        public FileInfo fMap;
        private List<string> lSection;

        public RA2Map(string path)
        {
            fMap = new FileInfo(path);
            lSection = new List<string>();
        }

        public async Task ParseSection()
        {
            fMap.Refresh(); //To easier check the existence (IOException)
            var doc = await IniDocumentUtils.ParseAsync(fMap.OpenRead());
            foreach (var reg in doc["TaskForces"])
            {
                lSection.Add(reg.Value);
            }
            foreach (var reg in doc["ScriptTypes"])
            {
                lSection.Add(reg.Value);
            }
            foreach (var reg in doc["TeamTypes"])
            {
                lSection.Add(reg.Value);
            }
        }

        public async Task CRC32Encrypt()
        {
            fMap.Refresh();
            var crc = new CRC32();
            IIniDocument doc;
            using (FileStream fs = fMap.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                doc = await IniDocumentUtils.ParseAsync(fs);
            foreach (var i in lSection)
            {
                var desc = (string)doc[i, "Name"];
                doc[i, "Name"] = crc.Encrypt(desc).ToString("x");
            }
            foreach (var kv in doc["Triggers"])
            {
                var element = ((string)kv.Value).Split(',');
                element[2] = crc.Encrypt(element[2]).ToString("x");
                doc["Triggers", kv.Key] = string.Join(",", element);
            }
            foreach (var kv in doc["Tags"])
            {
                var element = ((string)kv.Value).Split(',');
                element[1] = crc.Encrypt(element[1]).ToString("x");
                doc["Tags", kv.Key] = string.Join(",", element);
            }
            foreach (var kv in doc["VariableNames"])
            {
                var element = ((string)kv.Value).Split(',');
                element[0] = crc.Encrypt(element[0]).ToString("x");
                doc["VariableNames", kv.Key] = string.Join(",", element);
            }
            using (var fs = fMap.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
                await doc.DeparseAsync(fs);
        }

        public async Task BarCodeEncrypt()
        {
            fMap.Refresh();
            var bc = new BarCode();
            IIniDocument doc;
            using (FileStream fs = fMap.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                doc = await IniDocumentUtils.ParseAsync(fs);
            foreach (var i in lSection)
            {
                doc[i, "Name"] = new string(bc.Generate());
            }
            foreach (var kv in doc["Triggers"])
            {
                var element = ((string)kv.Value).Split(',');
                element[2] = new string(bc.Generate());
                doc["Triggers", kv.Key] = string.Join(",", element);
            }
            foreach (var kv in doc["Tags"])
            {
                var element = ((string)kv.Value).Split(',');
                element[1] = new string(bc.Generate());
                doc["Tags", kv.Key] = string.Join(",", element);
            }
            foreach (var kv in doc["VariableNames"])
            {
                var element = ((string)kv.Value).Split(',');
                element[0] = new string(bc.Generate());
                doc["VariableNames", kv.Key] = string.Join(",", element);
            }
            using (var fs = fMap.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
                await doc.DeparseAsync(fs);
        }
    }
}
