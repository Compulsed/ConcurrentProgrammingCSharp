using System;
using DSalter.ConcurrentUtils;

namespace DSalter.Submissions
{
	public class _5_ChannelActiveObjectTest
	{

		public class StringWriter : ChannelActiveObject<String>
		{

			public StringWriter(){}

			protected override void Process (string passedData)
			{
				Console.WriteLine ("StringWriter -> " + passedData);
			}

		}

		public static void TestOne()
		{
			StringWriter one = new StringWriter ();
			one.Start ();

			while (true) {
				one._inputChannel.Put (Console.ReadLine ());
			}
		}

		public static void Main()
		{
			Console.WriteLine("Inside ChannelActiveObjectTest");

			TestOne ();

			return;
		}

	}
}

