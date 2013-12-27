using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;

namespace ImagePicker
{
	public class PhotoItemCell : UICollectionViewCell
	{
		UIImageView imageView;

		[Export ("initWithFrame:")]
		public PhotoItemCell (System.Drawing.RectangleF frame) : base (frame)
		{
			BackgroundView = new UIView{BackgroundColor = UIColor.Orange};

			var checkboxImage = UIImage.FromFile (@"checkbox.png");
			var checkboxImageView = new UIImageView (checkboxImage);
			SelectedBackgroundView = checkboxImageView;
			SelectedBackgroundView.Layer.ZPosition = 100;

			ContentView.Layer.ZPosition = 99;
			ContentView.BackgroundColor = UIColor.Black;

			imageView = new UIImageView (new RectangleF (0, 0, AppDelegate.ItemWidth, AppDelegate.ItemHeight));
			imageView.Center = ContentView.Center;

			ContentView.AddSubview (imageView);
		}

		public UIImage Thumbnail {
			set {
				imageView.Image = value;
				ContentView.BringSubviewToFront(imageView);
			}
		}
	}

}

