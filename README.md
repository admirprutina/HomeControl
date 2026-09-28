# HomeControl

HomeControl is a small backend learning project. It is intentionally used to practice .NET 10 and ASP.NET Core while exploring how a device-focused application can keep its domain rules, use cases, persistence, and entry points separate. It is a learning codebase, not a production-ready service.

## Current implementation

The solution is organized around clean dependency boundaries, CQRS, an existing custom mediator, and domain modeling with aggregate roots. Its Event Sourcing work uses Marten with PostgreSQL for event streams and document storage, including device projections and read models. The current code also handles optimistic concurrency and uses the Repository pattern for device reads.

| Project | Responsibility |
| --- | --- |
| `HomeControl.Api` | ASP.NET Core HTTP entry point and API contracts |
| `HomeControl.Application` | Use cases, commands, queries, handlers, and mediator pipeline |
| `HomeControl.Domain` | Domain model, aggregate roots, and business rules |
| `HomeControl.Infrastructure` | Marten persistence, projections, RabbitMQ publishing, and Kafka telemetry publishing |
| `HomeControl.DeviceWorker` | RabbitMQ device audit consumer |
| `HomeControl.TelemetryWorker` | Independent Kafka telemetry consumer with manual offset commits |

Dependencies point inward: Domain depends on nothing in the solution; Application depends on Domain; Infrastructure depends on Application and Domain; and both Api and DeviceWorker depend on Application and Infrastructure. Marten stays in Infrastructure so Application and Domain remain persistence independent.

In the current Event Sourcing flow, a command reaches an application handler through the custom mediator. The handler loads a domain aggregate from its event stream, asks it to perform the requested operation, and persists the resulting events through Infrastructure. Events are past-tense facts. Marten stores events in `mt_events` and stream metadata in `mt_streams`; a document projection builds the device read model stored in `mt_doc_device`. Queries read the projected document instead of changing the event stream.

## Learning roadmap

The current code includes separate RabbitMQ audit and Kafka telemetry examples for practicing message bus concepts and event-driven microservice communication.

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

## Kafka device telemetry example

Telemetry uses its own integration stream and never changes the Device aggregate or
Marten event stream. The API checks the existing device document through
`IDeviceRepository`, creates `DeviceTelemetryRecorded`, and publishes through the
Application's `IDeviceTelemetryPublisher`. Infrastructure uses Confluent.Kafka directly.
Application and Domain contain no Kafka client types. TelemetryWorker references
Application and Infrastructure, like DeviceWorker, and starts independently.

`KafkaTopology` contains the topic `homecontrol.device-telemetry`, three partitions,
consumer group `homecontrol.analytics`, and the local retention/replication constants.
`KafkaTopicInitializer` uses AdminClient to create the topic on the first publication
and at worker startup. It accepts an existing topic only after checking that it has
three partitions. Existing topic retention settings are left untouched; newly created
topics use delete cleanup and seven-day retention. Changing the partition count can
change where a key maps, so the initializer never changes it automatically.

Start the two local brokers and wait for Kafka readiness:

```sh
docker compose up -d --wait
```

Compose runs RabbitMQ and one Kafka broker/controller in KRaft mode, without ZooKeeper.
RabbitMQ keeps its existing ports, and Kafka is available to native applications at
`localhost:9092`. Kafka data and group offsets survive container recreation in the
`kafka-data` named volume. The API, PostgreSQL, DeviceWorker, and TelemetryWorker run
natively; Compose does not run them.

Keep the API's existing Marten and RabbitMQ configuration. Both API and TelemetryWorker
include `Kafka:BootstrapServers` in appsettings. Override it with
`Kafka__BootstrapServers` or user secrets. For secured brokers, `KafkaSettings` also
reads `SecurityProtocol`, `SaslMechanism`, `SaslUsername`, `SaslPassword`, and
`SslCaLocation` from the same section; keep credentials in secrets/environment variables.
The local broker uses plaintext and requires no credentials. To load the new worker's
user secrets, run it with `--environment Development`.

Run each application in a separate terminal:

```sh
dotnet run --project src/HomeControl.Api --launch-profile http
dotnet run --project src/HomeControl.DeviceWorker
dotnet run --project src/HomeControl.TelemetryWorker
```

Either the API or TelemetryWorker can create the topic first. Register a device through
`POST /api/devices`, then send (replace `{id}` with its returned ID):

```http
POST http://localhost:5164/api/devices/{id}/telemetry
Content-Type: application/json

{
  "temperatureCelsius": 22.6,
  "powerUsageWatts": 8.3
}
```

At least one measurement must be supplied; there are no additional measurement ranges.
Optional `occurredAtUtc` is normalized to UTC; omitted timestamps use the handler's
current UTC time. An unknown device returns 404, missing measurements return 400, and
successful broker acknowledgement returns **202 Accepted** with device ID and timestamp.
Consumer processing may still be pending. A publish failure fails the request.

The producer lives in `KafkaDeviceTelemetryPublisher`. Its message key is
`DeviceId.ToString("D")`, and its value is JSON for the separate integration contract.
The broker assigns an offset within the selected partition. With the partition count
fixed, the same device key consistently maps to the same partition and preserves the
order of records appended there. There is no ordering guarantee between partitions.
The producer logs the acknowledged topic/key/partition/offset. It uses `Acks.All`,
disables producer send retries, and limits in-flight requests to one for ordering.

TelemetryWorker subscribes as `homecontrol.analytics` and processes one record at a
time. It logs device ID, measurements, timestamp, topic, partition, and offset. Both
`EnableAutoCommit` and `EnableAutoOffsetStore` are false. After processing succeeds,
`Commit(delivery)` commits **delivery offset + 1**, the next offset for that group and
partition. On processing or commit failure the worker logs and stops, rather than
allowing a later commit to skip the failed record. Close leaves the group without an
automatic commit. Processing can repeat if the process stops after logging but before
the commit is acknowledged. A commit failure can also leave its outcome uncertain.

Start a second TelemetryWorker instance to see group members share partitions. This
group can have at most three actively consuming instances for this three-partition
topic. A single device's records remain assigned to one partition and one group member
at a time. Send telemetry for multiple registered device IDs to observe partitioning;
different IDs can also hash to the same partition.

Inspect the topic and committed group offsets:

```sh
docker compose exec kafka /opt/kafka/bin/kafka-topics.sh --bootstrap-server kafka:19092 --describe --topic homecontrol.device-telemetry
docker compose exec kafka /opt/kafka/bin/kafka-consumer-groups.sh --bootstrap-server kafka:19092 --describe --group homecontrol.analytics
```

Offsets are per partition; the group's current offset is where it resumes, and lag is
the distance to the partition's end. Commits do not remove Kafka records. Retention
removes old log segments independently of whether a consumer processed them; seven
days is a retention policy, not an exact per-record deletion deadline.

For replay, **stop every TelemetryWorker in this group**, then preview and execute an
offset reset and restart the worker:

```sh
docker compose exec kafka /opt/kafka/bin/kafka-consumer-groups.sh --bootstrap-server kafka:19092 --group homecontrol.analytics --topic homecontrol.device-telemetry --reset-offsets --to-earliest --dry-run
docker compose exec kafka /opt/kafka/bin/kafka-consumer-groups.sh --bootstrap-server kafka:19092 --group homecontrol.analytics --topic homecontrol.device-telemetry --reset-offsets --to-earliest --execute
dotnet run --project src/HomeControl.TelemetryWorker
```

Only retained records can be replayed. `AutoOffsetReset.Earliest` starts at the earliest
retained offset when no valid committed offset exists; it does not cause every restart
to replay previously committed records. A separate consumer group can independently
read retained records without changing `homecontrol.analytics` offsets.

This example has one broker with replication factor one and logging-only processing.
It has no outbox, application retries, retry topics, DLQ, schema registry, Avro, or
Protobuf. HTTP callers retrying an uncertain publication can create duplicate records.
Marten event sourcing, projections, and the RabbitMQ light audit flow remain unchanged.

Verify the solution with:

```sh
dotnet build HomeControl.slnx
dotnet test HomeControl.slnx --no-build
```

Client reference: [Confluent .NET Kafka producer/consumer documentation](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html).
Local container reference: [Apache Kafka Docker documentation](https://kafka.apache.org/41/getting-started/docker/).
