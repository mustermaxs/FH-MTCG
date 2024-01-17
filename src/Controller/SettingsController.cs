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

            var userRepo = ServiceProvider.GetFreshInstance<UserRepository>();
            var user = userRepo.Get(UserId)!;

            if (!resConfig.SetLanguage(lang))
                return new Response<string>(404, resConfig["SETTINGS_LANG_UNKNOWN"]);

            user.Language = lang;
            userRepo.Update(user);

            SessionManager.TryGetSessionById(request.SessionId, out Session? session);
            SessionManager.UpdateSession(session.AuthToken, ref session!);
            resConfig = ServiceProvider.Get<ResponseTextTranslator>();
            resConfig.SetLanguage(user.Language);
            session.User = user;

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
            var userRepo = ServiceProvider.GetFreshInstance<UserRepository>();
            var cardRepo = ServiceProvider.GetFreshInstance<CardRepository>();
            var battleRepo = ServiceProvider.GetFreshInstance<BattleRepository>();
            var battleLogRepo = ServiceProvider.GetFreshInstance<BattleLogRepository>();

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