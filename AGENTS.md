# Instructions for Codex

Work within the existing architecture and make the smallest correct change. Avoid speculative abstractions. Follow existing feature-first naming conventions.

## Dependency boundaries

- Domain -> nothing in the solution.
- Application -> Domain.
- Infrastructure -> Application + Domain.
- Api -> Application + Infrastructure.
- DeviceWorker -> Application + Infrastructure.
- Keep Application and Domain independent from Marten. Put all Marten-specific code in Infrastructure.
- Keep future broker-specific code out of Application and Domain. Do not introduce RabbitMQ or Kafka until explicitly requested.

## Application and domain behavior

- Use the existing custom mediator; do not add MediatR.
- Keep only the existing `ValidationBehavior` unless explicitly asked to add another behavior.
- Let domain aggregate roots own domain and business rules. Let handlers orchestrate use cases.
- Prefer specific custom exceptions over generic exceptions. Never classify an error by parsing its exception message.
- Commands mutate state. Queries read state and must not mutate it.
- Keep API contracts separate from Application request and result models. Keep controllers thin and dispatch through `ISender`.

## Events and persistence

- Name events as past-tense facts and treat recorded events as append-only.
- `Apply` methods only apply events that have already been accepted; put decisions and validation before applying or recording an event.
- Preserve the two distinct projections: an aggregate projection replays events into a domain aggregate, while a document projection turns events into a read model or document.

## Working practice

- After changes, always run `dotnet build` and report its result.
- Do not commit, push, create branches, or rewrite Git history unless explicitly requested.
- At the end, report changed files and verification performed.
