using System;

namespace BenchmarkWebApiVsGrpc.CommonLib
{
    public class User
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}
