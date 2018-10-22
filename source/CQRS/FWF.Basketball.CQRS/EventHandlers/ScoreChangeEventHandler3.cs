using FWF.Basketball.CQRS.Events;
using FWF.Basketball.Logic.Data;
using FWF.CQRS;
using FWF.Logging;
using FWF.Security;

namespace FWF.Basketball.CQRS.EventHandlers
{
    internal class ScoreChangeEventHandler3 : IEventHandler<ScoreChangeEvent>
    {

        private readonly IGameDataRepository _gameDataRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILog _log;

        public ScoreChangeEventHandler3(
            IGameDataRepository gameDataRepository,
            IEventPublisher eventPublisher,
            ILogFactory logFactory
            )
        {
            _gameDataRepository = gameDataRepository;
            _eventPublisher = eventPublisher;

            _log = logFactory.CreateForType(this);
        }

        public void Handle(ISecurityContext securityContext, ScoreChangeEvent eventInstance)
        {
            // Whenever a score changes, this effects other calculations
            // Using the event-driven framework - ensure that each calculation(aggregate) is updated as well

            var playerFantasy = _gameDataRepository.FirstOrDefault<PlayerFantasy>(x => x.Id == eventInstance.PlayerId);

            if (playerFantasy.IsNull())
            {
                playerFantasy = new PlayerFantasy 
                {
                    Id = eventInstance.PlayerId.GetValueOrDefault(),
                    Name = eventInstance.GameName,
                    TeamId = eventInstance.TeamId.GetValueOrDefault(),
                    FantasyPoints = 0
                };
                using (var writeContext = _gameDataRepository.BeginWrite())
                {
                    writeContext.Insert(playerFantasy);
                }
            }

            // Assume that a fantasy point is incremented by 1 for every score
            if (eventInstance.Points >= 3)
            {
                playerFantasy.FantasyPoints += 5;
            }
            else if (eventInstance.Points >= 2)
            {
                playerFantasy.FantasyPoints += 2;
            }
            else
            {
                // Free throws are worth nothing
            }

            // Save in local repository 

            using (var writeContext = _gameDataRepository.BeginWrite())
            {
                writeContext.Update(playerFantasy);
            }

            // Publish new event of player detail update

            _eventPublisher.Publish(
                new PlayerFantasyChangeEvent
                {
                    Id = playerFantasy.Id,
                    Name = playerFantasy.Name,
                    TeamId = playerFantasy.TeamId,
                    FantasyPoints = playerFantasy.FantasyPoints,
                }
                );

        }
    }
}
