using System.Collections.Generic;

namespace KkmFactory;

public class EvotorStoresDevices
{
	public class EvotorStoreDevice
	{
		public string id;

		public string name;

		public string user_id;

		public string store_id;

		public string created_at;

		public string updated_at;
	}

	public List<EvotorStoreDevice> items;
}
