using System.Collections.Generic;
using Meebey.SmartIrc4net;

//todo
//link (like quote)

namespace Bot
{
	public class Bot
	{
		private IrcClient _irc = new IrcClient();
		private IEnumerable<IBottable> _modules;

		public Bot()
		{
			//admin must always come first
			_modules = new List<IBottable>
			           	{
							new Admin(_irc),
			           		new Quote(_irc),
							new Rainbow(_irc),
							new Big(_irc),
							new Puppet(_irc),
							new LOTD(_irc)
			           	};

			_irc.OnReadLine += OnReadLine;
			_irc.SendDelay = 500;
		}

		private void OnReadLine(object sender, ReadLineEventArgs e)
		{
			IrcMessageData data = _irc.MessageParser(e.Line);
			
			if (!_irc.IsMe(data.Nick))
				foreach (var bottable in Modules)
					if (!bottable.OnReadLine(data))
						break;

			//_irc.ListenOnce();
		}

        public IEnumerable<IBottable> Modules
		{
			get { return _modules; }
		}

	}

	class Program
	{
		static void Main(string[] args)
		{
			Bot bot = new Bot();
			
			foreach (var bottable in bot.Modules)
				bottable.OnConsoleLine(args);
		}
	}
}