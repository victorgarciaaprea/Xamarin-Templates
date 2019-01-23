using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace $rootnamespace$
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class $safeitemname$Master : ContentPage
    {
        public ListView ListView;

        public $safeitemname$Master()
        {
            InitializeComponent();

            BindingContext = new $safeitemname$MasterViewModel();
            ListView = MenuItemsListView;
        }

        class $safeitemname$MasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<$safeitemname$MenuItem> MenuItems { get; set; }
            
            public $safeitemname$MasterViewModel()
            {
                MenuItems = new ObservableCollection<$safeitemname$MenuItem>(new[]
                {
                    new $safeitemname$MenuItem { Id = 0, Title = "Page 1" },
                    new $safeitemname$MenuItem { Id = 1, Title = "Page 2" },
                    new $safeitemname$MenuItem { Id = 2, Title = "Page 3" },
                    new $safeitemname$MenuItem { Id = 3, Title = "Page 4" },
                    new $safeitemname$MenuItem { Id = 4, Title = "Page 5" },
                });
            }
            
            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}