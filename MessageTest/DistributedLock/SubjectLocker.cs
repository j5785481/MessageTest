using System;
using RedLockNet;
using RedLockNet.SERedis;

namespace MessageTest.DistributedLock
{
    public interface ISubjectLocker : IDistributedLock
    {
        IRedLock GrabLock(int subjectId);
    }
    public class SubjectLocker : ISubjectLocker
    {
        public RedLockFactory RedLockFactory { get; set; }
        public string AffixKey { get; set; }

        public IRedLock GrabLock(int subjectId)
        {
            return RedLockFactory.CreateLock(
                $"{AffixKey}:{nameof(SubjectLocker)}:{subjectId}",
                TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(100));
        }
    }
}
