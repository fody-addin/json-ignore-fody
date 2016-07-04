using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonIgnore.Fody.Consumer
{
    public class Student
    {
        public string Name { set; get; }

        [JsonIgnore]
        public object QPropertyA { set; get; }
        public object QPropertyB { set; get; }
    }
}
