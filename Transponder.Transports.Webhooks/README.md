# Transponder.Transports.Webhooks

Webhook transport for server-to-server integrations. Designed for durable delivery when paired with Transponder outbox + retries.

## Semantics
- Publish-only: request/response and send-to-endpoint patterns are not supported.
- Intended for integrations and external systems.
- Durable when combined with `UseOutbox()` and a persistence provider.

## Registration
```csharp
var subscriptions = new List<WebhookSubscription>
{
    new(new Uri("https://example.com/webhooks/transponder"), name: "partner", secret: "signing-key"),
    new(new Uri("https://example.com/webhooks/orders"), name: "orders", eventTypes: ["Sol9.Contracts.Orders.*"])
};

services.AddTransponder(localAddress, options =>
{
    options.TransportBuilder.UseWebhooks(new Uri("webhook://local"), subscriptions);
    options.UseOutbox();
});
```

## Delivery metadata
Each delivery includes headers:
- `X-Transponder-Id`
- `X-Transponder-Type`
- `X-Transponder-SentAt`
- `X-Transponder-Content-Type`
- `X-Transponder-Correlation`
- `X-Transponder-Conversation`
- `X-Transponder-Subscription`

Payload is the serialized message body (no additional envelope).

## Signatures
If a subscription has a `secret`, the transport emits:
- `X-Transponder-Timestamp`
- `X-Transponder-Signature`

Signature is `sha256=` + HMACSHA256 over:
```
{timestamp}.{raw-body-bytes}
```

## Incoming verification (middleware)
```csharp
app.UseTransponderWebhookSignatureValidation(options =>
{
    options.DefaultSecret = "shared-secret";
    options.SubscriptionSecrets["partner"] = "partner-secret";
});
```

Example with retries:
```csharp
var resilience = new TransportResilienceOptions
{
    EnableRetry = true,
    Retry = new TransportRetryOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(2)
    }
};

services.AddTransponder(localAddress, options =>
{
    var settings = new WebhookHostSettings(
        new Uri("webhook://local"),
        subscriptions,
        resilienceOptions: resilience);

    options.TransportBuilder.UseWebhooks(settings);
    options.UseOutbox();
});
```

## Retries and delivery semantics
- Retries are controlled by `TransportResilienceOptions` on `WebhookHostSettings`.
- Retries apply to the publish call and can result in duplicate deliveries.
- Use `X-Transponder-Id` for idempotency on receivers.
- For durability, pair with `UseOutbox()` and persistence.

## Filtering
Use `eventTypes` with optional wildcard suffix:
- `"Sol9.Contracts.Orders.*"` matches any type starting with that prefix.
- `"*"` matches all.
