Feature: EFCoreSqlite

    Background:
        Given I am using the EfCoreSqlite database
        Given I invoke the Background data scenario only once

    Scenario: Background data
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