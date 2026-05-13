namespace MessageTest.Lib.Procedure.Implements
{
    public class BaseProcedure<TResultCtx> : IProcedure<TResultCtx>
        where TResultCtx : IProcedureContext
    {
        public TResultCtx Ctx { get; set; }

        public IProcedure<TResultCtx> Execute<TProcessCtx>(IProcedureProcess<TProcessCtx> process)
            where TProcessCtx : TResultCtx
        {
            if (!Ctx.IsSuccess)
            {
                return this;
            }

            var typedCtx = (TProcessCtx)Ctx;
            Ctx = (TResultCtx)process.Process(typedCtx);

            return this;
        }

        public IProcedure<TResultCtx> Execute<TProcessCtx, TParam>(
            IProcedureProcess<TProcessCtx, TParam> process, TParam param)
            where TProcessCtx : TResultCtx
        {
            if (!Ctx.IsSuccess)
            {
                return this;
            }

            var typedCtx = (TProcessCtx)Ctx;
            Ctx = (TResultCtx)process.Process(typedCtx, param);

            return this;
        }

        /// <summary>
        /// 从指定的上下文创建一个新的处理过程
        /// </summary>
        /// <typeparam name="TResultCtx">结果上下文类型</typeparam>
        /// <param name="context">初始上下文</param>
        /// <returns>处理过程实例</returns>
        public static IProcedure<TResultCtx> From<TResultCtx>(TResultCtx context)
            where TResultCtx : IProcedureContext
        {
            return new BaseProcedure<TResultCtx>
            {
                Ctx = context
            };
        }

        public TResultCtx GetResult()
        {
            return Ctx;
        }
    }
}