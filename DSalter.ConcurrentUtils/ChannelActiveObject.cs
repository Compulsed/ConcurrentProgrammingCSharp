using System;
using System.Collections.Generic;

namespace DSalter.ConcurrentUtils
{
	/// <summary>
	/// An ChannelActiveObject is a channel with a single thread that continually
	/// </summary>
	public abstract class ChannelActiveObject<T> : ActiveObject
	{
		/// <summary>
		/// You are able to reference this channel and add data to it via the CAO client
		/// </summary>
		public readonly Channel<T> _inputChannel;

		/// <summary>
		/// Creates an active channel, if a input channel is given it uses that channel
		/// 	if not it creates its own one. 
		/// </summary>
		/// <param name="inputChannel">Input channel.</param>
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

		/// <summary>
		/// Client must create their own method to process data
		/// 	off of the channel
		/// </summary>
		/// <param name="passedData">Passed data.</param>
		protected abstract void Process (T passedData);
	}
}

