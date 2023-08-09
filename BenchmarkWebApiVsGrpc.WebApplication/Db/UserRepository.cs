using System;
using System.Collections.Generic;
using System.Linq;

namespace BenchmarkWebApiVsGrpc.WebApp.Db
{
    public class UserRepository
    {
        const int DataCount = 1000000;
        private readonly CommonLib.User[] _collection;

        public UserRepository()
        {
            var startDate = new DateTime(2020, 1, 1).ToUniversalTime();

            _collection = Enumerable.Range(0, DataCount)
                .Select(i => new CommonLib.User()
                {
                    Address = Guid.NewGuid().ToString(),
                    Age = 18 + i % 50,
                    CreateDateTime = startDate.AddDays(1),
                    Email = $"user{i}@zxc.net",
                    UserName = $"User{i}"
                }).ToArray();
        }

        public IEnumerable<CommonLib.User> EnumerateAll()
        {
            return _collection;
        }

        public Span<CommonLib.User> GetPage(int page, int size)
        {
            return new Span<CommonLib.User>(_collection, page * size, size);
        }
    }
}
