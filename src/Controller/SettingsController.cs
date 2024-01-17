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

            var userRepo = ServiceProvider.GetDisposable<UserRepository>();
            var user = userRepo.Get(UserId)!;
            resConfig.SetLanguage(lang);
            user.Language = lang;
            userRepo.Update(user);

            return new Response<string>(200, resConfig["SETTINGS_LANG_CHANGE_SUCC"]);
        }
        catch (Exception ex)
        {
            Logger.Err(ex, true);

            return new Response<string>(403, resConfig["INT_SVR_ERR"]);
        }
    }


    [Route("settings/reset", HTTPMethod.POST, Role.ADMIN)]
    public IResponse Reset()
    {
        try
        {
            var userRepo = ServiceProvider.GetDisposable<UserRepository>();
            var cardRepo = ServiceProvider.GetDisposable<CardRepository>();
            var battleRepo = ServiceProvider.GetDisposable<BattleRepository>();
            var battleLogRepo = ServiceProvider.GetDisposable<BattleLogRepository>();

            cardRepo.DeleteAll();
            battleRepo.DeleteAll();

            return new Response<string>(200, "Reset successfull.");
        }
        catch (Exception ex)
        {
            Logger.Err(ex, true);

            return new Response<string>(403, resConfig["INT_SVR_ERR"]);
        }
    }
}