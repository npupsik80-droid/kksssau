using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace KkmFactory;

public class ListSem<T> : List<T>
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

	public ListSem(int capacity)
		: base(capacity)
	{
	}
}
