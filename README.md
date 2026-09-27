# HomeControl

HomeControl is a small backend learning project. It is intentionally used to practice .NET 10 and ASP.NET Core while exploring how a device-focused application can keep its domain rules, use cases, persistence, and entry points separate. It is a learning codebase, not a production-ready service.

## Current implementation

The solution is organized around clean dependency boundaries, CQRS, an existing custom mediator, and domain modeling with aggregate roots. Its Event Sourcing work uses Marten with PostgreSQL for event streams and document storage, including device projections and read models. The current code also handles optimistic concurrency and uses the Repository pattern for device reads.

| Project | Responsibility |
| --- | --- |
| `HomeControl.Api` | ASP.NET Core HTTP entry point and API contracts |
| `HomeControl.Application` | Use cases, commands, queries, handlers, and mediator pipeline |
| `HomeControl.Domain` | Domain model, aggregate roots, and business rules |
| `HomeControl.Infrastructure` | Marten persistence, projections, and RabbitMQ publishing |
| `HomeControl.DeviceWorker` | RabbitMQ device audit consumer |

Dependencies point inward: Domain depends on nothing in the solution; Application depends on Domain; Infrastructure depends on Application and Domain; and both Api and DeviceWorker depend on Application and Infrastructure. Marten stays in Infrastructure so Application and Domain remain persistence independent.

In the current Event Sourcing flow, a command reaches an application handler through the custom mediator. The handler loads a domain aggregate from its event stream, asks it to perform the requested operation, and persists the resulting events through Infrastructure. Events are past-tense facts. Marten stores events in `mt_events` and stream metadata in `mt_streams`; a document projection builds the device read model stored in `mt_doc_device`. Queries read the projected document instead of changing the event stream.

## Learning roadmap

The current code is a foundation for further practice with message bus concepts, Kafka, and event-driven microservice communication.

## Build and local configuration

```sh
dotnet build
```

Keep local secrets and database credentials out of Git. Use ASP.NET Core User Secrets for local development settings where appropriate.

## RabbitMQ light audit example

After a successful Marten save, the light on and off command handlers publish a separate
`DeviceLightStateChanged` integration message. RabbitMQ routes it through the durable topic
exchange `homecontrol.events` using `device.light.turned-on` or `device.light.turned-off`.
The worker declares the durable `homecontrol.device-audit` queue, binds `device.#`, logs
`DeviceId`, `IsOn`, timestamp, and routing key, and ACKs only after processing succeeds.
Messages are persistent. The domain events and Marten event streams are unchanged.

Start the local broker with `docker compose up -d`. RabbitMQ AMQP is available on port
`5672`; its management UI is at `http://localhost:15672`. The example appsettings files
contain the host and port. Set `RabbitMq:UserName` and `RabbitMq:Password` in both project
configurations using user secrets or environment variables, and configure the API's
existing `ConnectionStrings:Marten` PostgreSQL connection string. For example, environment
variables use `RabbitMq__UserName` and `RabbitMq__Password`.

Run the worker first so its queue and binding exist before messages are published:

```sh
dotnet run --project src/HomeControl.DeviceWorker
dotnet run --project src/HomeControl.Api --launch-profile http
```

Run those commands in separate terminals. The API listens on `http://localhost:5164`.
Register a light with `POST /api/devices`, then call `POST /api/devices/{id}/turn-on` and
`POST /api/devices/{id}/turn-off` to see the audit log and queue activity. There is no
outbox: a RabbitMQ publish failure after the Marten save leaves the device state changed
while the API request fails. This dual-write gap is intentional for the learning example.
