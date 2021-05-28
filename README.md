# TransactionMyths

This repo shows how to solve concurrency problems in a high load multi user system with SQL transactions and NServiceBus sagas.

## Setup

1. Add the connection string for your MSSQL database in `Shared/Configuration.cs`
2. Run the project `ResetDb` to setup a new database with test data based on your connection string.

## Scenario

Your application is used by many users. Each user belongs to a company.
Some users inside a company can have the role "Admin" to access some additional features like the user management where they can manage the roles of all users inside their own company.
The company has the invariant that it must have **at least one admin**.

Imagine the following scenario:
Currently a company has two admins. Both admins are managing user roles and degrade each other at the exact same time (as it might happen in a high load multi user system).

Depending on how the application is implemented there are three possible results:

1. Everything stays the same and both users are still admins
2. Both users get degraded and the company has no admin anymore
3. One of both admins get degraded and the company has one admin left (order doesn't matter)

In the following examples we try to prevent result two as it violates the invariant. But how?

### Default Transactions

In the first example we try to solve the problem with SQL transactions.

When you run the `ResetDb` project the database reflects the scenario in a simplified manner. It contains a `companies` table with a company and an `users` table containing two users. Both users have the role "Admin" and are in the same company.

In the solution folder `DefaultTransactions` are two projects. Each of the projects simulates a request from the user management by trying to remove the admin status of a user.

For that they call the `RemoveAdmin()` method which creates a SqlConnection and executes sql statements inside of an transaction. First the current amount of admins in the own company gets selected. If it is greater than one the user gets degraded.

Note the `Thread.Sleep()` call through which the transactions overlap which helps to simulate a concurrent execution.

When you run the projects `Transaction1` and `Transaction2` at the same time they both read that there are two admins in the company and therefore degrade the user. As a result no admin is left. That's because the database uses pessimistic concurrency with the IsolationLevel "Read Committed" by default. Therefore a lock gets acquired before reading and released immediately after reading is finished. So both transactions run parallel and read the same admin count of two.

In order to not violate the invariant, the time for how long the lock is acquired, needs to be changed. You can configure it by passing in an `IsolationLevel` to the `.BeginTransaction()` method.
The only two possible IsolationLevels for this scenario are "RepeatableRead" and "Serializable" since both of them acquire the lock from the beginning of the transaction until the end. As a result only one transaction completes while the other one fails because the required records are locked and a ConcurrencyException occurs. The invariant is kept.

When using transactions the only solution for this problem is to lock database records so that only one of the transactions can succeed. For this you have to choose the right IsolationLevel and make sure that the failed transaction gets retried.
Note that other transactions in your application that need to read that same table get blocked too although they have nothing to do with your role management.
That's the reason why you should avoid this solution in high load multi user systems.

In conclusion the technical issue is that you have to read the whole table to check for the invariant and that whole table needs to be locked during the transaction while other transactions get blocked.

### SagaTransaction

The key to solve this issue is to reduce all data which is needed to check the invariant to one database row.

For that we are using a saga from NServiceBus. For understanding: A saga is a handler which can hold a state by persisting data to a database row.
We use it as a state machine which holds the information how many admins are exist in a company. So it receives the request to degrade a user from the api, checks the invariant, sends a new command to really degrade the user if the invariant isn't violated and reduces it's admin count.

The solution folder `SagaTransaction` contains two projects.

The project `Api` simulates the api which receives the degradation request from the client and sends a corresponding command into the bus.

The `Backend` project handles these commands.

Now you can run the `Api` project to put some degradation request messages into the queue. When you are starting the `Backend` project all messages from the queue get handled simultaneously by the saga handler.
Because we configured the NServiceBus endpoint with the transaction mode "SendsWithAtomicReceive" every message has to be handled successfully before it's removed from the input queue and all messages that the handler tries to send are only sent if the handling is successful.
