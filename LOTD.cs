using System;
using System.Collections.Generic;
using System.Linq;
using Meebey.SmartIrc4net;

namespace Bot
{
	//links of the day
	public class LOTD : IBottable
	{
		private IrcClient _irc;
		List<string> _links = new List<string> ();
		DateTime _today = DateTime.Now;
		private IList<string> _ignore = new List<string> {"keanu", "kip"};

		public LOTD(IrcClient irc)
		{
			_irc = irc;
		}

		public bool OnReadLine(IrcMessageData data)
		{
			if ((data.Type != ReceiveType.ChannelMessage) || (_ignore.Contains(data.Nick)))
				return true;

			if ((data.MessageArray.Length > 0) && (data.MessageArray[0] == "!links"))
				DisplayLinks(data.Nick);
			
			if (data.MessageArray[0] == "!help")
				DisplayHelp(data);

			if ((data.MessageArray.Length > 0) && (data.Message.Trim().StartsWith("!")))
				return true;
			
			ParseForLink(data);

			return true;
		}

		private void ParseForLink(IrcMessageData data)
		{
			CheckLinkDate();
			_links.AddRange(data.MessageArray.Where(word =>
				(!_links.Contains(word)) &&
				(Uri.IsWellFormedUriString(word, UriKind.Absolute)) && 
					((word.StartsWith("http")) ||
					(word.StartsWith("www")))
				));
		}

		private void DisplayLinks(string nick)
		{
			CheckLinkDate();
			if (_links.Count == 0)
				_irc.SendMessage(SendType.Message, nick, "No links today :-(");
			foreach (string link in _links) {
				_irc.SendMessage(SendType.Message, nick, link);
			}
		}

		private void CheckLinkDate()
		{
			if (null == _links)
				_links = new List<string>();
			if (_today.Date != DateTime.Now.Date) {
				_today = DateTime.Now.Date;
				_links = new List<string>();
			}
		}

		public void OnConsoleLine(string[] args)
		{
		}
        
		private void DisplayHelp(IrcMessageData data)
		{
			_irc.SendMessage(SendType.Message, data.Nick, "04!links see the links pasted today.");
		}

	}
}