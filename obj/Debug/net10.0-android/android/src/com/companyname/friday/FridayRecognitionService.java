package com.companyname.friday;


public class FridayRecognitionService
	extends android.speech.RecognitionService
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onStartListening:(Landroid/content/Intent;Landroid/speech/RecognitionService$Callback;)V:GetOnStartListening_Landroid_content_Intent_Landroid_speech_RecognitionService_Callback_Handler\n" +
			"n_onStopListening:(Landroid/speech/RecognitionService$Callback;)V:GetOnStopListening_Landroid_speech_RecognitionService_Callback_Handler\n" +
			"n_onCancel:(Landroid/speech/RecognitionService$Callback;)V:GetOnCancel_Landroid_speech_RecognitionService_Callback_Handler\n" +
			"";
		mono.android.Runtime.register ("Friday.FridayRecognitionService, Friday", FridayRecognitionService.class, __md_methods);
	}

	public FridayRecognitionService ()
	{
		super ();
		if (getClass () == FridayRecognitionService.class) {
			mono.android.TypeManager.Activate ("Friday.FridayRecognitionService, Friday", "", this, new java.lang.Object[] {  });
		}
	}

	public void onStartListening (android.content.Intent p0, android.speech.RecognitionService.Callback p1)
	{
		n_onStartListening (p0, p1);
	}

	private native void n_onStartListening (android.content.Intent p0, android.speech.RecognitionService.Callback p1);

	public void onStopListening (android.speech.RecognitionService.Callback p0)
	{
		n_onStopListening (p0);
	}

	private native void n_onStopListening (android.speech.RecognitionService.Callback p0);

	public void onCancel (android.speech.RecognitionService.Callback p0)
	{
		n_onCancel (p0);
	}

	private native void n_onCancel (android.speech.RecognitionService.Callback p0);

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
