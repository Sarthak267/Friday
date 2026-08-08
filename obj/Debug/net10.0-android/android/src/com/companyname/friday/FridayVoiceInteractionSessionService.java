package com.companyname.friday;


public class FridayVoiceInteractionSessionService
	extends android.service.voice.VoiceInteractionSessionService
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onNewSession:(Landroid/os/Bundle;)Landroid/service/voice/VoiceInteractionSession;:GetOnNewSession_Landroid_os_Bundle_Handler\n" +
			"";
		mono.android.Runtime.register ("Friday.FridayVoiceInteractionSessionService, Friday", FridayVoiceInteractionSessionService.class, __md_methods);
	}

	public FridayVoiceInteractionSessionService ()
	{
		super ();
		if (getClass () == FridayVoiceInteractionSessionService.class) {
			mono.android.TypeManager.Activate ("Friday.FridayVoiceInteractionSessionService, Friday", "", this, new java.lang.Object[] {  });
		}
	}

	public android.service.voice.VoiceInteractionSession onNewSession (android.os.Bundle p0)
	{
		return n_onNewSession (p0);
	}

	private native android.service.voice.VoiceInteractionSession n_onNewSession (android.os.Bundle p0);

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
