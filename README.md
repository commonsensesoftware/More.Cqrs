# More CQRS
A generic, portable set of libraries for implementing CQRS and Event Sourcing using the .NET Framework.

## Background
[CQS](https://en.wikipedia.org/wiki/Command–query_separation), and its more well-known evolution CQRS, has been around for years. There are numerous frameworks and projects out there, so why another one?

After countless hours of reading and researching different implementation stacks, I didn't find a single one that wholistically satisified all of the things I was looking for in both a CQRS and Event Sourcing stack. This project is currently an incubation project as both an academic exercise for learning as well as a chance to collate some of the best ideas from different implemenation stacks into a single place using the most current features in .NET.

The end goal is to have a common set of abstractions that can be easily swapped out for any number of different implementations - be it well-known or custom. Perhaps some day the project will manifest as a prolific free, open source framework for all to benefit from.

An ancillary goal of the project is to provide sample projects to not only flush out the design, but to also provide plausible end-to-end demonstrations of how all the pieces fit together. While there are some, practical end-to-end solutions are lacking in the wild.

## Disclaimer
There is no one _right_ answer to implementing CQRS or Event Sourcing. There are a few universal truths that evangelists will agree should or should not be done, but _how_ things are actually implemented varies greatly. The ideas presented here should not necessarily be considered any more correct or valid than other implementations. CQRS has been argued to be a _style_ more than a _design pattern_. To that end, there is no _correct_ way to implement a style.

Feedback and criticism are both welcome as long as everyone understands that we all have strong opinions with regard to our preferences and experiences.

The examples provided here are meant for education and demonstration purposes only. There is no express relationship, explicit or implied, to any existing system. Any similarity to an actual system is purely coincidental.

## Influences
There were many, many influences that drove this project to where it is now. In no particular order, the most relevant sources are:

* Greg Young's [Simple CQRS](https://github.com/gregoryyoung/m-r)
* [A CQRS Journey](https://msdn.microsoft.com/en-us/library/jj554200.aspx)
* [It's CQRS](https://github.com/jonsequitur/Its.Cqrs)
* [NEventStore](https://github.com/NEventStore)
* [NServiceBus](https://github.com/Particular/NServiceBus)

There are also countless other reading references in print, on the web, etc that are too numerous to list here.

## Project Disposition
The current state of the project has basic support for using a Microsoft SQL Server database as well as in-memory support for message and events. These were selected purely because they were the easiest to understand and implement. Future versions of the project will provide support for Azure Service Bus, CosmosDb (formerly DocumentDb), and other providers. The necessary abstractions to implement your own providers are possible now.

Advocates interested in driving this project forward are welcome. Wiki support and other documentation will be forthcoming at some point in the future with additional help or when there's enough time.

### Current Capabilities

* Events
* Commands
* Event Sourcing
* Message Queueing with Scheduling Options
* Snapshots
* Single-Phase Commits (by design; implemention may use multi-phase commits)
* Sagas (aka _Process Managers_ or _Processors_)
* Correlation
* Time Abstraction (e.g. _virtual_ clock)


### Future Capabilities

* Projections (e.g. Projector Agents)
* Snapshotters (e.g. Snapshot Agents)
* Microsoft CosmosDb Support
* Microsoft Service Bus Support