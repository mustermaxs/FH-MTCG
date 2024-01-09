using System;
using System.ComponentModel;

namespace MTCG;


[Controller]
public class SettingsController : IController
{
    public SettingsController(IRequest request) : base(request) {}


    [Route("/settings/language/{lang:alpha}", HTTPMethod.PUT, Role.USER | Role.ADMIN | Role.ALL)]
    public IResponse ChangeLanguageSettings(string lang)
    {
        try
        {
            if (!resConfig.TranslationExists(lang))  return new Response<string>(403, "Language unknown.");

            resConfig.SetLanguage(lang);

            return new Response<string>(200, resConfig["SETTINGS_LANG_CHANGE_SUCC"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return new Response<string>(403, "Failed to change language settings.");
        }
    }
}