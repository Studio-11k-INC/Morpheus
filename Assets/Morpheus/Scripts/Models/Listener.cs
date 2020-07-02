using Boo.Lang;
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Listener 
{
	public eMessages Message;
	public UnityEvent Handler;	
}
