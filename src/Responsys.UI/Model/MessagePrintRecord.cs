using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Responsys.UI.Model
{
    public class MessagePrintRecord
    {

        public Guid Id { get; set; }

        public Guid JobMessageId { get; set; }

        public DateTime DatePrinted { get; set; }

        public MessagePrintRecord()
        {
            Id = Guid.NewGuid();
        }

    }
}
