using System;
using System.ComponentModel;

namespace MTCG;


[Controller]
public class SettingsController : IController
{
    public SettingsController(IRequest request) : base(request) { }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    [Route("/settings/language/{lang:alpha}", HTTPMethod.PUT, Role.USER | Role.ADMIN)]
    public IResponse ChangeLanguageSettings(string lang)
    {
        try
        {
            if (!resConfig.TranslationExists(lang)) return new Response<string>(403, resConfig["SETTINGS_LANG_UNKNOWN"]);

            resConfig.SetLanguage(lang);
            LoggedInUser.Language = lang;

            return new Response<string>(200, resConfig["SETTINGS_LANG_CHANGE_SUCC"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(403, resConfig["INT_SVR_ERR"]);
        }
    }
}