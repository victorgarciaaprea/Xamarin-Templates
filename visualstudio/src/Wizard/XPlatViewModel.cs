using EnvDTE80;
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

        bool isSharedSelected = false;
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

        public bool IsOkEnabled
        {
            get
            {
                return IsAndroidSelected || IsIOSSelected;
            }
        }

        private bool isAndroidSelected = true;
        public bool IsAndroidSelected
        {
            get
            {
                return isAndroidSelected;
            }
            set
            {
                isAndroidSelected = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsAndroidSelected)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsOkEnabled)));
            }
        }

        private bool isIOSSelected = true;
        public bool IsIOSSelected
        {
            get
            {
                return isIOSSelected;
            }
            set
            {
                isIOSSelected = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsIOSSelected)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsOkEnabled)));
            }
        }

        public bool IsAndroidEnabled { get; set; } = true;
        public bool IsIOSEnabled { get; set; } = true;

        private void UpdateFromSharing()
        {
            if (!IsAzureEnabled)
            {
                IsAzureSelected = IsAzureAvailable && SelectedSharing.Azure != null;

                PropertyChanged(this, new PropertyChangedEventArgs("IsAzureSelected"));
            }

            PropertyChanged(this, new PropertyChangedEventArgs("IsAzureEnabled"));
        }

        public bool IsNetStandardSelected
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

                return isInstalled == 1;
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

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => {};

        private List<XPlatItemViewModel> CreateTemplatesContext()
        {
            var list = new List<XPlatItemViewModel>();

            if (IsSharedSupported && IsMasterDetailSupportedVersion()) list.Add(new XPlatItemViewModel
            {
                Id = "master-detail",
                TargetPlatform = "Forms",
                Name = Resources.MasterDetail,
                Icon = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), @"FormsProject.jpg"),
                Description = Resources.MasterDetailDescription,
                Forms = new ReferenceTypeItem
                {
                    Shared = new AzureTypeItem
                    {
                        Azure = "master-detail",
                        NoAzure = "master-detail"
                    },
                    PCL = new AzureTypeItem
                    {
                        Azure = "master-detail",
                        NoAzure = "master-detail"
                    }
                },
                Native = new ReferenceTypeItem
                {
                    Shared = new AzureTypeItem
                    {
                        Azure = "master-detail",
                        NoAzure = "master-detail"
                    },
                    PCL = new AzureTypeItem
                    {
                        Azure = "master-detail",
                        NoAzure = "master-detail"
                    }
                }
            }
            );

            if (IsSharedSupported && IsMasterDetailSupportedVersion()) list.Add(new XPlatItemViewModel
            {
                Id = "tabbed", // Using "tabbed" as Id since the value of Resources.MasterDetail is "Tabbed"
                TargetPlatform = "Forms",
                Name = Resources.TabbedApp,
                Icon = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), @"FormsProject.jpg"),
                Description = Resources.TabbedAppDescription,
                Forms = new ReferenceTypeItem
                {
                    Shared = new AzureTypeItem
                    {
                        Azure = "tabbed",
                        NoAzure = "tabbed"
                    },
                    PCL = new AzureTypeItem
                    {
                        Azure = "tabbed",
                        NoAzure = "tabbed"
                    }
                }
            });

            if (IsSharedSupported && IsMasterDetailSupportedVersion()) list.Add(new XPlatItemViewModel
            {
                Id = "shell",
                TargetPlatform = "Forms",
                Name = Resources.ShellApp,
                Icon = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), @"FormsProject.jpg"),
                Description = Resources.ShellAppDescription,
                Forms = new ReferenceTypeItem
                {
                    Shared = new AzureTypeItem
                    {
                        Azure = "shell",
                        NoAzure = "shell"
                    },
                    PCL = new AzureTypeItem
                    {
                        Azure = "shell",
                        NoAzure = "shell"
                    }
                }
            });

            list.Add(new XPlatItemViewModel
            {
                Id = "blank",
                TargetPlatform = "Forms",
                Name = Resources.BlankApp,
                Icon = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), @"FormsProject.jpg"),
                Description = Resources.BlankAppDescription,
                Forms = new ReferenceTypeItem
                {
                    PCL = new AzureTypeItem
                    {
                        NoAzure = "blank"
                    },
                    Shared = new AzureTypeItem
                    {
                        NoAzure = "blank"
                    }
                },
                Native = new ReferenceTypeItem
                {
                    PCL = new AzureTypeItem
                    {
                        NoAzure = "blank"
                    },
                    Shared = new AzureTypeItem
                    {
                        NoAzure = "blank"
                    }
                }

            });

            return list;
        }

    }
}
