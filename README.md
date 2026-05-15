# API Aggregator Project

This project is an API aggregation service developed using **.NET 10**. Its primary purpose is to fetch data from three external sources—Weather, News, and Exchange Rates—and consolidate them into a single, unified response.

The implementation focuses on high availability, performance through parallelism, and reliable error handling to ensure that the service remains functional even if one of the external providers fails.

---

## Technical Features

* **Parallel Processing**: Instead of fetching data sequentially, the service uses **Task.WhenAll** to trigger all external requests at once. This ensures the total response time is determined by the slowest individual API call rather than the sum of all three.
* **Resilience and Fallbacks**: I used the **Polly** library to handle transient failures. If a request fails, the system automatically retries three times. If it still fails, the service returns a graceful fallback message for that specific section of the data, allowing the rest of the response to be delivered to the user.
* **Caching**: To minimize redundant network calls and stay within API rate limits, the service implements in-memory caching. Results are stored for a short duration to speed up subsequent identical requests.
* **Performance Statistics**: The service includes an endpoint that monitors API health. It tracks the total number of requests and calculates average response times, categorizing them into **Performance Buckets**:
    * **Fast**: < 100ms
    * **Average**: 100ms - 200ms
    * **Slow**: > 200ms

---

## API Protection & Rate Limiting

To maintain the stability of the service and prevent abuse of the external API providers, a Rate Limiting mechanism has been implemented.

* **Policy Type**: Fixed Window Limiter.
* **Request Limit**: 10 requests per minute.
* **Queue Management**: A small queue of up to 2 requests is allowed before rejection.
* **Status Code**: If the limit is exceeded, the API returns a **429 (Too Many Requests)** response.

---

## Setup and Configuration

### 1. API Keys
To run this project, you will need your own API keys from:
* **OpenWeatherMap** (Weather data)
* **NewsAPI** (Headlines and articles)
* **ExchangeRate-API** (Currency conversion rates)

### 2. Configuration
The repository includes an appsettings.json template. Insert your keys into the file located in the project root:

"OpenWeather": {
  "ApiKey": ""
},
"News": {
  "ApiKey": ""
},
"ExchangeRates": {
  "ApiKey": ""
}

### 3. Installation
1. Clone the repository to your local machine.
2. Ensure you have the **.NET 10 SDK** installed.
3. Open the solution in Visual Studio 2026 or your preferred IDE.

---

## Running the Application

* **Execution**: Press **F5** or use the command "dotnet run" from the terminal.
* **Interactive Documentation**: The project is configured with **Swagger**. Once the application starts, you can access the UI at the root URL to test the endpoints directly.
    * GET /api/AggregateData: Retrieves the consolidated data.
    * GET /api/AggregateData/stats: Retrieves the in-memory performance metrics.

---

## Unit Testing

I have included unit tests using **xUnit** and **Moq** to verify the core logic of the aggregation service and the accuracy of the statistics calculation. To run the tests, use the following command:

---

## Architecture and Scalability

The project follows a modular architecture using the **Strategy Pattern**. By injecting a collection of IApiClientInterface implementations into the aggregation service, the system is highly scalable. Adding a new external data source requires only the creation of a new client class and its registration in the Dependency Injection container, without any modifications to the existing aggregation logic.