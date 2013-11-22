C# datatables parser
========================

A C# Serverside parser for the popuplar [jQuery datatables plugin](http://www.datatables.net) originally released by [Zack Owens](http://weblogs.asp.net/zowens/archive/2010/01/19/jquery-datatables-plugin-meets-c.aspx).

The version in this repo features many changes and improvements from the original. 
The most significant change is the shift to returning an array of objects instead of a two dimensional array. 
This allows for binding to the actual object property names instead of array indexes. It also supports string filtering on dates and numberic fields in linq to entities without retrieving the entire list. The project was created in visual studio 2012 and requires .net framework 4.5 to be installed.

This project is provided under the MIT License.

jQuery Datatables
========================

The jQuery Datatables plugin is a very powerful javascript grid plugin which comes with the following features out of the box:

* filtering
* sorting
* paging
* jqQuery ui themeroller support
* plugins  
* Ajax/Remote and local datasource support

Using the Parser
========================

Please see the [official datatables documentation](http://datatables.net/release-datatables/examples/data_sources/server_side.html) for examples on setting it up to connect to a serverside datasource.

Here is a sample asp.net mvc action which uses the parser

            public JsonResult DocumentList()
            {
            	var context = new DocumentsEntities(); //Entity framework context
                IQueriable<Document> documents = context.Documents.where(d => d.IsActive);

            	var parser = new DataTablesParser<Document>(Request, documents);
            	return Json(parser.Parse());
             }


Installation
========================
 
 The easiest way to get this library into your project is through NuGet. You can search using the NuGet package manager, or you
 can enter the following command in your package manager console:
 
            PM> Install-Package DataTablesParser              

Contributions, comments, changes, issues
========================

I welcome any suggestions for improvement, contributions, questions or issues with using this code. 
It is the result of continuous changes to deal with issues encountered while using it so I am sure there are some things that can be 
optimized or removed altogether. One of the biggest things this has forced me to deal with is the differences between LINQ to entites and LINQ to Objects.

* Please do not include multiple unrelated fixes in a single pull request
* The diff for your pull request should only show changes related to your fix/addition (Some editors create unnecessary changes).
* When possible include tests that cover the features/changes in your pull request
* Before you submit make sure the existing tests pass with your changes
* Also, issues that are accompanied by failing tests will probably get handleded quicker

Contact 
========================
Twitter: [garvincasimir](https://twitter.com/garvincasimir)
Google+: [garvincasimir](https://plus.google.com/100137710586918559017)
