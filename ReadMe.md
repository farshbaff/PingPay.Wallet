# Wallet API

The Wallet API is a simple RESTful API for managing user transactions.

## Features

- Deposit or withdraw an amount for a user
- Get a list of transactions for a user

## Sample Data

The API uses a Postgres database to store user transactions. Here's an example of how to create the necessary tables and seed some sample data:

```sql
CREATE TABLE transaction (
  id BIGSERIAL PRIMARY KEY,
  wallet_id BIGINT NOT NULL REFERENCES wallet(id),
  transaction_type SMALLINT NOT NULL REFERENCES transaction_type(id),
  amount NUMERIC(20, 8) NOT NULL DEFAULT 0,
  correlation_id UUID NOT NULL,
  description VARCHAR(500),
  created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);
```

```sql
INSERT INTO transaction (wallet_id, transaction_type, amount, correlation_id, description, created_at) VALUES
    (1, 1, 100.00, "9BF5E638-8A70-4195-AF44-4AF9BAEB8D53", 'description', NOW()),
    (1, 2, 50.00, "2198C669-BCC9-4E38-8BAC-65BE29AE160D", 'description', NOW()),
    (2, 1, 500.00, "7DEF6FC3-8F29-4F9B-88A5-192016B70A3F", 'description', NOW()),
    (2, 2, 250.00, "21CD4A30-AA5D-4FFB-A989-A5EF8B611B0A", 'description', NOW());
```

## Deployment
### Prerequisites

- Docker
- Docker Compose

### Build
To build the API and Postgres database Docker images, run the following command in the root directory of the project:

```
docker-compose build
```

### Run
To start the API and Postgres database Docker containers, run the following command:

```
docker-compose up
```

The API will be available at http://localhost:8080.

### Stop
To stop the API and Postgres database Docker containers, press CTRL+C in the terminal where they are running.

### Clean up
To remove the API and Postgres database Docker images and containers, run the following command:

```
docker-compose down --rmi all --volumes --remove-orphans
```

## API Documentation
The API is documented using Swagger. To view the Swagger documentation, go to [http://localhost:2020/swagger/index.html](http://localhost:2020/swagger/index.html) in your web browser.