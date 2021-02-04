using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationPortal.Helpers
{
	public class AppSession 
	{
		private ISession _session;

		public AppSession(ISession session)
		{
			_session = session;
		}

		public ISession Session { get { return _session; } }


		public OrgUser GetAccount() 
		{
			if (!Session.Keys.Contains(AppSessionKeys.OrgAccount.ToString()) && Session.GetString(AppSessionKeys.OrgAccount.ToString()) != null)
				return null;

			return (OrgUser)JsonConvert.DeserializeObject(Session.GetString(AppSessionKeys.OrgAccount.ToString()));
		}

		public void SetAccount(OrgUser account) 
		{
			Session.SetString(AppSessionKeys.OrgAccount.ToString(), JsonConvert.SerializeObject(account));
		}

		public void RemoveAccount()
		{
			Session.SetString(AppSessionKeys.OrgAccount.ToString(), null);
		}

		public enum AppSessionKeys 
		{
			OrgAccount
		}
	}
}
