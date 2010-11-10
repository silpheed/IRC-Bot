using Meebey.SmartIrc4net;

namespace Bot
{
	public interface IBottable
	{
		/// <returns>Returns true to pass text on to other modules, false for don't.</returns>
		bool OnReadLine(IrcMessageData data);
		/// <summary>
		/// Called once upon app start.
		/// </summary>
		void OnConsoleLine(string[] args);
	}
}