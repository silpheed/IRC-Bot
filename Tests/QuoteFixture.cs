using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Bot.Tests
{
	[TestFixture]
	public class QuoteFixture
	{
		[Test]
		public void SplitQuote_blank()
		{
			IList<string> result = Quote.SplitQuote(String.Empty);
			Assert.AreEqual(0, result.Count);
			result = Quote.SplitQuote(null);
			Assert.AreEqual(0, result.Count);
			result = Quote.SplitQuote(" ");
			Assert.AreEqual(0, result.Count);
		}

		[Test]
		public void SplitQuote_basic()
		{
			string input = "<test1> nectarine <test2> sight i've never seen <test3> sweet and mean <test4> human or machine?";
			IList<string> result = Quote.SplitQuote(input);
			Assert.AreEqual(4, result.Count);
			Assert.AreEqual("<test1> nectarine", result[0]);
			Assert.AreEqual("<test2> sight i've never seen", result[1]);
			Assert.AreEqual("<test3> sweet and mean", result[2]);
			Assert.AreEqual("<test4> human or machine?", result[3]);
		}

		[Test]
		public void SplitQuote_tricky()
		{
			string input = "now you're crushed <test2> baby what's < the rush? <test3> flesh and < > stone <<test4> which to leave alone?<";
			IList<string> result = Quote.SplitQuote(input);
			Assert.AreEqual(4, result.Count);
			Assert.AreEqual("now you're crushed", result[0]);
			Assert.AreEqual("<test2> baby what's < the rush?", result[1]);
			Assert.AreEqual("<test3> flesh and < > stone <", result[2]);
			Assert.AreEqual("<test4> which to leave alone?<", result[3]);
		}

		[Test]
		public void SplitQuote_hard()
		{
			string input = "<and it hurts<test2> nectarine<test3>and it hurts<test4>necta>rine<test5>yes, it hur<ts";
			IList<string> result = Quote.SplitQuote(input);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("<and it hurts", result[0]);
			Assert.AreEqual("<test2> nectarine", result[1]);
			Assert.AreEqual("<test3>and it hurts", result[2]);
			Assert.AreEqual("<test4>necta>rine", result[3]);
			Assert.AreEqual("<test5>yes, it hur<ts", result[4]);
		}

		[Test]
		public void SplitQuote_can_be_restricted()
		{
			string input = "<test1> test1 <test2> test2 <test3> test3 <test4> test4 <test5> test5 <test6> test6 <test7> test7 <test8> test8 <test9> test9 <test10> test10 <test11> test11 <test12> test12";
			IList<string> result = Quote.SplitQuote(input);
			Assert.AreEqual(10, result.Count);
			Assert.AreEqual("<test1> test1", result[0]);
			Assert.AreEqual("<test2> test2", result[1]);
			Assert.AreEqual("<test3> test3", result[2]);
			Assert.AreEqual("<test4> test4", result[3]);
			Assert.AreEqual("<test5> test5", result[4]);
			Assert.AreEqual("<test6> test6", result[5]);
			Assert.AreEqual("<test7> test7", result[6]);
			Assert.AreEqual("<test8> test8", result[7]);
			Assert.AreEqual("<test9> test9", result[8]);
			Assert.AreEqual("<test10> test10 <test11> test11 <test12> test12", result[9]);
		}
	}
}
