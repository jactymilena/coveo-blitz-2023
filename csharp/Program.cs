var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, _) => cancellationTokenSource.Cancel();

await GameClient.Run(cancellationTokenSource.Token);
