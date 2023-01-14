using System.Net.WebSockets;
using System.Text;
using Application.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Application;

public class GameClient
{
    private readonly Bot _bot;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    private GameClient()
    {
        _bot = new Bot();
        _jsonSerializerSettings = new JsonSerializerSettings {
            Converters = { new StringEnumConverter() },
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };
    }

    public static Task Run(CancellationToken cancellationToken)
    {
        return new GameClient().StartGameClient(cancellationToken);
    }

    private async Task StartGameClient(CancellationToken cancellationToken, string address = "127.0.0.1:8765")
    {
        using var webSocket = new ClientWebSocket();
        var serverUri = new Uri($"ws://{address}");
        await webSocket.ConnectAsync(serverUri, cancellationToken);

        var token = Environment.GetEnvironmentVariable("TOKEN");
        var registerPayload = token == null ?
                                  JsonConvert.SerializeObject(new { type = "REGISTER", teamName = Bot.Name }) :
                                  JsonConvert.SerializeObject(new { type = "REGISTER", token });

        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(registerPayload)),
                                  WebSocketMessageType.Text,
                                  true,
                                  cancellationToken);

        while (webSocket.State == WebSocketState.Open) {
            var message = await ReadMessage(webSocket, cancellationToken);
            var gameMessage = JsonConvert.DeserializeObject<GameMessage>(message, _jsonSerializerSettings);
            if (gameMessage == null) {
                continue;
            }

            Console.WriteLine($"Round {gameMessage.Round} (Tick {gameMessage.Tick}). Ticks until payout: {gameMessage.TicksUntilPayout}");
            var serializedCommand = JsonConvert.SerializeObject(new {
                                                                    type = "COMMAND",
                                                                    actions = await _bot.GetActionsAsync(gameMessage,
                                                                                                         cancellationToken),
                                                                    tick = gameMessage.Tick
                                                                },
                                                                _jsonSerializerSettings);

            await webSocket.SendAsync(Encoding.UTF8.GetBytes(serializedCommand),
                                      WebSocketMessageType.Text,
                                      true,
                                      cancellationToken);
        }
    }

    private static async Task<string> ReadMessage(ClientWebSocket client, CancellationToken cancellationToken)
    {
        ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[8192]);

        WebSocketReceiveResult result;

        using var memoryStream = new MemoryStream();
        do {
            result = await client.ReceiveAsync(buffer, cancellationToken);
            memoryStream.Write(buffer.Array!, buffer.Offset, result.Count);
        } while (!result.EndOfMessage);

        memoryStream.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(memoryStream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
