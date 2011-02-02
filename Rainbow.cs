using System;
using System.Collections.Generic;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot
{
	public class Rainbow : IBottable
	{
		private IrcClient _irc;
		private static IList<string> _colours = new List<string> { "07", "04", "08", "09", "11", "12", "06" };
		
		public Rainbow(IrcClient irc)
		{
			_irc = irc;
		}

		public bool OnReadLine(IrcMessageData data)
		{
			if (data.Type != ReceiveType.ChannelMessage)
				return true;

			if ((data.MessageArray[0] == "!rainbow") || (data.MessageArray[0] == "!r"))
				Fabulise(data);

			if ((data.MessageArray[0] == "!doublerainbow") || (data.MessageArray[0] == "!dr")) {
				Fabulise(data);
				Fabulise(data);
			}

			if (data.MessageArray[0] == "!help")
				DisplayHelp(data);

			return true;
		}

		public void OnConsoleLine(string[] args)
		{
		}

		private void Fabulise(IrcMessageData data)
		{
			if (data.MessageArray.Length < 2)
				return;

			int colNum = 0;
			string input = data.Message.Split(null, 2)[1].Replace("", String.Empty);
			StringBuilder output = new StringBuilder();

			foreach (char c in input) {
				if (c == ' ') {
					output.Append(c);
					continue;
				}
				output.Append(WrapTextInColour(c));
				colNum++;
				if (colNum >= _colours.Count)
					colNum = 0;
			}

			_irc.SendMessage(SendType.Message, data.Channel, output.ToString());
		}

		public string WrapTextInColour(char text)
		{
			return WrapTextInColour(Convert.ToString(text), false, false, false);
		}

		public static int ColourIndex
		{
			get;
			set;
		}

		public static string WrapTextInColour(string text, bool terminate, bool background, bool holdColour)
		{
			if (!holdColour)
				ColourIndex++;

			if ((ColourIndex >= _colours.Count) || (ColourIndex < 0))
				ColourIndex = 0;

			StringBuilder coloured = new StringBuilder();
			coloured.Append('');
			if (background)
				coloured.Append("00,");
			coloured.Append(_colours[ColourIndex]);
			coloured.Append(text);
			if (terminate)
				coloured.Append('');

			return coloured.ToString();
		}

		private void DisplayHelp(IrcMessageData data)
		{
			_irc.SendMessage(SendType.Message, data.Nick, "04!rainbow <text> or 04!r <text> fabulise your <text>.");
			_irc.SendMessage(SendType.Message, data.Nick, "04!doublerainbow <text> or 04!dr <text> fabulise your <text> twice.");
		}

	}
}
