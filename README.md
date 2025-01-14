# Payment Gateway - Checkout.com

![img.png](img.png)

This is the .NET version of the Payment Gateway challenge. If you haven't already read this [README.md](https://github.com/cko-recruitment/) on the details of this exercise, please do so now. 

# How to run

To run the project, simply run the docker-compose.yml. It will generate the Payment Gateway API image and run the docker container, alongside the dependencies.
The service will run on `http://localhost:8081`. You can choose your preferred API Client (Postman, cURL, etc), or try out the file `requests.http`. 

Currently it supports two endpoints:

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
Specifically it covers the following:

- A merchant should be able to process a payment through the payment gateway and receive one of the following types of response:
  - **Authorized** - the payment was authorized by the call to the acquiring bank
  - **Declined** - the payment was declined by the call to the acquiring bank
  - **Rejected** - When the bank simulator returns a specific error code (client errors 400, 401, 403, etc.) instead of returning an error to the end-user, we will store the payment as rejected and return this response to the user. 
    - This 


# Non-Functional Requirements

Below is a description of the low-level, technical details of the current project, 
with some justification on the why.

- The service should be 

### Observability
Distributed tracing is critical for applications, especially those based on microservices architectures, where requests often traverse multiple services. 
Thanks to OpenTelemetry, it can provide insight into the flow of requests, and help diagnose performance bottlenecks, errors, and improve overall system reliability.

For this project I decided to use Jaeger as the OTEL exporter. The reason is because is an open-source tool and integrates well into ASP .NET, and since this is 
an MVP project this should cover everything. Currently we have a very simple Observability ecosystem, but as an MVP it works as a proof of concept. 

When running the `docker-compose.yml`, you should be able see the traces of each request under http://localhost:16686. 

### Data Storage



### Testing

Inside the `/test` folder, I setup two testing project: `UnitTest` and `IntegrationTest`. Some description provided below:
- **Unit tests:** 
  - I tried to cover  test the smallest pieces of code in isolation directly. For example, I tested the methods on the extension methods, the mappers, and the validators directly.
  - On this project, I also test the service layer. The reason why is in here and not integration test project, is because I focused on testing the core service logic. The dependencies are directly mocked using Moq and WireMock (for the Simulator API client).
- **Integration tests:** 
  - Aims to ensure that different parts of the application work together as expected.
  - To do so, I tested directly from the Controllers using the `TestContainers` library, a lightweight solution to run some dependencies as docker containers. 
  - For the project I had to setup containers for Postgres and the `bbyars/mountebank` image. 
# Future Improvements

Below are some potential improvements I purposelly left out of the implementation, due to the nature of the 
project and limited amount of time, but it would be interesting to explore!

- [ ] Authentication/Authorization
- [ ] CI pipeline using Github Actions
- [ ] CD of the overall service
- [ ] BDD or Behaviour-Driven Testing
- [ ] Idempotency Key for retriable payments 
- [ ] Better separation of layers (i.e. remove the Shared library)

