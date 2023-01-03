using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp_Squash.Model;
    
namespace WpfApp_Squash.ViewModel
{
    class CampagneCgiVM : Utilities.ViewModelBase
    {
      
        private readonly Campagne _campagne;
       
        public CampagneCgiVM()
        {
            _campagne = new Campagne();
  
         
        }
 

    }
}
