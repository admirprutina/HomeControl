# HomeControl

HomeControl is a small backend learning project. It is intentionally used to practice .NET 10 and ASP.NET Core while exploring how a device-focused application can keep its domain rules, use cases, persistence, and entry points separate. It is a learning codebase, not a production-ready service.

## Current implementation

The solution is organized around clean dependency boundaries, CQRS, an existing custom mediator, and domain modeling with aggregate roots. Its Event Sourcing work uses Marten with PostgreSQL for event streams and document storage, including device projections and read models. The current code also handles optimistic concurrency and uses the Repository pattern for device reads.

| Project | Responsibility |
| --- | --- |
| `HomeControl.Api` | ASP.NET Core HTTP entry point and API contracts |
| `HomeControl.Application` | Use cases, commands, queries, handlers, and mediator pipeline |
| `HomeControl.Domain` | Domain model, aggregate roots, and business rules |
| `HomeControl.Infrastructure` | Marten persistence and projections |
| `HomeControl.DeviceWorker` | Scaffolded background worker host |

Dependencies point inward: Domain depends on nothing in the solution; Application depends on Domain; Infrastructure depends on Application and Domain; and both Api and DeviceWorker depend on Application and Infrastructure. Marten stays in Infrastructure so Application and Domain remain persistence independent.

In the current Event Sourcing flow, a command reaches an application handler through the custom mediator. The handler loads a domain aggregate from its event stream, asks it to perform the requested operation, and persists the resulting events through Infrastructure. Events are past-tense facts. Marten stores events in `mt_events` and stream metadata in `mt_streams`; a document projection builds the device read model stored in `mt_doc_device`. Queries read the projected document instead of changing the event stream.

## Learning roadmap

The current code is a foundation for further practice with the Strategy pattern, RabbitMQ, message bus concepts, Kafka, and event-driven microservice communication. These are future learning topics; their mention here does not mean broker integration or distributed messaging is implemented.

## Build and local configuration

```sh
dotnet build
```

Keep local secrets and database credentials out of Git. Use ASP.NET Core User Secrets for local development settings where appropriate.
