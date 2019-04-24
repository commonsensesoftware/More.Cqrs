# Contoso Tokens
Contoso Tokens is a fictional system which creates and manages tokens. Tokens are an abstraction for digital goods and services. A token can be used to redeem a good or service such as an online game or subscription. Each token has its own life cycle and state machine.

## Why Tokens
A token system is a variant of the traditional ordering system examples. This example was selected as it has many of the classic ordering system scenarios as well as unique, real world challenges. For example, event sourcing is perfect for the state machine for tokens, but how do you implement such as system at scale when the quantity of tokens could be millions, or even billions, at a time? This example does not attempt to solve these challenges directly, but highlights the fact that they exist and puts forth remedial approaches on how they _might_ be solved. Ultimately, a token system has simple enough business requirements to understand, but is complex enough to warrant CQRS and event sourcing.

## Design
The design consists of a several pieces:

* A _domain_ model
* A _read_ model (e.g. projections)
* A RESTful API using OData
* An API runtime, which uses dependency injection (DI) to compose all the pieces together

## Workflows

TODO: add block diagrams of the business flows

## Tests
Domain-driven tests are provided using the _in-memory_ model. This allows rapid feedback on the validity of the domain and business process. Test sequences consistently happen in temporal order.

End-to-end API scenario tests also exist. The API tests build up scenarios, generally modeled on acceptance criteria, and exercise the full stack. These are slower and more difficult to troubleshoot. Scenarios should be considered after the domain model is sound. Be aware that the end-to-end scenario tests do **not** clear any state from the backing database. You may want to occasionally re-create (e.g. publish) the database. Since tests run in parallel using a shared database, you may notice cross-scenario activity, which will be more indicative of how the system would run in a production environment.

All tests are written using the _Given-When-Then_ (GWT) style. This makes understanding what each test does clear and concise. It also makes it clear as to which scenarios work and which are broken. All tests are configured to capture and trace basic message information that travels through the bus. This is useful in a number of diagnostic scenarios.