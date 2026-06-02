using Moq;
using RedLockNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Tests.Helper
{
    public static class RedLockHelper
    {
        public static Mock<IRedLock> GetRedLock(bool acquired)
        {
            var m = new Mock<IRedLock>();
            m.Setup(x => x.IsAcquired)
                .Returns(acquired);
            return m;
        }
    }
}
