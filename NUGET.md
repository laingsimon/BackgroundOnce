# 'BackgroundOnce'

Support once-only execution of [Specflow](https://specflow.org/) steps in the `Background`, similar to [Nunit OneTimeSetup](https://docs.nunit.org/articles/nunit/writing-tests/attributes/onetimesetup.html).

### Example

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

See more details on [GitHub](https://github.com/laingsimon/BackgroundOnce/)
