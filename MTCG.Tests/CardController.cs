using System;

namespace MTCG;

[Controller]
public class CardController : IController
{
    public CardController(IRequest request) : base(request) {}

    /// 08.12.2023 17:00
    /// INFO this method implementation is only temporary
    /// here until authentication is implemented and in use
    /// should then work by checking the Authentication token
    /// and getting the users uuid from DB
    [Route("/cards", HTTPMethod.GET)]
    public IResponse GetAllCardsForUserByUserId(Guid userid)
    {
        try
        {
            
        }
    }
}