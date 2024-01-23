using Android.App;
using Android.Widget;
using AndroidHUD;
using Com.Cashfree.PG.Api;
using Com.Cashfree.PG.Core.Api;
using Com.Cashfree.PG.Core.Api.Callback;
using Com.Cashfree.PG.Core.Api.Utils;
using Com.Cashfree.PG.UI.Api;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Payment;
using DeepSoundClient.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace DeepSound.Payment
{
    public class InitCashFreePayment : Object, ICFCheckoutResponseCallback
    {
        private readonly Activity ActivityContext;
        private string Price;
        private CashFreeObject CashFreeObject;

        public InitCashFreePayment(Activity context)
        {
            try
            {
                ActivityContext = context;

                CFPaymentGatewayService.Initialize(context); // Application Context.
                AnalyticsUtil.SendPaymentEventsToBackend(); // required for error reporting.

                CFPaymentGatewayService.Instance?.SetCheckoutCallback(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DisplayCashFreePayment(CashFreeObject cashFreeObject, string price)
        {
            ActivityContext.RunOnUiThread(() =>
            {
                try
                {
                    CashFreeObject = cashFreeObject;
                    Price = price;

                    CFSession.Environment cfEnvironment = ListUtils.SettingsSiteList?.CashfreeMode switch
                    {
                        "SandBox" => CFSession.Environment.Sandbox,
                        "Live" => CFSession.Environment.Production,
                        _ => CFSession.Environment.Sandbox
                    };

                    CFSession cfSession = new CFSession.CFSessionBuilder()
                        .SetEnvironment(cfEnvironment)
                        .SetPaymentSessionID(CashFreeObject.OrderLinkObject.PaymentSessionId)
                        .SetOrderId(CashFreeObject.OrderId)
                        .Build();

                    //CFPaymentComponent cfPaymentComponent = new CFPaymentComponent.CFPaymentComponentBuilder()
                    //    .Add(CFPaymentComponent.CFPaymentModes.Card)
                    //    .Add(CFPaymentComponent.CFPaymentModes.Upi)
                    //    .Add(CFPaymentComponent.CFPaymentModes.Wallet)
                    //    .Build();

                    CFTheme cfTheme = new CFTheme.CFThemeBuilder()
                        .SetNavigationBarBackgroundColor(AppSettings.MainColor)
                        .SetNavigationBarTextColor("#ffffff")
                        .SetButtonBackgroundColor(AppSettings.MainColor)
                        .SetButtonTextColor("#ffffff")
                        .SetPrimaryTextColor("#000000")
                        .SetSecondaryTextColor("#000000")
                        .Build();

                    CFDropCheckoutPayment cfDropCheckoutPayment = new CFDropCheckoutPayment.CFDropCheckoutPaymentBuilder()
                        .SetSession(cfSession)
                        //By default all modes are enabled. If you want to restrict the payment modes uncomment the next line
                        //.SetCFUIPaymentModes(cfPaymentComponent)
                        .SetCFNativeCheckoutUITheme(cfTheme)
                        .Build();

                    CFPaymentGatewayService gatewayService = CFPaymentGatewayService.Instance;
                    gatewayService.DoPayment(ActivityContext, cfDropCheckoutPayment);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        public void OnPaymentFailure(CFErrorResponse cfErrorResponse, string orderId)
        {
            try
            {
                //Error  
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnPaymentVerify(string orderId)
        {
            try
            {
                //verifyPayment triggered 
                // wael update after update api system 
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Payments.CashFreeGetStatusAsync(CashFreeObject.AppId, ListUtils.SettingsSiteList?.CashfreeSecretKey ?? "", orderId, ListUtils.SettingsSiteList?.CashfreeMode);
                    if (apiStatus == 200)
                    {
                        if (respond is CashFreeGetStatusObject result)
                            await CashFree(result, "pay");
                    }
                    else
                        Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task CashFree(CashFreeGetStatusObject statusObject, string request)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"txStatus", statusObject.TxStatus},
                        {"orderId", CashFreeObject.OrderId},
                        {"orderAmount", statusObject.OrderAmount},
                        {"referenceId", statusObject.ReferenceId},
                        {"paymentMode", statusObject.PaymentMode},
                        {"txMsg", statusObject.TxMsg},
                        {"txTime", statusObject.TxTime},
                        {"signature", CashFreeObject.Signature},
                        {"price", Price}
                    };
                    //wael change after update system
                    var (apiStatus, respond) = await RequestsAsync.Payments.TopWalletPaypalAsync(Price);
                    //var (apiStatus, respond) = await RequestsAsync.Payments.CashFreeAsync(request, keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                            AndHUD.Shared.Dismiss();

                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Short)?.Show();

                            break;
                        default:
                            Methods.DisplayAndHudErrorResult(ActivityContext, respond);
                            break;
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                AndHUD.Shared.Dismiss();
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}