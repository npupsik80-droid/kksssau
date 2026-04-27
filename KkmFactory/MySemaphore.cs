using System.Runtime.Serialization;
using System.Threading;

namespace KkmFactory;

public class MySemaphore
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
