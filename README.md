## System Design - *English*

### High-Level Architecture (Mermaid)

```mermaid
flowchart LR
    A[Billing Service] -- publishes/consumes --> R[<b>RabbitMQ</b>]
    B[Wallet Service] -- publishes/consumes --> R
    D[Database Proxy Service] -- SQL --> SS[(SQL Server)]
    D -- uses as outbox --> RE[Redis]
    D -- send integration events for updating read model --> R
    A -- SQL --> SS
    A -- receives events --> R

    subgraph Data Stores
      SS
      PG[(Postgres)]
      MG[(MongoDB)]
      RE[(Redis)]
    end

    B -- stores events --> PG
    B -- reads/writes read model --> MG
