namespace ExampleApplication.FiksIOSender;

public class MessageSender
{
    private readonly IFiksIOSender _fiksIoSender;
    private readonly AppSettings _appSettings;

    private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);


    public MessageSender(IFiksIOSender fiksIoSender, AppSettings appSettings)
    {
        _fiksIoSender = fiksIoSender;
        _appSettings = appSettings;
    }

    public async Task<Guid> Send(string messageType, Guid toAccountId)
    {
        try
        {
            var klientMeldingId = Guid.NewGuid();
            Log.Information(
                "MessageSender - sending messagetype {MessageType} to account id: {AccountId} with klientMeldingId {KlientMeldingId}",
                messageType, toAccountId, klientMeldingId);

            using var fileStream = new FileStream("testfile.txt", FileMode.Open);
            var payload = new List<IPayload> { new StreamPayload(fileStream, "testfile.txt") };

            var sendtMessage = await _fiksIoSender
                .SendWithEncryptedData(
                    new MeldingSpesifikasjonApiModel(
                        _appSettings.FiksIoAccountId,
                        toAccountId,
                        messageType,
                        ttl: (long)TimeSpan.FromDays(2).TotalMilliseconds,
                        headere: new()),
                    payload)
                .ConfigureAwait(false);
            Log.Information("MessageSender - message sendt with messageid: {MessageId}", sendtMessage.MeldingId);
            return sendtMessage.MeldingId;
        }
        catch (Exception e)
        {
            Log.Error("MessageSender - could not send message to account id {AccountId}. Error: {ErrorMessage}",
                toAccountId, e.Message);
            throw;
        }
    }
}