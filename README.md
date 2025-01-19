# Payment Gateway @ Checkout.com

# How to run

To run the project, use the the `docker-compose.yml` in the command line:

```shell
  docker-compose up -d
```
It will generate the Payment Gateway API image and run the docker container, alongside the dependencies.

The service will run locally on `http://localhost:8081`. To test the APIs, choose your preferred API Platform (Postman, cURL, etc), 
or try run the file `requests.http`. Just remember to replace the `<payment-id>` placeholder with the one you created.

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

The aim of this project/service is to cover the functional requirements described in [here](https://github.com/cko-recruitment/). To recap:

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
- Strong consistency: we need to provide consistency guarantees in our payment systems, critical to avoid inconsistencies with balances and payments.
- Low latency (<1 second) to offer good user experience
- Scalable to (potentially) millions of users. A company like Checkout.com currently runs millions of transactions per day. 

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
When developing this services I tried to mimic a _Clean Architecture_ structure. The reason is because I wanted to organize code into layers and enforce a clear separation of concerns to make the code more scalable, maintainable, and testable. 

_Clean architecture_ might be a bit of an overkill for this project, but I believe is good practise to do so in general, specially if the project is gonna grow with more engineers working on it, so I decided to refactor my original code to accomodate it. 
Here's a breakdown of the layers:

1. **Api Layer (PaymentGateway.Api)**
   - This layer acts as the entry point for the payment gateway. 
   - responsible for interacting with external clients, handling HTTP requests, and returning responses.
2. **Core Layer** : This is the business logic layer. Divided into three subproject: 
   - **PaymentGateway.Application**: Contains the application service layer. Responsible for orchestrating domain logic and managing workflows. Here we also store Interfaces inside `/Contracts` (e.g., IPaymentsRepository).
   - **PaymentGateway.Domain**: The core business logic and entities reside here. This layer doesn't depend on any external frameworks or libraries, but rather all dependent from this layer. 
   - **PaymentGateway.Shared**: For cross-cutting concerns like utilities, observability, or shared logic between different layers. I created this layer avoid duplication and centralize reusable code.
3. **Infrastructure Layer**: The outer layer 
   - **PaymentGateway.ApiClient**: code for interacting with external APIs or third-party services (in our case, the Mount Bank container).
   - **PaymentGateway.Persistence**: Responsible for data access and persistence: repositories, database context, and migrations.

## Observability
Distributed tracing is critical for applications, especially those based on microservices architectures, where requests often traverse multiple services. 
It can provide insight into the flow of requests, and help diagnose performance bottlenecks, errors, and improve overall system reliability.

For this project I decided to use Jaeger as our OTEL exporter. The reason is because is an open-source tool and integrates well into ASP .NET, and since this is 
an MVP project this should cover everything. Currently we have a very simple Observability ecosystem, but as an MVP it works as a proof of concept. 

When running the `docker-compose.yml`, you should be able to see the trace and spans for each request, under http://localhost:16686. 

## Data Storage

In order to store the payments response, I used Postgres as the main persistence. The reason is because of its strong ACID properties, 
which is essential in a strongly consistent distributed system like a payment system.

To query the database, please use the following JDBC connection string: `jdbc:postgresql://localhost:5432/payments-db?user=admin&password=password`

## Caching

For the get payment request, I set up a in-memory cache with an expiration. This improves latency of GET payment request with minimal effort.
However, in a real scenario this should treated carefully: in-memory is ephemeral and therefore unreliable, and each replica will store different keys, creating potential stale data. 

## Testing

Inside the `/test` folder, I set up two testing project: `UnitTest` and `IntegrationTest`. 
I tried to follow the [testing pyramid](https://martinfowler.com/articles/practical-test-pyramid.html) approach: unit tests for Core layers and integration tests for API and Infrastructure.

Some description provided below:
- **Unit tests:** 
  - I tried to cover  test the smallest pieces of code in isolation directly. For example, I tested the methods on the extension methods, the mappers, and the validators directly.
  - On this project, I also test the service layer. The reason why is in here and not integration test project, is because I focused on testing the core service logic. The dependencies are directly mocked using Moq and WireMock (for the Simulator API client).
- **Integration tests:** 
  - Aims to ensure that different parts of the application work together as expected.
  - The approach is to test the Controller directly using the `TestContainers` library, a lightweight solution to run some dependencies as docker containers. For more info, check [here](https://testcontainers.com/)
  - Specifically, for the payment service I setup containers for Postgres and the `bbyars/mountebank` image. 

# Future Improvements

Below are some potential improvements I purposely left out of the implementation, due to the nature of the 
project been an MVP and limited amount of time, but it would be interesting to explore in the future!

- [ ] Authentication/Authorization of our APIs
- [ ] CI pipeline using Github Actions
- [ ] CD and deployment in AWS or other cloud provider
- [ ] BDD or Behaviour-Driven Testing
- [ ] Idempotency Key header for retryable payments 
- [ ] Better separation of layers (i.e. remove the Shared library)
- [ ] Distributed Caching and replace in-memory (or combine using an hybrid approach)

