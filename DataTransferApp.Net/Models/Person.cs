namespace DataTransferApp.Net.Models;

public record Person
{
    public string FirstName { get; init; }

    public string LastName { get; init; }

    public string Name => $"{FirstName} {LastName}";

    public string Company { get; init; }

    public string EmployeeID { get; init; }

    public Person(string firstName, string lastName, string company, string employeeID)
    {
        FirstName = firstName;
        LastName = lastName;
        Company = company;
        EmployeeID = employeeID;
    }
}
