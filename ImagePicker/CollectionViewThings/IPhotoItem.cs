using System;
using MonoTouch.Foundation;

namespace ImagePicker
{
	public interface IPhotoItem
	{
		NSUrl Url { get; }
	}
}

