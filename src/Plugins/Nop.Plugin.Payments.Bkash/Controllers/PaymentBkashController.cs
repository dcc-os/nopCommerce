using Nop.Core;
using Nop.Plugin.Payments.Bkash.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.Bkash.Controllers
{
    public class PaymentBkashController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        public PaymentBkashController(IWorkContext workContext,
            IStoreService storeService, 
            ISettingService settingService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            ILanguageService languageService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._languageService = languageService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var bkashPaymentSettings = _settingService.LoadSetting<BkashPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.DescriptionText = bkashPaymentSettings.DescriptionText;
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
              {
                  locale.DescriptionText = bkashPaymentSettings.GetLocalizedSetting(x => x.DescriptionText, languageId, false, false);
              });
            model.AdditionalFee = bkashPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = bkashPaymentSettings.AdditionalFeePercentage;
            model.ShippableProductRequired = bkashPaymentSettings.ShippableProductRequired;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.DescriptionText_OverrideForStore = _settingService.SettingExists(bkashPaymentSettings, x => x.DescriptionText, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(bkashPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(bkashPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.ShippableProductRequired_OverrideForStore = _settingService.SettingExists(bkashPaymentSettings, x => x.ShippableProductRequired, storeScope);
            }

            return View("~/Plugins/Payments.Bkash/Views/PaymentBkash/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var bkashPaymentSettings = _settingService.LoadSetting<BkashPaymentSettings>(storeScope);

            //save settings
            bkashPaymentSettings.DescriptionText = model.DescriptionText;
            bkashPaymentSettings.AdditionalFee = model.AdditionalFee;
            bkashPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            bkashPaymentSettings.ShippableProductRequired = model.ShippableProductRequired;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.DescriptionText_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(bkashPaymentSettings, x => x.DescriptionText, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(bkashPaymentSettings, x => x.DescriptionText, storeScope);

            if (model.AdditionalFee_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(bkashPaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(bkashPaymentSettings, x => x.AdditionalFee, storeScope);

            if (model.AdditionalFeePercentage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(bkashPaymentSettings, x => x.AdditionalFeePercentage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(bkashPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            if (model.ShippableProductRequired_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(bkashPaymentSettings, x => x.ShippableProductRequired, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(bkashPaymentSettings, x => x.ShippableProductRequired, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            //localization. no multi-store support for localization yet.
            foreach (var localized in model.Locales)
            {
                bkashPaymentSettings.SaveLocalizedSetting(x => x.DescriptionText,
                    localized.LanguageId,
                    localized.DescriptionText);
            }

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var bkashPaymentSettings = _settingService.LoadSetting<BkashPaymentSettings>(_storeContext.CurrentStore.Id);

            var model = new PaymentInfoModel()
            {
                DescriptionText = bkashPaymentSettings.GetLocalizedSetting(x => x.DescriptionText, _workContext.WorkingLanguage.Id)
            };

            return View("~/Plugins/Payments.Bkash/Views/PaymentBkash/PaymentInfo.cshtml", model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }
    }
}
