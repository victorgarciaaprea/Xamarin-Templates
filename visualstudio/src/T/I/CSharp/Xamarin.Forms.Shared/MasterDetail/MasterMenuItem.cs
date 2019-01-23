using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace $rootnamespace$
{

    public class $safeitemname$MenuItem
    {
        public $safeitemname$MenuItem()
        {
            TargetType = typeof($safeitemname$Detail);
        }
        public int Id { get; set; }
        public string Title { get; set; }

        public Type TargetType { get; set; }
    }
}