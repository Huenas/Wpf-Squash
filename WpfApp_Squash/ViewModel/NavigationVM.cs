using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp_Squash.Utilities;


namespace WpfApp_Squash.ViewModel
{

    class NavigationVM : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand HomeCommand { get; set; }
        public ICommand CampagneCgiCommand { get; set; }
        public ICommand CampagneS2eCommand { get; set; }    

    
        private void Home(object obj) => CurrentView = new HomeVM();
        private void CampagneCgi(object obj) => CurrentView = new CampagneCgiVM();
        private void CampagneS2e(object obj) => CurrentView = new CampagneS2eVM();


        public NavigationVM()
        {
            HomeCommand = new RelayCommand(Home);
            CampagneCgiCommand = new RelayCommand(CampagneCgi);
            CampagneS2eCommand = new RelayCommand(CampagneS2e);


            // Startup Page
            CurrentView = new HomeVM();
        }
    }
    
}
