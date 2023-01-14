namespace Application.Models;

public record GameMessage(string TeamId,
                          int Tick,
                          Map Map,
                          string[] Teams,
                          Dictionary<string, Team> TeamInfos,
                          Dictionary<string, PlayArea> PlayAreas,
                          int Round,
                          int TicksUntilPayout,
                          string[] LastTickErrors,
                          Shop.Shop Shop,
                          Constants Constants);
