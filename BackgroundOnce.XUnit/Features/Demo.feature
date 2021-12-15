Feature: SubScenarioDemo

    Background:
        Given I invoke the Background data scenario only once

    Scenario: Background data
        Given there are the following people
          | Name    | Age |
          | Simon   | 30  |
          | James   | 12  |
          | Dorothy | 90  |

    Scenario: Minors are excluded
        Then there are 3 people
        Given There are no people under the age of 18

        When I request all people

        Then I receive the following people
          | Name    |
          | Simon   |
          | Dorothy |

    Scenario: Elderly are excluded
        Then there are 3 people
        Given There are no people over the age of 85

        When I request all people

        Then I receive the following people
          | Name  |
          | Simon |
          | James |