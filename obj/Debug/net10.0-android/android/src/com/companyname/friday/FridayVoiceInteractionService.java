package com.companyname.friday;


public class FridayVoiceInteractionService
	extends android.service.voice.VoiceInteractionService
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onReady:()V:GetOnReadyHandler\n" +
			"n_onStartCommand:(Landroid/content/Intent;II)I:GetOnStartCommand_Landroid_content_Intent_IIHandler\n" +
			"n_onShutdown:()V:GetOnShutdownHandler\n" +
			"n_onDestroy:()V:GetOnDestroyHandler\n" +
			"";
		mono.android.Runtime.register ("Friday.FridayVoiceInteractionService, Friday", FridayVoiceInteractionService.class, __md_methods);
	}

	public FridayVoiceInteractionService ()
	{
		super ();
		if (getClass () == FridayVoiceInteractionService.class) {
			mono.android.TypeManager.Activate ("Friday.FridayVoiceInteractionService, Friday", "", this, new java.lang.Object[] {  });
		}
	}

	public void onReady ()
	{
		n_onReady ();
	}

	private native void n_onReady ();

	public int onStartCommand (android.content.Intent p0, int p1, int p2)
	{
		return n_onStartCommand (p0, p1, p2);
	}

	private native int n_onStartCommand (android.content.Intent p0, int p1, int p2);

	public void onShutdown ()
	{
		n_onShutdown ();
	}

	private native void n_onShutdown ();

	public void onDestroy ()
	{
		n_onDestroy ();
	}

	private native void n_onDestroy ();

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
