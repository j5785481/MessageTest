namespace MessageTest.Lib.Procedure.Implements
{
    /// <summary>
    /// 處理過程介面
    /// </summary>
    public interface IProcedure<TResultCtx>
        where TResultCtx : IProcedureContext
    {
        /// <summary>
        /// 執行處理程序
        /// </summary>
        IProcedure<TResultCtx> Execute<TProcessCtx>(IProcedureProcess<TProcessCtx> procedure)
            where TProcessCtx : TResultCtx;

        /// <summary>
        /// 執行處理程序（帶參數）
        /// </summary>
        IProcedure<TResultCtx> Execute<TProcessCtx, TParam>(
            IProcedureProcess<TProcessCtx, TParam> procedure, TParam param)
            where TProcessCtx : TResultCtx;

        /// <summary>
        /// 取得執行結果
        /// </summary>
        TResultCtx GetResult();
    }
}