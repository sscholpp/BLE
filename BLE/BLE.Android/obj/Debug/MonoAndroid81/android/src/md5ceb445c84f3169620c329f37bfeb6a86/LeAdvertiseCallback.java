package md5ceb445c84f3169620c329f37bfeb6a86;


public class LeAdvertiseCallback
	extends android.bluetooth.le.AdvertiseCallback
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("BLE.Droid.LeAdvertiseCallback, BLE.Android", LeAdvertiseCallback.class, __md_methods);
	}


	public LeAdvertiseCallback ()
	{
		super ();
		if (getClass () == LeAdvertiseCallback.class)
			mono.android.TypeManager.Activate ("BLE.Droid.LeAdvertiseCallback, BLE.Android", "", this, new java.lang.Object[] {  });
	}

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
