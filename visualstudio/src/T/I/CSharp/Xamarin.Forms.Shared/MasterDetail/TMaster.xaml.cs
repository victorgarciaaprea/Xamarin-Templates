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
    public partial class $fileinputname$Master : ContentPage
    {
        public ListView ListView;

        public $fileinputname$Master()
        {
            InitializeComponent();

            BindingContext = new $fileinputname$MasterViewModel();
            ListView = MenuItemsListView;
        }

        class $fileinputname$MasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<$fileinputname$MenuItem> MenuItems { get; set; }
            
            public $fileinputname$MasterViewModel()
            {
                MenuItems = new ObservableCollection<$fileinputname$MenuItem>(new[]
                {
                    new $fileinputname$MenuItem { Id = 0, Title = "Page 1" },
                    new $fileinputname$MenuItem { Id = 1, Title = "Page 2" },
                    new $fileinputname$MenuItem { Id = 2, Title = "Page 3" },
                    new $fileinputname$MenuItem { Id = 3, Title = "Page 4" },
                    new $fileinputname$MenuItem { Id = 4, Title = "Page 5" },
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