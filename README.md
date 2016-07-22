# Summon API Search

This is a C# library for connecting to and querying the Summon Search API.  The API documentation is here:

[http://api.summon.serialssolutions.com/](http://api.summon.serialssolutions.com/)

>**Note:** that this code was developed for an unpaid prototype.  It is incomplete and was only designed to function at the most basic level. Use at your own risk, and understand that this library encompasses maybe 25% of the API's functionality, at most.

> Please submit pull requests for any improvements.

##Usage:

* A `SummonQuery` object is created by passing in the Access ID and Secret Key (as provided to you).
* The search is configured by adding multiple parameters, as listed in the documentation [here](http://api.summon.serialssolutions.com/help/api/search/parameters) (you do not have to add the `s.` prefix for each parameter -- that is added for you).
* The `Execute()` method is called
* The `Documents` property is now populated with a `List<SummonDocument>`

Any combination of parameters can be added, but at the current time, they cannot be duplicated.

## Code Sample

    var query = new SummonQuery("[access ID]", "[secret key]");
	query.AddParameter("q", "[search term]");
    query.Execute();

    foreach(var document in query.Documents)
    {
        Console.Writeline(document.Title);
        Console.Writeline(document.Link);
        Console.Writeline(document.GetField("PublicationYear"));
    }

 