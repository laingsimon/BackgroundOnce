Feature: CascadingBackground

    Background:
        Given I invoke the Background data scenario only once

    Scenario: Background data
        Given there are the following people
          | Name    | Age |
          | Simon   | 30  |
          | James   | 12  |
          | Dorothy | 90  |

        Given I invoke the Address data scenario only once

    Scenario: Address data
        Given there are the following addresses
          | House         | Stret             | County |
          | 17            | Wimpole Street    | London |
          | Dorothy house | Yellow brick road | Kansas |

    Scenario: Minors are excluded
        Then there are 3 people
        Then there are 2 addresses
        Given There are no people under the age of 18
        And There are only addresses in London

        When I request all people

        Then I receive the following people
          | Name    |
          | Simon   |
          | Dorothy |

        When I request all addresses

        Then I receive the following addresses
          | House | Stret          | County |
          | 17    | Wimpole Street | London |

    Scenario: Elderly are excluded
        Then there are 3 people
        Then there are 2 addresses
        Given There are no people over the age of 85
        And There are only addresses in Kansas

        When I request all people

        Then I receive the following people
          | Name  |
          | Simon |
          | James |

        When I request all addresses

        Then I receive the following addresses
          | House         | Stret             | County |
          | Dorothy house | Yellow brick road | Kansas |