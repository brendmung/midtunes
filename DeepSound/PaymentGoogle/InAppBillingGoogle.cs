using Android.BillingClient.Api;
using System.Collections.Generic;

namespace DeepSound.PaymentGoogle
{
    public static class InAppBillingGoogle
    {
        public const string Membership = "membership";

        public static readonly List<QueryProductDetailsParams.Product> ListProductSku = new List<QueryProductDetailsParams.Product> // ID Product
        {
            //All products should be of the same product type.
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Membership).SetProductType(BillingClient.IProductType.Inapp).Build(),
        };
    }
}