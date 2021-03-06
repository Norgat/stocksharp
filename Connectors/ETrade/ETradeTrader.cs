namespace StockSharp.ETrade
{
	using System;
	using System.Linq;
	using System.Security;

	using Ecng.Common;
	using Ecng.ComponentModel;

	using StockSharp.Algo;
	using StockSharp.BusinessEntities;
	using StockSharp.ETrade.Native;

	/// <summary>
	/// The interface <see cref="IConnector"/> implementation which provides a connection to the ETrade.
	/// </summary>
	[Icon("ETrade_logo.png")]
	public class ETradeTrader : Connector
	{
		/// <summary>
		/// Key.
		/// </summary>
		public string ConsumerKey
		{
			get { return NativeAdapter.ConsumerKey; }
			set { NativeAdapter.ConsumerKey = value; }
		}

		/// <summary>
		/// Secret.
		/// </summary>
		public string ConsumerSecret
		{
			get { return NativeAdapter.ConsumerSecret.To<string>(); }
			set { NativeAdapter.ConsumerSecret = value.To<SecureString>(); }
		}

		/// <summary>
		/// OAuth access token. Required to restore connection. Saved AccessToken can be valid until EST midnight.
		/// </summary>
		public OAuthToken AccessToken
		{
			get { return NativeAdapter.AccessToken; }
			set { NativeAdapter.AccessToken = value; }
		}

		/// <summary>
		/// Verification code, received by user in browser, after confirming program's permission to work.
		/// </summary>
		public string VerificationCode
		{
			get { return NativeAdapter.VerificationCode; }
			set { NativeAdapter.VerificationCode = value; }
		}

		/// <summary>
		/// Sandbox mode.
		/// </summary>
		public bool Sandbox
		{
			get { return NativeAdapter.Sandbox; }
			set { NativeAdapter.Sandbox = value; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ETradeTrader"/>.
		/// </summary>
		public ETradeTrader()
		{
			CreateAssociatedSecurity = true;

			Adapter.InnerAdapters.Add(new ETradeMessageAdapter(TransactionIdGenerator));

			ReConnectionSettings.TimeOutInterval = TimeSpan.FromMinutes(5);
			ReConnectionSettings.Interval = TimeSpan.FromMinutes(1);
			ReConnectionSettings.AttemptCount = 5;
		}

		private ETradeMessageAdapter NativeAdapter
		{
			get { return Adapter.InnerAdapters.OfType<ETradeMessageAdapter>().First(); }
		}

		/// <summary>
		/// Set own authorization mode (the default is browser uses).
		/// </summary>
		/// <param name="method">ETrade authorization method.</param>
		public void SetCustomAuthorizationMethod(Action<string> method)
		{
			NativeAdapter.SetCustomAuthorizationMethod(method);
		}
	}
}