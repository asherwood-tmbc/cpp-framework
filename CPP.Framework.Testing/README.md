# CPP.Framework.Testing

Provides helper classes to simplify writing unit and integration tests.

## Features

- **Test Suite Base** — Base class and attributes for organizing tests into named groups
- **Stub Factory** — Rhino Mocks-based factory helpers for creating stubs and mocks
- **Verify Helpers** — Assertion extensions for collections, dictionaries, and dynamic objects
- **Expected Exception Attributes** — `ExpectedArgumentExceptionAttribute`, `ExpectedArgumentNullExceptionAttribute`, and `ExpectedArgumentOutOfRangeException` for declarative exception testing
- **Config Stubs** — Extensions for stubbing configuration settings in tests
- **Method Invocation Extensions** — Rhino Mocks extension allowing a stub to delegate to its original implementation
