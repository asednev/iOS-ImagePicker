using System;
using MonoTouch.Foundation;

namespace ImagePicker
{
	public class PhotoItem : IPhotoItem
	{
		NSUrl m_Url;

		public PhotoItem (NSUrl assetUrl)
		{
			m_Url = assetUrl;
		}

		#region IPhotoItem implementation

		public NSUrl Url
		{
			get { return m_Url; }
		}

		#endregion
	}
}

