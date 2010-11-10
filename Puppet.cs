using System;
using Meebey.SmartIrc4net;

namespace Bot
{
	public class Puppet : IBottable
	{
		private IrcClient _irc;

		public Puppet(IrcClient irc)
		{
			_irc = irc;
		}

        public bool OnReadLine(IrcMessageData data)
		{
			if ((data.Type != ReceiveType.ChannelMessage) && (data.Type != ReceiveType.QueryMessage))
				return true;
	
			if ((data.MessageArray.Length > 1) && ((data.MessageArray[0].ToLower() == "!nick") || (data.MessageArray[0].ToLower() == "!name"))) {
				_irc.RfcNick(data.MessageArray[1]);
				return true;
			}

			if ((data.Type == ReceiveType.ChannelMessage) && (data.MessageArray[0] == "!help")) {
				DisplayHelp(data);
				return true;
			}

        	if (data.Type != ReceiveType.QueryMessage)
				return true;

			if (data.MessageArray.Length > 0) {
				_irc.SendMessage(SendType.Message, Admin._channel, data.Message);
				Console.WriteLine(data.Nick + " " + data.Message);
			}

        	return true;
		}

		public void OnConsoleLine(string[] args)
		{
		}

        private void DisplayHelp(IrcMessageData data)
		{
			_irc.SendMessage(SendType.Message, data.Nick, "04!nick <text> or 04!name <text> change bot's name (channel or query).");
			_irc.SendMessage(SendType.Message, data.Nick, "04<text> make bot talk (query only).");
		}
	}
}