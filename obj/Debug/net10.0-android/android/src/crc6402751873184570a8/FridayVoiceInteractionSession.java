package crc6402751873184570a8;


public class FridayVoiceInteractionSession
	extends android.service.voice.VoiceInteractionSession
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("Friday.FridayVoiceInteractionSession, Friday", FridayVoiceInteractionSession.class, __md_methods);
	}

	public FridayVoiceInteractionSession (android.content.Context p0, android.os.Handler p1)
	{
		super (p0, p1);
		if (getClass () == FridayVoiceInteractionSession.class) {
			mono.android.TypeManager.Activate ("Friday.FridayVoiceInteractionSession, Friday", "Android.Content.Context, Mono.Android:Android.OS.Handler, Mono.Android", this, new java.lang.Object[] { p0, p1 });
		}
	}

	public FridayVoiceInteractionSession (android.content.Context p0)
	{
		super (p0);
		if (getClass () == FridayVoiceInteractionSession.class) {
			mono.android.TypeManager.Activate ("Friday.FridayVoiceInteractionSession, Friday", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
		}
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
