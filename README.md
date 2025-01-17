# Payment Gateway Challenge @ Checkout.com

# How to run

To run the project, simply run the `docker-compose.yml` on the command line:

```shell
  docker-compose up -d
```
It will generate the Payment Gateway API image and run the docker container, alongside the dependencies.

The service will run locally on `http://localhost:8081`. To test the APIs, You can choose your preferred API Client (Postman, cURL, etc), 
or try out the file `requests.http`. Just remember to replace the payment-id placeholder with the one you created.

Currently, it supports two endpoints:

- `api/Payments`:

Example of curl request:
```curl
curl -X POST http://localhost:8081/api/Payments \
-H "Content-Type: application/json" \
-d '{
  "card_number": "2222405343248111",
  "expiry_month": "01",
  "expiry_year": "2026",
  "currency": "USD",
  "amount": 60000,
  "cvv": "456"
}'
```

- `api/Payments/{id}`:
```curl
curl -X GET http://localhost:8081/api/Payments/<YOUR-PAYMENT-ID> \
-H "Content-Type: application/json" 
```

# Functional Requirements

The aim of this project/service is to cover the functional requirements described in [here](https://github.com/cko-recruitment/).
To cover them up here too:

- A merchant should be able to process a payment through the payment gateway and receive one of the following types of response:
  - **Authorized** - the payment was authorized by the call to the acquiring bank
  - **Declined** - the payment was declined by the call to the acquiring bank
  - **Rejected** - When the bank simulator returns a specific client error code, we can assume the payment has failed due to the user request (e.g. 400 Bad Request).
    > This is a bit different from the original requirements, but I considered it might be what we will like to do on a real scenario. 
- A merchant should be able to retrieve the details of a previously made payment.

# Non-Functional Requirements

Below is a description of the low-level, technical details of the current project, 
with some justification on the why.

- Fault-Tolerant: we want to be able to set retries when the client request fails if it's due to a 5xx or Timeout error.
- Strongly consistent: we need to provide consistency guarantees in our payment systems, critical to avoid inconsistencies with balances and payments.
- Low latency
- Scalable to millions of users

# Technical Details 

## Architecture

```
src
├── Api
│   └── PaymentGateway.Api
├── Core
│   ├── PaymentGateway.Application
│   ├── PaymentGateway.Domain
│   └── PaymentGateway.Shared
├── Infrastructure
│   ├── PaymentGateway.ApiClient
│   └── PaymentGateway.Persistence
test
```
When developing this services I tried to mimic a _Clean Architecture_ structure. The reason is because I wanted to organize code into layers and enforce a clear separation of concerns to make the code more scalabile, maintainabile, and testable. 

Honestly, _Clean architecture_ might be a bit of an overkill for this project, but I believe is good practise to do so in general, specially if the project is gonna grow with more engineers working on it, so I decided to refactor my original code to accomodate it. 
Here's a breakdown of the layers:

1. **Api Layer (PaymentGateway.Api)**
   - This layer acts as the entry point for the payment gateway. 
   - responsible for interacting with external clients, handling HTTP requests, and returning responses.
2. **Core Layer** : This is the business logic layer. Divided into three subproject: 
   - **PaymentGateway.Application**: Contains the application service layer.
Responsible for orchestrating domain logic and managing workflows.
Interfaces (e.g., IPaymentsRepository) define contracts for infrastructure dependencies.
   - **PaymentGateway.Domain**: The core business logic and entities reside here. This layer doesn't depend on any external frameworks or libraries, but rather all dependent from this layer. 
   - **PaymentGateway.Shared**: For cross-cutting concerns like utilities, observability, or shared logic between different layers. I created this layer avoid duplication and centralize reusable code.
3. **Infrastructure Layer**
   - **PaymentGateway.ApiClient**: code for interacting with external APIs or third-party services (in our case, the Mount Bank container).
   - **PaymentGateway.Persistence**: Responsible for data access and persistence: repositories, database context, and migrations.

## Observability
Distributed tracing is critical for applications, especially those based on microservices architectures, where requests often traverse multiple services. 
Thanks to OpenTelemetry, it can provide insight into the flow of requests, and help diagnose performance bottlenecks, errors, and improve overall system reliability.

For this project I decided to use Jaeger as the OTEL exporter. The reason is because is an open-source tool and integrates well into ASP .NET, and since this is 
an MVP project this should cover everything. Currently we have a very simple Observability ecosystem, but as an MVP it works as a proof of concept. 

When running the `docker-compose.yml`, you should be able see the traces of each request under http://localhost:16686. 

## Data Storage

In order to store the payments response, I used Postgres as the main persistence. The reason is because of its strong ACID properties, 
which is essential in a strongly consistent distributed system like a payment system.

To query the database, please use the following JDBC connection string: `jdbc:postgresql://localhost:5432/payments-db?user=admin&password=password`.

## Caching

For the get payment request, I set up a simple in-memory cache with an expiration. This improves our GET request with minimal effort.
However, in a real scenario where we have many replicas of our service, this should treated carefully: in-memory is ephemeral and therefore unreliable,
and each replica will store different keys, causing inconsistencies. 

## Testing

Inside the `/test` folder, I setup two testing project: `UnitTest` and `IntegrationTest`. 
I tried to follow the [testing pyramid](https://martinfowler.com/articles/practical-test-pyramid.html) approach: unit tests for Core layers and integration tests for API and Infrastructure.

Some description provided below:
- **Unit tests:** 
  - I tried to cover  test the smallest pieces of code in isolation directly. For example, I tested the methods on the extension methods, the mappers, and the validators directly.
  - On this project, I also test the service layer. The reason why is in here and not integration test project, is because I focused on testing the core service logic. The dependencies are directly mocked using Moq and WireMock (for the Simulator API client).
- **Integration tests:** 
  - Aims to ensure that different parts of the application work together as expected.
  - To do so, I tested directly from the Controllers using the `TestContainers` library, a lightweight solution to run some dependencies as docker containers. 
  - For the project I had to setup containers for Postgres and the `bbyars/mountebank` image. 

# Future Improvements

Below are some potential improvements I purposelly left out of the implementation, due to the nature of the 
project been an MVP and limited amount of time, but it would be interesting to explore in the future!

- [ ] Authentication/Authorization of our APIs
- [ ] CI pipeline using Github Actions
- [ ] CD and deployment in AWS or other cloud provider
- [ ] BDD or Behaviour-Driven Testing
- [ ] Idempotency Key header for retryable payments 
- [ ] Better separation of layers (i.e. remove the Shared library)

