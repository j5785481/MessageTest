namespace MessageTest.Lib.Procedure.Implements
{
	public interface IProcedureProcess
	{
	}

	/// <summary>
	/// 存儲過程程序介面
	/// </summary>
	public interface IProcedureProcess<TCtx> : IProcedureProcess
		where TCtx : IProcedureContext
	{
		/// <summary>
		/// 處理程序
		/// </summary>
		TCtx Process(TCtx ctx);
	}


	/// <summary>
	/// 帶參數的process
	/// </summary>
	public interface IProcedureProcess<TCtx, in TParam> : IProcedureProcess
		where TCtx : IProcedureContext
	{
		/// <summary>
		/// 執行程序，並加入參數
		/// </summary>
		TCtx Process(TCtx ctx, TParam param);
	}
}