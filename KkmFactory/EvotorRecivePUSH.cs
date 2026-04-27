using System;
using System.Collections.Generic;

namespace KkmFactory;

public class EvotorRecivePUSH
{
	public class tdetails
	{
		public DateTime active_till;

		public DateTime created_at;

		public DateTime rejected_at;

		public DateTime sent_at;

		public string device_id;

		public string status;
	}

	public List<tdetails> details;

	public string id;

	public DateTime modified_at;

	public string status;
}
