
namespace Live.PubSub.Applibs
{
    using System;
    using Autofac;
    using Live.PubSub.Core;

    /// <summary>
    /// 調度員實例
    /// </summary>
    public class PubSubDispatcher<TEventStream> : IPubSubDispatcher<TEventStream>
            where TEventStream : EventStream
    {
        private IContainer container;

        /// <summary>
        /// 發生錯誤時 callback回寫
        /// </summary>
        private Action<string> errorCallBack;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="container"></param>
        /// <param name="errorCallBack"></param>
        public PubSubDispatcher(IContainer container, Action<string> errorCallBack)
        {
            this.container = container;
            this.errorCallBack = errorCallBack;
        }

        /// <summary>
        /// 調度事件
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public bool DispatchMessage(TEventStream stream)
        {
            try
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    var handler = scope.ResolveNamed<IPubSubHandler<TEventStream>>(stream.Type);
                    return handler.Handle(stream);
                }
            }
            catch (Autofac.Core.Registration.ComponentNotRegisteredException)
            {
                return true;
            }
            catch (Exception ex)
            {
                this.errorCallBack($"DispatchMessage Exception:{ex.Message}");
                return false;
            }
        }
    }
}
