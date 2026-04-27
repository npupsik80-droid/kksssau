using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace KkmFactory;

public class DictionarySem<TKey, TValue> : Dictionary<TKey, TValue>
{
	private SemaphoreSlim _Semaphore;

	[IgnoreDataMember]
	public SemaphoreSlim Semaphore
	{
		get
		{
			if (_Semaphore == null)
			{
				_Semaphore = new SemaphoreSlim(1);
			}
			return _Semaphore;
		}
	}
}
