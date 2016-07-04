using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyToProcess
{
    public class Student
    {
        public string Name { set; get; }
        public float Pga { set; get; }

        [JsonIgnore]
        public object QReferenceA { set; get; }
        public object QReferenceB { set; get; }
    }
}
