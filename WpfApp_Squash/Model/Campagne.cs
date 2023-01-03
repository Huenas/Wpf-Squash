using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp_Squash.Model
{
    public class Campagne
    {
        public string CampagneName { get; set; }
        public int IdCampagne { get; set; }
        public string Campagnes { get; set; }
        public string Apr { get; set; }
        public int Success { get; set; }
        public int Running { get; set; }
        public int Failed { get; set; }

        public List<string> Evol = new List<string>();
        public List<string> ResultEvol = new List<string> { };
        public static List<Campagne> Listresult = new List<Campagne> { };

        private List<Campagne> myList = new List<Campagne>();
        public List<Campagne> result()
        {
            return myList;
        }


        public static List<Campagne> listCampagne = new List<Campagne>
        {
                new Campagne{ CampagneName = "22R1", IdCampagne = 2365},
                new Campagne{ CampagneName = "22R1.1.1", IdCampagne = 2616},
                new Campagne{ CampagneName = "22R2", IdCampagne = 2515},
                new Campagne{ CampagneName = "22R3", IdCampagne = 2749},
                new Campagne{ CampagneName = "22R3.1.1", IdCampagne = 3086},
                new Campagne{ CampagneName = "22R3.2.1 IFU", IdCampagne = 3087},
                new Campagne{ CampagneName = "22R4", IdCampagne = 2829},
                new Campagne{ CampagneName = "23R1", IdCampagne = 3127}                    
        };

        

    }
}
