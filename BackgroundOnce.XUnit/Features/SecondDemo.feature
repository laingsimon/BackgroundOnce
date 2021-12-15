Feature: SecondDemo

    Background:
        Given I invoke the Background data scenario only once

    Scenario: Background data
        Given there are the following people
          | Name      | Age |
          | George    | 8   |
          | Charles   | 75  |
          | William   | 39  |
          | Elizabeth | 95  |

    Scenario: Minors are excluded
        Then there are 4 people
        Given There are no people under the age of 40

        When I request all people

        Then I receive the following people
          | Name      |
          | Charles   |
          | Elizabeth |

    Scenario: Elderly are excluded
        Then there are 4 people
        Given There are no people over the age of 40

        When I request all people

        Then I receive the following people
          | Name    |
          | George  |
          | William |