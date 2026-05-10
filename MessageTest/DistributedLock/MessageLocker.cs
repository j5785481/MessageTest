using System;
using RedLockNet;
using RedLockNet.SERedis;

namespace MessageTest.DistributedLock
{
    public interface IMessageLocker : IDistributedLock
    {
        IRedLock GrabLock(string messageBoardId);
    }
    public class MessageLocker : IMessageLocker
    {
        public RedLockFactory RedLockFactory { get; set; }
        public string AffixKey { get; set; }

        public IRedLock GrabLock(string messageId)
        {
            return RedLockFactory.CreateLock(
                $"{AffixKey}:{nameof(MessageLocker)}:{messageId}",
                TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(100));
        }
    }
}
