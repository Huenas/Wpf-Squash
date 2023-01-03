using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp_Squash.Model
{
    public class Evol:Campagne
    {
        public string Evoleees { get; set; }
        public int SuccessEvol { get; set; }
        public int RunningEvol { get; set; }
        public int FailedEvol { get; set; }

        public List<string> ResultEvoltest = new List<string> { };
    }
}
