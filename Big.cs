using System;
using System.Collections.Generic;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot
{
	public class Big : IBottable
	{
		private IrcClient _irc;
		private int _timeLimitMinutes = 10;
		private IDictionary<string, DateTime> _userLimit = new Dictionary<string, DateTime>();

		public Big(IrcClient irc)
		{
			_irc = irc;
		}

		public bool OnReadLine(IrcMessageData data)
		{
			if (data.Type != ReceiveType.ChannelMessage)
				return true;

			if (data.MessageArray[0] == "!help")
				DisplayHelp(data);

			if (data.MessageArray.Length < 2)
				return true;

			if ((data.MessageArray[0] == "!big") || (data.MessageArray[0] == "!b"))
				Embiggen(data.Message.Split(null, 2)[1], false, data.Channel, data.Ident, data.Host);

			if ((data.MessageArray[0] == "!bigrainbow") || (data.MessageArray[0] == "!br"))
				Embiggen(data.Message.Split(null, 2)[1], true, data.Channel, data.Ident, data.Host);

			return true;
		}

		public void OnConsoleLine(string[] args)
		{
		}

		private bool IsAllowed(string ident, string host)
		{
			string key = ident + "@" + host;

			if (!_userLimit.ContainsKey(key)) {
				_userLimit.Add(key, DateTime.Now);
				return true;
			}
			if (DateTime.Now - _userLimit[key] >= new TimeSpan(0, _timeLimitMinutes, 0)) {
				_userLimit[key] = DateTime.Now;
				return true;
			}

			return false;
		}

		private void DisplayHelp(IrcMessageData data)
		{
			_irc.SendMessage(SendType.Message, data.Nick, "04!big <text> or 04!b <text> grow your <text> by 5 lines!");
			_irc.SendMessage(SendType.Message, data.Nick, "04!bigrainbow <text> or 04!br <text> fabulise your big <text>.");
		}

		private void Embiggen(string input, bool isRainbow, string channel, string ident, string host)
		{
			if ((input.Length > 55) || (!IsAllowed(ident, host))) {
				_irc.SendMessage(SendType.Message, channel, "no");
				return;
			}

			int resetTo = Rainbow.ColourIndex;

			for (int line = 0; line < 5; line++) {
				Rainbow.ColourIndex = resetTo;
				StringBuilder output = new StringBuilder();
				foreach (char c in input) {
					output.Append(FormCharLine(c, line, isRainbow));
					output.Append((char) 160);
				}

				_irc.SendMessage(SendType.Message, channel, output.ToString());
			}
		}

		private string FormCharLine(char input, int line, bool isRainbow)
		{
			StringBuilder output = new StringBuilder();
			string code = GetCharMap(input).Split(new[] {'.'})[line];
			int flip = 1;
			bool holdColour = false;

			//edge case of no colour in a line, bump the colour up one anyway
			if ((code.Length < 2) && (input != (char)32) && (input != (char)160))
				WrapTextInColour(string.Empty, false);

			foreach (char repeat in code) {
				int repeatNum = (int)char.GetNumericValue(repeat);
				flip *= -1;

				if ((flip > 0) && (input != (char)32) && (input != (char)160)) {
					if (isRainbow)
					{
						output.Append(WrapTextInColour(new String((char)160, repeatNum), holdColour));
						holdColour = true;
					}
					else
					{
						output.Append((char)22);
						output.Append((char)160, repeatNum);
						output.Append((char)22);
					}
				}
				else {
			
					output.Append((char) 160, repeatNum);
				}

			}

			

			return output.ToString();
		}

		public string WrapTextInColour(string text, bool holdColour)
		{
			return Rainbow.WrapTextInColour(text, true, true, holdColour);
		}

		private string GetCharMap(char c)
		{
			switch (c)
			{
				case ('a'):
				case ('A'):
					return "222.141.0222.06.0222";
				case ('b'):
				case ('B'):
					return "051.0222.051.0222.051";
				case ('c'):
				case ('C'):
					return "141.0222.024.0222.141";
				case ('d'):
				case ('D'):
					return "051.0222.0222.0222.051";
				case ('e'):
				case ('E'):
					return "06.024.042.024.06";
				case ('f'):
				case ('F'):
					return "06.024.042.024.024";
				case ('g'):
				case ('G'):
					return "141.024.0213.0222.141";
				case ('h'):
				case ('H'):
					return "0222.0222.06.0222.0222";
				case ('i'):
				case ('I'):
					return "06.222.222.222.06";
				case ('j'):
				case ('J'):
					return "06.222.222.222.042";
				case ('k'):
				case ('K'):
					return "0222.02121.042.02121.0222";
				case ('l'):
				case ('L'):
					return "024.024.024.024.06";
				case ('m'):
				case ('M'):
					return "22222.181.022222.022222.0262";
				case ('n'):
				case ('N'):
					return "0332.0422.021212.0224.0233";
				case ('o'):
				case ('O'):
					return "141.0222.0222.0222.141";
				case ('p'):
				case ('P'):
					return "051.0222.051.024.024";
				case ('q'):
				case ('Q'):
					return "142.02221.02221.02221.16";
				case ('r'):
				case ('R'):
					return "051.0222.051.0222.0222";
				case ('s'):
				case ('S'):
					return "141.024.141.42.141";
				case ('t'):
				case ('T'):
					return "06.222.222.222.222";
				case ('u'):
				case ('U'):
					return "0222.0222.0222.0222.141";
				case ('v'):
				case ('V'):
					return "0222.0222.0222.141.222";
				case ('w'):
				case ('W'):
					return "0262.022222.022222.181.22222";
				case ('x'):
				case ('X'):
					return "0232.12121.232.12121.0232";
				case ('y'):
				case ('Y'):
					return "0222.0222.141.222.222";
				case ('z'):
				case ('Z'):
					return "06.42.222.024.06";
				case ('0'):
					return "151.0223.021112.0322.151";
				case ('1'):
					return "222.042.222.222.06";
				case ('2'):
					return "051.42.141.024.06";
				case ('3'):
					return "051.42.141.42.051";
				case ('4'):
					return "024.024.02121.06.321";
				case ('5'):
					return "06.024.051.42.051";
				case ('6'):
					return "141.024.051.0222.141";
				case ('7'):
					return "06.42.321.222.123";
				case ('8'):
					return "141.0222.141.0222.141";
				case ('9'):
					return "141.0222.15.42.141";
				case ((char)32):
				case ((char)160):
					return "4.4.4.4.4";
				case ('`'):
					return "031.22.4.4.4";
				case ('~'):
					return "1321.01231.7.7.7";
				case ('!'):
					return "02.02.02.2.02";
				case ('@'):
					return "161.022112.021113.02222.17";
				case ((char)35):
					return "12121.07.12121.07.12121";
				case ((char)36):
					return "151.02113.151.3112.151";
				case ((char)37):
					return "0132.321.222.123.0231";
				case ('^'):
					return "222.141.0222.6.6";
				case ('&'):
					return "133.02122.1321.02221.1411";
				case ('*'):
					return "12121.232.07.232.12121";
				case ((char)40):
					return "12.021.021.021.12";
				case ((char)41):
					return "021.12.12.12.021";
				case ('-'):
					return "6.6.06.6.6";
				case ('_'):
					return "6.6.6.6.06";
				case ((char)61):
					return "6.06.6.06.6";
				case ('+'):
					return "6.222.06.222.6";
				case ((char)91):
					return "04.022.022.022.04";
				case ((char)93):
					return "04.22.22.22.04";
				case ((char)123):
					return "13.121.022.121.13";
				case ((char)125):
					return "031.121.22.121.031";
				case (':'):
					return "2.02.2.02.2";
				case ((char)59):
					return "3.12.3.12.012";
				case ('"'):
					return "0212.0212.5.5.5";
				case ((char)39):
					return "02.02.2.2.2";
				case ('<'):
					return "43.232.034.232.43";
				case ('>'):
					return "034.232.43.232.034";
				case (','):
					return "3.3.3.12.012";
				case ('.'):
					return "2.2.2.2.02";
				case ((char)47):
					return "42.321.222.123.024";
				case ((char)92):
					return "024.123.222.321.42";
				case ((char)124):
					return "02.02.02.02.02";
				case ((char)161):
					return "02.2.02.02.02";
				case ((char)191):
					return "223.7.133.0232.151";
				case ((char)176):
					return "02.2.2.2.2";
				case ((char)223):
					return "042.02121.051.0222.02121";
				case ((char)254):
					return "024.051.0222.051.024";
				default:
					return "151.0232.331.7.322";
			}
		}
	}
}