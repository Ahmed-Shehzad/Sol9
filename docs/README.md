# Sol9 Documentation

Documentation for the Sol9 solution: deployment, architecture, and core libraries.

---

## Deployment

- **[Deployment Guide](Deployment/README.md)** – Docker, Kubernetes, blue-green with Argo Rollouts, gRPC/Transponder configuration, CI/CD, and troubleshooting.

---

## Libraries and Services

Documentation for each core library: setup, configuration, when/where/why to use (and when not to).

| Library | Description | Doc |
|---------|-------------|-----|
| **Transponder** | Enterprise messaging framework: request/response, publish/subscribe, sagas, outbox, multiple transports (gRPC, Kafka, RabbitMQ, etc.). | [Transponder](Transponder/README.md) |
| **Intercessor** | In-process mediator: commands, queries, notifications, pipeline behaviors (validation, logging, retry, caching). | [Intercessor](Intercessor/README.md) |
| **Verifier** | Validation framework: fluent rule builders, sync/async validators, integration with Intercessor. | [Verifier](Verifier/README.md) |

### Quick reference

- **Transponder**: Use for **cross-service** or **cross-process** communication (e.g. Orders → Bookings). Setup: `AddTransponder`, transport (e.g. gRPC), optional persistence and outbox/saga. Config: addresses, connection strings. See [Transponder README](Transponder/README.md) and [Transports](Transponder/Transports/README.md).
- **Intercessor**: Use for **in-process** commands, queries, and notifications (controller → handler). Setup: `AddIntercessor`, `RegisterFromAssembly`, optional `AddBehavior`. Config: assemblies and behaviors. See [Intercessor README](Intercessor/README.md).
- **Verifier**: Use for **validating** commands, queries, and events before handlers run. Setup: usually via Intercessor; or `AddVerifier` + `RegisterFromAssembly`. Config: assemblies. See [Verifier README](Verifier/README.md).

---

## See Also

- [Sol9 README](../README.md) – Solution overview, architecture, quick start.
