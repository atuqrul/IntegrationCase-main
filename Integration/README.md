# AdCreative.ai Integration Case

## Description of the Problem

The project comprises two layers: Service and Backend.

For brevity, the API or presentation layer is excluded from the description.

This represents a typical integration scenario where items are externally sent by a third party to our integration service. The service is called with only the item content, to which we assign internal incremental identifiers, returning them (in text form here) to the third party.

The rule dictates that any item content should be saved only once and not occur twice in our systems. As per the agreement, the third party should wait for the result of their call, which can take a while (simulated as two seconds here, but realistically closer to forty seconds). However, in reality, the third party calls the service multiple times without waiting for a response.

Although protection is in place to check for duplicate items, if called rapidly in parallel (as demonstrated in the main Program), multiple entries with the same content are recorded on our end.

## Required Solution

### 1- Single Server Scenario

**a: Solution Description:**
- Modify the code exclusively within the Service layer (folder) to ensure that the same content is never saved more than once under any circumstances.
- Ensure that items with different content are processed concurrently without waiting for each other.

**b: Implementation:**
- Implement the solution within the Service layer.

**c: Demonstration in Program.cs:**
- Add code to Program.cs to showcase that the implemented solution allows parallel storage of items with different content.

### 2 - Distributed System Scenario

**a: Solution Description:**
- In case of multiple servers containing ItemIntegrationService, implement a solution for the distributed system scenario.

**b: Weaknesses:**
- Identify and describe any weaknesses that the solution might have in a text file.


### Solution Description

# Item Integration Service

## Local Setup
**Note:** Redis has been installed locally for the project. Make sure Redis is running before executing the application to ensure proper functionality.

### Code Organization
The main method is organized using regions for clarity and ease of navigation. Each section is defined as follows:

## 1. Single Server Scenario (SemaphoreSlim):
The **ItemIntegrationService** class solves concurrency issues using **SemaphoreSlim**. 
- **SaveItemAsync Method:** Ensures only one item is saved at a time. If the same content is attempted to be saved simultaneously, the operation is rejected.

## 2. Distributed Architecture (Redis Usage):
The **DistributedItemService** class prevents duplicates in a distributed system using **Redis**.
- **SaveItemAsync Method:** Creates a lock based on the item's content (`lock:{content}`). The item is saved when the lock is acquired, and released upon completion. If the lock fails, the item already exists, and an error is thrown.

### RedLock:
- **RedLockFactory** creates a lock, ensuring only one transaction proceeds.
- **AddItemIfNotExists Method:** Checks if the item already exists in Redis.

### Summary:
- **Single Server:** SemaphoreSlim prevents parallel duplicates.
- **Distributed System (Redis):** Locks ensure items are not saved multiple times by different servers. If a lock acquisition fails, the item is already added.

## Distributed Weaknesses:
- **Locking Challenges:** RedLock may fail due to network delays, causing inconsistent lock behavior.
- **Data Consistency Issues:** In-memory Redis data can be lost during outages.
- **Network Dependency:** Network issues can degrade performance, and Redis can become a single point of failure.
- **Performance Load:** Frequent lock transactions can create bottlenecks.
- **TTL and Time Sync:** Incorrect TTL calculation or time desynchronization may release locks prematurely or delay transactions.
- **Scalability Issues:** High load can overwhelm Redis, reducing performance.
- **Deadlock Risks:** Unreleased locks can cause deadlocks, blocking other transactions.
- **Monitoring Challenges:** Distributed locks are difficult to debug without advanced logging and monitoring.

## Conclusion:
While Redis offers robust distributed locking, challenges such as performance, network dependency, and potential vulnerabilities must be carefully managed.


