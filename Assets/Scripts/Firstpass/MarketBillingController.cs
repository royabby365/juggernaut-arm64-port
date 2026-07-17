using UnityEngine;

public class MarketBillingController : MonoBehaviour
{
	private enum Managed
	{
		MANAGED,
		UNMANAGED
	}

	private class CatalogEntry
	{
		public string sku;

		public Managed managed;

		public CatalogEntry(string sku, Managed managed)
		{
			this.sku = sku;
			this.managed = managed;
		}
	}

	private CatalogEntry[] CATALOG = new CatalogEntry[6]
	{
		new CatalogEntry("sword_001", Managed.MANAGED),
		new CatalogEntry("potion_001", Managed.UNMANAGED),
		new CatalogEntry("android.test.purchased", Managed.UNMANAGED),
		new CatalogEntry("android.test.canceled", Managed.UNMANAGED),
		new CatalogEntry("android.test.refunded", Managed.UNMANAGED),
		new CatalogEntry("android.test.item_unavailable", Managed.UNMANAGED)
	};

	private string bankBuyResultLabel = string.Empty;

	private void Start()
	{
		JavaVM.AttachCurrentThread();
		MarketBillingPlugin.SetCallbackHandler("CallJavaCode", "PurchaseRequestStatus");
	}

	public void PurchaseRequestStatus(string message)
	{
		MonoBehaviour.print("PurchaseRequestStatus: " + message);
		bankBuyResultLabel = "Purchase status: " + message;
	}

	private void OnGUI()
	{
		int num = 10;
		int num2 = 16;
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
		GUI.Label(new Rect(15f, num, 450f, 20f), bankBuyResultLabel);
		num += 20 + num2;
		for (int i = 2; i < CATALOG.Length; i++)
		{
			if (GUI.Button(new Rect(15f, num, 450f, 50f), "Buy " + CATALOG[i].sku))
			{
				MarketBillingPlugin.RequestPurchase(CATALOG[i].sku);
			}
			num += 50 + num2;
		}
	}
}
