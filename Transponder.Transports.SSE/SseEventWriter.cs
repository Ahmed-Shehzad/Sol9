using System.Text;

namespace Transponder.Transports.SSE;

internal static class SseEventWriter
{
    public async static Task WriteAsync(Stream stream, SseEvent message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(message);

        var builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(message.Comment))
        {
            builder.Append(':').Append(' ').Append(message.Comment).Append('\n');
            builder.Append('\n');
            await WriteAsync(stream, builder, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!string.IsNullOrWhiteSpace(message.Id))
            builder.Append("id: ").Append(message.Id).Append('\n');

        if (!string.IsNullOrWhiteSpace(message.EventName))
            builder.Append("event: ").Append(message.EventName).Append('\n');

        if (!string.IsNullOrWhiteSpace(message.Data))
        {
            string[] lines = message.Data.Split('\n');
            foreach (string line in lines)
                builder.Append("data: ").Append(line).Append('\n');
        }
        else builder.Append("data: ").Append('\n');

        builder.Append('\n');
        await WriteAsync(stream, builder, cancellationToken).ConfigureAwait(false);
    }

    private async static Task WriteAsync(Stream stream, StringBuilder builder, CancellationToken cancellationToken)
    {
        byte[] payload = Encoding.UTF8.GetBytes(builder.ToString());
        await stream.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
