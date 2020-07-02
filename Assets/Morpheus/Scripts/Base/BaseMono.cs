using Doozy.Engine;
using System.Collections.Generic;
using UnityEngine;

public class BaseMono : MonoBehaviour
{
	[SerializeField]		
	public List<Listener> Listener;	

	public GameEventMessage EventMessage;
	public System.Object CallbackObject; 
	
	public virtual void Awake()
	{	
		Message.AddListener<GameEventMessage>(OnMessage);
	}

	private void OnMessage(GameEventMessage message)
	{
		if (message != null)
		{
			if (Listener != null)
			{
				foreach(Listener listener in Listener)
				{
					if(message.EventName == listener.Message.ToString())
					{
						if (listener.Handler != null)
						{
							EventMessage = message;
							CallbackObject = message.CustomObject;
							listener.Handler.Invoke();
						}
					}
				}
			}
		}

		return;
	}	
}
   
