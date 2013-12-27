using System;
using MonoTouch.Foundation;
using MonoTouch.AssetsLibrary;
using System.Collections.Generic;
using System.Threading;

namespace ImagePicker
{
	public class AssetsWrapper
	{
		const int BATCH_SIZE = 4;

		ALAssetsLibrary m_Lib = new ALAssetsLibrary();
		List<NSUrl> m_AssetsBatch = new List<NSUrl>(BATCH_SIZE);

		public event EventHandler Finished;
		public event EventHandler<AssetsBatchArgs> BatchLoaded;

		public AssetsWrapper ()
		{
			ThreadPool.QueueUserWorkItem (delegate {

				m_Lib.Enumerate (ALAssetsGroupType.SavedPhotos, this.GroupsEnumeration, this.GroupsEnumerationFailure);

			});
		}

		void GroupsEnumeration(ALAssetsGroup group, ref bool stop)
		{
			if (group != null) {

				stop = false;

				group.SetAssetsFilter (ALAssetsFilter.AllPhotos);

				group.Enumerate (NSEnumerationOptions.Reverse, this.AssertsEnumeration);
			}
		}

		void GroupsEnumerationFailure(NSError error)
		{
			if (error != null) {
				Console.WriteLine ("Group enumeration failed: " + error.LocalizedDescription);
			}
		}

		void AssertsEnumeration(ALAsset asset, int index, ref bool stop)
		{
			if (asset == null) {

				lock (m_AssetsBatch)
				{
					if(m_AssetsBatch.Count > 0)
					{
						if(BatchLoaded != null)
							BatchLoaded(this, new AssetsBatchArgs() { Batch = m_AssetsBatch.ToArray() });
					}
					m_AssetsBatch.Clear();
				}

				if (Finished != null)
					Finished (this, EventArgs.Empty);
			}
			else
			{
				lock (m_AssetsBatch)
				{
					m_AssetsBatch.Add(asset.AssetUrl);

					if(m_AssetsBatch.Count == BATCH_SIZE)
					{
						if(BatchLoaded != null)
							BatchLoaded(this, new AssetsBatchArgs() { Batch = m_AssetsBatch.ToArray() });

						m_AssetsBatch.Clear();
					}
				}
			}
		}
	}


	public class AssetsBatchArgs : EventArgs
	{
		public AssetsBatchArgs() : base()
		{
		}

		public NSUrl[] Batch { get; set; }
	}
}

