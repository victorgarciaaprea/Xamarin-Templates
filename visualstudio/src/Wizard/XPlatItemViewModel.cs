using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Templates.Wizards
{
    class XPlatItemViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string TargetPlatform { get; set; }

        public ReferenceTypeItem Forms { get; set; }
        public ReferenceTypeItem Native { get; set; }
    }

    public class ReferenceTypeItem
    {
        public AzureTypeItem Shared { get; set; }
        public AzureTypeItem PCL { get; set; }
    }

    public class AzureTypeItem
    {
        public string Azure { get; set; }
        public string NoAzure { get; set; }
    }
}
