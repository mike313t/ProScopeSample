using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProScopeSampleApp.DataObjects
{
    public class LookupItemDTO
    {
        public int Id { get; set; }
        public Guid GuidId { get; set; }
        public bool IsActive { get; set; }
        public virtual string ValueShort { get; set; }
        
        public virtual string ValueLong { get; set; }
        
        public virtual string ValueDetail { get; set; }
        
        public virtual string HelpText { get; set; }
        public int SortOrder { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
    }
}
