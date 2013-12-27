using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.AssetsLibrary;

namespace ImagePicker
{
	public partial class ImagePickerViewController : UICollectionViewController
	{
		static NSString picCellId = new NSString ("PicCell");

		List<IPhotoItem> PhotoItems = new List<IPhotoItem>();
		List<IPhotoItem> SelectedItems = new List<IPhotoItem>();
		//AddPrivacyViewController controllerAddPrivacy;
		DateTime m_dteDidLoad;
		UIAlertView m_alertView;

		ALAssetsLibrary m_AssetsLibrary = null;
		object m_AssetsLibLock = new object();

		public ImagePickerViewController (UICollectionViewLayout layout) : base (layout)
		{

			AssetsWrapper aw = new AssetsWrapper ();
			aw.BatchLoaded += AssetsWrapper_BatchLoaded;
			aw.Finished += AssetsWrapper_Finished;

		}

		#region UIView

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			m_dteDidLoad = DateTime.Now;

			CollectionView.RegisterClassForCell (typeof(PhotoItemCell), picCellId);
			CollectionView.AllowsMultipleSelection = true;
			CollectionView.BackgroundColor = UIColor.White;

			UIApplication.SharedApplication.SetStatusBarStyle (UIStatusBarStyle.Default, true);

		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			InvokeOnMainThread(delegate
				{
					Console.WriteLine("PhotoPicker :: ViewWillAppear");

					CollectionView.PerformBatchUpdates(delegate {

						lock(SelectedItems) {
							foreach(var item in SelectedItems)
							{
								Console.WriteLine("Deselect programatically " + PhotoItems.IndexOf(item));

								var indexPath = NSIndexPath.FromRowSection(PhotoItems.IndexOf(item), 0);
								CollectionView.DeselectItem(indexPath, false);
							}
							SelectedItems.Clear();
						}

					}, null);

				});
		}

		public override bool CanBecomeFirstResponder
		{
			get
			{
				return true;
			}
		}
		#endregion

		#region UICollectionView

		public override int NumberOfSections (UICollectionView collectionView)
		{
			return 1;
		}

		public override int GetItemsCount (UICollectionView collectionView, int section)
		{
			return PhotoItems.Count;
		}

		public override UICollectionViewCell GetCell (UICollectionView collectionView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			var cell = (PhotoItemCell)collectionView.DequeueReusableCell (picCellId, indexPath);

			var animal = PhotoItems [indexPath.Row];

			lock (m_AssetsLibLock) {
				if (m_AssetsLibrary == null) {
					m_AssetsLibrary = new ALAssetsLibrary ();			
				}
			}

    		m_AssetsLibrary.AssetForUrl (animal.Url, 
				delegate(ALAsset asset) {

					var cgImage = asset.Thumbnail;
					var uiImage = new UIImage(cgImage);

					cell.Thumbnail = uiImage;

					//Console.WriteLine("Display for " + animal.Url);
				},
				delegate(NSError error) {

					Console.WriteLine("Error loading an asset: " + error.ToString());

				}
			);

			return cell;
		}

		public override void ItemHighlighted (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = collectionView.CellForItem (indexPath);
			cell.ContentView.BackgroundColor = UIColor.Yellow;
		}

		public override void ItemUnhighlighted (UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = collectionView.CellForItem (indexPath);
			cell.ContentView.BackgroundColor = null;
		}

		public override bool ShouldHighlightItem (UICollectionView collectionView, NSIndexPath indexPath)
		{
			return true;
		}

		public override bool ShouldSelectItem (UICollectionView collectionView, NSIndexPath indexPath)
		{
			if (indexPath != null && indexPath.Row < PhotoItems.Count) {
				var item = PhotoItems [indexPath.Row];
				lock (SelectedItems)
				{
					SelectedItems.Add(item);
				}
			}

			Console.WriteLine ("Selected " + indexPath.Row);
			return true;
		}

		public override bool ShouldDeselectItem (UICollectionView collectionView, NSIndexPath indexPath)
		{
			if (indexPath != null && indexPath.Row < PhotoItems.Count) {
				var item = PhotoItems [indexPath.Row];

				lock(SelectedItems)
				{
					SelectedItems.Remove (item);
				}
			}

			Console.WriteLine ("Deselected " + indexPath.Row);
			return true;
		}

		public override bool ShouldShowMenu(UICollectionView collectionView, NSIndexPath indexPath)
		{
			return false;
		}
		#endregion

		#region Asset Library

		void AssetsWrapper_BatchLoaded (object sender, AssetsBatchArgs e)
		{
			var batch = e.Batch;

			Console.WriteLine("AssetsWrapper_BatchLoaded");

			lock (PhotoItems)
			{
				int startIndex = PhotoItems.Count;
				int endIndex = startIndex + batch.Length;

				Console.WriteLine("Merge in [{0}:{1}]", startIndex, endIndex);

				List<NSIndexPath> mergeItems = new List<NSIndexPath>(endIndex - startIndex);
				for(int i = startIndex; i < endIndex; i ++)
				{
					mergeItems.Add(NSIndexPath.FromItemSection(i, 0));
				}

				InvokeOnMainThread(delegate
					{
						CollectionView.PerformBatchUpdates(delegate {

							foreach(NSUrl item in batch)
							{
								PhotoItems.Add(new PhotoItem(item));
							}

							CollectionView.InsertItems(mergeItems.ToArray());


						}, null);

					});

			}
		}

		void AssetsWrapper_Finished (object sender, EventArgs e)
		{
			Console.WriteLine("Loaded {0} pics in {1} ms", PhotoItems.Count, DateTime.Now.Subtract(m_dteDidLoad).Milliseconds);
		}

		#endregion

	}
}

