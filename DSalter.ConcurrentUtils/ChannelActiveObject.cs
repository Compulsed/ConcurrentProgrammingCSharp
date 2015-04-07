using System;
using System.Collections.Generic;

namespace DSalter.ConcurrentUtils
{
	public abstract class ChannelActiveObject<T> : ActiveObject
	{
		public readonly Channel<T> _inputChannel;

		public ChannelActiveObject (Channel<T> inputChannel = null)
		{
			if (inputChannel == null)
				_inputChannel = new Channel<T> ();
			else
				_inputChannel = inputChannel;
		}

		protected override void Run ()
		{
			while (true)
				Process (_inputChannel.Take ());
		}

		protected abstract void Process (T passedData);
	}
}

