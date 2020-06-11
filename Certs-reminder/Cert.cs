using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Certs_reminder
{
    public class Certificate
    {
        public string Fio { get; set; }
        public int Day { get; set; }
        public int Year { get; set; }
        public bool Expired { get; set; }
    }
}
