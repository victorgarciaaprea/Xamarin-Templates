using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Templates.Properties;

namespace Xamarin.Templates.Wizards
{
    class XPlatViewModel : INotifyPropertyChanged
    {
        public List<XPlatItemViewModel> Templates { get; private set; }

        XPlatItemViewModel selectedTemplate;
        public XPlatItemViewModel SelectedTemplate
        {
            get { return selectedTemplate ?? Templates.First(); }
            set
            {
                selectedTemplate = value;

                UpdateFromTemplate();

                PropertyChanged(this, new PropertyChangedEventArgs("SelectedTemplate"));
            }
        }

        private void UpdateFromTemplate()
        {
            if (!IsTechnologyEnabled)
            {
                IsFormsSelected = selectedTemplate.Forms != null;
            }

            PropertyChanged(this, new PropertyChangedEventArgs("IsTechnologyEnabled"));

            UpdateFromTechnology();
        }

        Lazy<bool> isSharedSupported = new Lazy<bool>(() =>
            {
                var dte = ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

                var subKey = Registry.CurrentUser.OpenSubKey(dte.RegistryRoot + @"_Config\Projects\{D954291E-2A0B-460D-934E-DC6B0785DB48}", false);
                return (subKey != null);
            }
        );
        public bool IsSharedSupported => isSharedSupported.Value;

        bool isFormsSelected = true;
        public bool IsFormsSelected
        {
            get { return isFormsSelected; }
            set
            {
                isFormsSelected = value;

                UpdateFromTechnology();

                PropertyChanged(this, new PropertyChangedEventArgs("IsFormsSelected"));
                PropertyChanged(this, new PropertyChangedEventArgs("IsNativeSelected"));
            }
        }

        private void UpdateFromTechnology()
        {
            if (!IsSharingEnabled)
            {
                IsSharedSelected = IsSharedSupported && SelectedTechnology.Shared != null;
            }

            PropertyChanged(this, new PropertyChangedEventArgs("IsSharingEnabled"));
            PropertyChanged(this, new PropertyChangedEventArgs("IsSharedSelected"));
            PropertyChanged(this, new PropertyChangedEventArgs("IsPCLSelected"));

            UpdateFromSharing();
        }

        public bool IsNativeSelected
        {
            get
            {
                return !IsFormsSelected;
            }
            set
            {
                IsFormsSelected = !value;
            }
        }


        bool isSharedSelected = true;
        public bool IsSharedSelected
        {
            get { return isSharedSelected; }
            set
            {
                isSharedSelected = value;

                UpdateFromSharing();
                PropertyChanged(this, new PropertyChangedEventArgs("IsSharedSelected"));
                PropertyChanged(this, new PropertyChangedEventArgs("IsPCLSelected"));
            }
        }

        private void UpdateFromSharing()
        {
            if (!IsAzureEnabled)
            {
                IsAzureSelected = IsAzureAvailable && SelectedSharing.Azure != null;

                PropertyChanged(this, new PropertyChangedEventArgs("IsAzureSelected"));
            }

            PropertyChanged(this, new PropertyChangedEventArgs("IsAzureEnabled"));
        }

        public bool IsPCLSelected
        {
            get
            {
                return !IsSharedSelected;
            }
            set
            {
                IsSharedSelected = !value;
            }
        }

        public bool IsAzureSelected { get; set; }

        public XPlatViewModel()
        {
            Templates = CreateTemplatesContext();
            IsAzureAvailable = CheckAzureAvailability();
            isSharedSelected = IsSharedSupported;
        }

        private bool CheckAzureAvailability()
        {
            try
            {
                int isInstalled = 0;
                Guid azureGuid = new Guid("842377a1-ba0e-49be-ba8a-61f6101ce0da");

                var shell = ServiceProvider.GlobalProvider.GetService(typeof(SVsShell)) as IVsShell;
                shell.IsPackageInstalled(ref azureGuid, out isInstalled);

                var dte = ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

                return isInstalled == 1 && Type.GetType($"Microsoft.VisualStudio.Web.WindowsAzure.CommonContracts.IAzureShoppingCartDeploymentDialog, Microsoft.VisualStudio.Web.WindowsAzure.CommonContracts, Version={dte.Version}.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetVsVersion()
        {
            var dte = ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

            return dte.Version;
        }

        private bool IsMasterDetailSupportedVersion()
        {
            var version = new Version(GetVsVersion());

            return version.Major > 12;
        }

        public ReferenceTypeItem SelectedTechnology
        {
            get
            {
                if (IsFormsSelected)
                {
                    return SelectedTemplate.Forms;
                }
                else
                {
                    return SelectedTemplate.Native;
                }
            }
        }

        public AzureTypeItem SelectedSharing
        {
            get
            {
                if (IsSharedSelected)
                {
                    return SelectedTechnology.Shared;
                }
                else
                {
                    return SelectedTechnology.PCL;
                }
            }
        }

        public bool IsSharingEnabled
        {
            get
            {
                return IsSharedSupported && SelectedTechnology.PCL != null && SelectedTechnology.Shared != null;
            }
        }

        public bool IsTechnologyEnabled
        {
            get
            {
                return SelectedTemplate.Forms != null && SelectedTemplate.Native != null;
            }
        }

        public bool IsAzureEnabled
        {
            get
            {
                return IsAzureAvailable && SelectedSharing.Azure != null && SelectedSharing.NoAzure != null;
            }
        }

        public string SelectedTemplatePath
        {
            get
            {
                if (IsAzureSelected)
                {
                    return SelectedSharing.Azure;
                }
                else
                {
                    return SelectedSharing.NoAzure;
                }
            }
        }

        public bool IsAzureAvailable { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private List<XPlatItemViewModel> CreateTemplatesContext()
        {
            var list = new List<XPlatItemViewModel>();
            list.Add(new XPlatItemViewModel
            {
                Name = Resources.BlankApp,
                Icon = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), @"T\P\CSharp\FormsProject.jpg"),
                Description = Resources.BlankAppDescription,
                Forms = new ReferenceTypeItem
                {
                    PCL = new AzureTypeItem
                    {
                        NoAzure = "Xamarin.Forms.BlankApp"
                    },
                    Shared = new AzureTypeItem
                    {
                        NoAzure = "Xamarin.Forms.BlankApp"
                    }
                },
                Native = new ReferenceTypeItem
                {
                    PCL = new AzureTypeItem
                    {
                        NoAzure = "Xamarin.Native.BlankApp"
                    },
                    Shared = new AzureTypeItem
                    {
                        NoAzure = "Xamarin.Native.BlankApp"
                    }
                }

            });

            if (IsSharedSupported && IsMasterDetailSupportedVersion()) list.Add(new XPlatItemViewModel
            {
                Name = Resources.MasterDetail,
                Icon = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), @"T\P\CSharp\FormsProject.jpg"),
                Description = Resources.MasterDetailDescription,
                Forms = new ReferenceTypeItem
                {
                    Shared = new AzureTypeItem
                    {
                        Azure = "Xamarin.Forms.MasterDetailApp",
                        NoAzure = "Xamarin.Forms.MasterDetailApp"
                    },
                    PCL = new AzureTypeItem
                    {
                        Azure = "Xamarin.Forms.MasterDetailApp",
                        NoAzure = "Xamarin.Forms.MasterDetailApp"
                    }
                },
                Native = new ReferenceTypeItem
                {
                    Shared = new AzureTypeItem
                    {
                        Azure = "Xamarin.Native.MasterDetailApp",
                        NoAzure = "Xamarin.Native.MasterDetailApp"
                    },
                    PCL = new AzureTypeItem
                    {
                        Azure = "Xamarin.Native.MasterDetailApp",
                        NoAzure = "Xamarin.Native.MasterDetailApp"
                    }
                }
            }
            );
            return list;
        }

    }
}
