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
        public List<string> AIElementRegs;
        public FileInfo file;
        public RA2Map(string fs)
        {
            file = new FileInfo(fs);
            Task.WaitAny(AIElementParse(file));
        }
        public RA2Map(FileInfo f)
        {
            file = f;
            Task.WaitAny(AIElementParse(f));
        }
        private async Task AIElementParse(FileInfo f)
        {
            var doc = await IniDocumentUtils.ParseAsync(f.OpenRead());
            AIElementRegs = new List<string>(doc["TaskForces"].Select(i => (string)i.Value));
            AIElementRegs.AddRange(doc["ScriptTypes"].Select(i => (string)i.Value));
            AIElementRegs.AddRange(doc["TeamTypes"].Select(i => (string)i.Value));
        }
    }
}
