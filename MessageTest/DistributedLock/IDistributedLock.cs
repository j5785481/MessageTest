using RedLockNet.SERedis;

namespace MessageTest.DistributedLock
{
    /// <summary>
    ///     分布式鎖 interface
    /// </summary>
    public interface IDistributedLock
    {
        /// <summary>
        ///     red lock factory
        /// </summary>
        RedLockFactory RedLockFactory { get; set; }

        /// <summary>
        ///     前綴
        /// </summary>
        string AffixKey { get; set; }
    }
}
