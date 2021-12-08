# 'BackgroundOnce'

[Specflow](https://specflow.org/) and [Gherkin](https://cucumber.io/docs/gherkin/) allow you to specify a [`Background`](https://cucumber.io/docs/gherkin/reference/#background), a series of steps that execute before every scenario.
But what if you this can be executed once, and remain valid for all scenarios, much like the [Nunit OneTimeSetup](https://docs.nunit.org/articles/nunit/writing-tests/attributes/onetimesetup.html) approach.

This doesn't undermine any of the Gherkin spec, nor the way in which Specflow implements it. To all intents and purposes, the steps in the background have executed before each scenario, just once per feature rather than once per scenario.

This project explores such an approach. For now this project is purely experimental and has received no approval nor any affiliation with [Specflow](https://specflow.org/) or [Cucumber/Gherkin](https://cucumber.io/).

### Example
Imagine you specify [the following scenarios](Specflow.BackgroundOnce.Nunit/Features/Readme.feature)

```gherkin
Background:
    Given the database engine is started
    And standard reference data is created

Scenario: Employee details can be retrieved with reference data
    When I request the details of Joe Blogs
    Then I receive the following details
      | Property   | Value                                    |
      | Name       | Joe Blogs                                |
      | Gender     | Male                                     |
      | Department | Human Resources                          |
      | Address    | Dorothy House, Yellow Brick Road, Kansas |

Scenario: Reference data can be updated for all employees
    Given I update the address labeled Head Quarters to
      | Property | Value          |
      | House    | 27             |
      | Street   | Wimpole Street |
      | County   | London         |

    When I request the details of Joe Blogs
    Then I receive the following details
      | Property   | Value                      |
      | Name       | Joe Blogs                  |
      | Gender     | Male                       |
      | Department | Human Resources            |
      | Address    | 27, Wimpole Street, London |
```

There is nothing to say that the steps in the background couldn't be executed once, so long as the state is consistent before the steps in the scenario continue
In doing so we can ensure that any expensive operations happen once and don't need to take (potentially valuable) time or compute again.

### Approach

To achieve this, and be super clear about this atypical behaviour, the following approach is taken

```gherkin
Background:
  Given I execute Background steps1️⃣ only once
  2️⃣

Scenario: Background steps1️⃣
  Given the database engine is started
  And standard reference data is created
  
3️⃣
Scenario: Employee details can be retrieved with reference data
  ...

3️⃣
Scenario: Reference data can be updated for all employees
  ...
```

In the above there are a few things to call out
1. The name of the scenario containing the steps can have any name, it will be executed only once
   1. The steps and what they do are written by you, for your systems
2. A snapshot will be created once the steps in `Background steps` have been executed
3. The snapshot will be restored before any subsequent scenarios execute

To achieve the snapshot creation and restore some classes must be created that implement `ISnapshotData`.
For example:

```csharp
public class DataContext : ISnapshotData
{
  private readonly FeatureContext _featureContext;

  public DataContext(FeatureContext featureContext)
  {
      _featureContext = featureContext;
  }
  
  public Task CreateSnapshot()
  {
      // store the current state of the data in _featureContext
      return Task.CompletedTask;
  }

  public Task RestoreSnapshot()
  {
      // reset the current state of the data to that stored in _featureContext
      return Task.CompletedTask;
  }
}
```
There can be any number of these 'context' objects, any which are created within the invoked scenario will have their methods invoked at the appropriate times.
See [the sample projects in this repository](Specflow.BackgroundOnce.UnitTestCommon/Context/DataContext.cs) for some examples of how this can be implemented.

#### Notes on the implementation:
- **Don't use static objects/fields** not only are they not thread safe but they circumvent dependency injection and encapsulation of snapshots within features
   - Instead store any data/flags on the FeatureContext object, see the samples... 
- The method for finding the scenario to invoke _isn't_ the best, but works so far, there may be unforeseen issues with the approach taken
- The following frameworks are supported:
  - [MSTest](https://docs.specflow.org/projects/specflow/en/latest/Integrations/MsTest.html)
  - [NUnit](https://docs.specflow.org/projects/specflow/en/latest/Integrations/NUnit.html)
  - [xUnit](https://docs.specflow.org/projects/specflow/en/latest/Integrations/xUnit.html) 